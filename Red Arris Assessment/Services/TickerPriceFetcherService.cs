using Red_Arris_Assessment.Controllers;
using Red_Arris_Assessment.Models;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;

namespace Red_Arris_Assessment.Services
{
    public class TickerPriceFetcherService
    {
        private readonly string baseUrl;
        private readonly string token;
        private readonly int maxDaysRange;
        public TickerPriceFetcherService(string baseUrl, string token, Settings settings)
        {
            this.baseUrl = baseUrl;
            this.token = token;
            this.maxDaysRange = settings.MaxDaysRange;
        }
        public async Task<ReturnSummary> GetReturn(string symbol, DateTime startDate, DateTime endDate)
        {
            var (normalizedStartDate, NormalizedEndDate) = CheckDateRange(startDate, endDate);

            // We pull range of data from a day before the start of the range to calculate the return
            // There is an edge case that this fails for that when the previous day falls on a market holiday and no ticker data exists for that day
            var tickers = await GetTickerPriceRecordClosedAsync(symbol, normalizedStartDate.AddDays(-1), NormalizedEndDate);
            var result = new ReturnSummary(symbol, normalizedStartDate, NormalizedEndDate);

            //Since our range starts a day earlier than the requested range, start from the second item in the range
            for (int i = 1; i < tickers.Count; i++)
            {
                var dayReturn = tickers[i].close - tickers[i - 1].close;
                result.ReturnRecords.Add(new ReturnRecord(tickers[i].Date, dayReturn, dayReturn / tickers[i].open));
            }
            return result;
        }

        public async Task<AlphaSummary> GetAlpha(string symbol, string benchMarkSymbol, DateTime startDate, DateTime endDate)
        {
            var (normalizedStartDate, NormalizedEndDate) = CheckDateRange(startDate, endDate);
            var result = new AlphaSummary(symbol, benchMarkSymbol, normalizedStartDate, NormalizedEndDate);

            // We pull range of data from a day before the start of the range to calculate the alpha
            // There is an edge case that this fails for that when the previous day falls on a market holiday and no ticker data exists for that day
            var tickers = await GetTickerPriceRecordClosedAsync(symbol, normalizedStartDate.AddDays(-1), NormalizedEndDate);
            var benchMarks = await GetTickerPriceRecordClosedAsync(benchMarkSymbol, normalizedStartDate.AddDays(-1), NormalizedEndDate);

            // Check for some edge cases, although I don't expect encountering this
            if (tickers.Count != benchMarks.Count)
            {
                throw new Exception($"The number of data points for the selected symbol and benchmark data points do not match.");
            }
            if (!tickers.All(ticker => benchMarks.Any(benchmark => ticker.Date == benchmark.Date)))
            {
                throw new Exception($"The referenced range has missing benchmark data.");
            }

            //Since our range starts a day earlier than the requested range, start from the second item in the range
            for (int i = 1; i < tickers.Count; i++)
            {
                var percentageDayReturn = (tickers[i].close - tickers[i - 1].close) / tickers[i].open;
                var percentageBenchmarkReturn = (benchMarks[i].close - benchMarks[i - 1].close) / benchMarks[i].open;
                result.AlphaRecords.Add(new AlphaRecord(tickers[i].Date, percentageDayReturn - percentageBenchmarkReturn));
            }
            return result;
        }

        private async Task<List<TickerPriceRecord>> GetTickerPriceRecordClosedAsync(string symbol, DateTime startDate, DateTime endDate)
        {
            List<TickerPriceRecord> tickerPriceRecords = new List<TickerPriceRecord>();

            //https://cloud.iexapis.com/v1/stock/aapl/chart/6m?token=pk_035d50c28f334229a87bbc91c2c5374b&chartLast=299&chartByDay=true&chartCloseOnly=true&range=2020
            if (startDate.Year == endDate.Year)
            {
                tickerPriceRecords = await GetTickerPriceForYear(symbol, startDate.Year);
            }
            else
            {
                tickerPriceRecords = await GetTickerPriceForYear(symbol, startDate.Year);
                tickerPriceRecords.AddRange(await GetTickerPriceForYear(symbol, endDate.Year));
            }
            return tickerPriceRecords.Where(x => x.Date >= startDate && x.Date <= endDate).ToList();
        }
        private async Task<List<TickerPriceRecord>> GetTickerPriceForYear(string symbol, int forYear)
        {
            string uri = $"{baseUrl}/stock/{symbol}/chart/1y?token={token}&chartLast=365&chartByDay=true&range={forYear}";
            var client = new HttpClient();

            List<TickerPriceRecord> tickerPriceRecords = new List<TickerPriceRecord>();
            string result = "";
            try
            {
                using (var response = await client.GetAsync(uri))
                using (Stream stream = response.Content.ReadAsStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = await reader.ReadToEndAsync();
                    tickerPriceRecords.AddRange(JsonSerializer.Deserialize<List<TickerPriceRecord>>(result));
                }
            }
            catch (Exception e)
            {
                //This error just comes as a string, not as a valid json or empty array.
                if (result.Equals("Unknown symbol"))
                {
                    throw new Exception($"The supplied symbol [{symbol}] is not a recognized symbol.");
                }
                throw;
            }

            return tickerPriceRecords;
        }
        private (DateTime, DateTime) CheckDateRange(DateTime startDate, DateTime endDate)
        {
            DateTime normalizedStartDate = startDate, normalizedEndDate = endDate;

            if (startDate == DateTime.MinValue && endDate == DateTime.MinValue)
            {
                // No dates supplied, so we default to YTD for this year
                normalizedEndDate = DateTime.Now;
                normalizedStartDate = new DateTime(normalizedEndDate.Year, 1, 1);

                return (normalizedStartDate, normalizedEndDate);
            }
            if (endDate == DateTime.MinValue)
            {
                // No end date provided, so taking end of start date's year as range end.
                // If the start date is in current year, then the range will automatically be ending in today
                normalizedEndDate = new DateTime(startDate.Year, 12, 31);
                return (normalizedStartDate, normalizedEndDate);
            }
            if (startDate == DateTime.MinValue)
            {
                //Start date not provided
                normalizedStartDate = new DateTime(endDate.Year, 1, 1);
                return (normalizedStartDate, normalizedEndDate);
            }

            if (startDate > DateTime.Now || endDate > DateTime.Now)
            {
                throw new Exception($"The date range cannot extend into future.");
            }
            if (startDate > endDate)
            {
                throw new Exception($"The start date of the range cannot be greater than the end date.");
            }
            if ((endDate - startDate).TotalDays > maxDaysRange)
            {
                throw new Exception($"The range of days for calculation cannot be greater than 365");
            }
            //Everyting seems up to scratch, return the dates as is
            return (normalizedStartDate, normalizedEndDate);
        }
    }
}
