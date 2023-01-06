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
        HttpClient _client;
        public TickerPriceFetcherService(string baseUrl, string token, Settings settings)
        {
            this.baseUrl = baseUrl;
            this.token = token;
            this.maxDaysRange = settings.MaxDaysRange;
            this._client = new HttpClient();
        }
        public async Task<ReturnSummary> GetReturn(string symbol, DateTime startDate, DateTime endDate)
        {
            var (normalizedStartDate, NormalizedEndDate) = CheckDateRange(startDate, endDate);

            var tickers = await GetTickerPriceRecordsWithOnePreviousRecordAsync(symbol, normalizedStartDate, NormalizedEndDate);
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

            var tickers = await GetTickerPriceRecordsWithOnePreviousRecordAsync(symbol, normalizedStartDate, NormalizedEndDate);
            var benchMarks = await GetTickerPriceRecordsWithOnePreviousRecordAsync(benchMarkSymbol, normalizedStartDate, NormalizedEndDate);

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


        // We pull range of data from a day before the start of the range to calculate the alpha
        // There is an edge case when the previous day falls on a market
        // holiday and no ticker data exists for that day and the range still starts from the current day,
        // so we pull in a slightly longer range and pick the newest record thats only older than the start date.
        private async Task<List<TickerPriceRecord>> GetTickerPriceRecordsWithOnePreviousRecordAsync(string symbol, DateTime startDate, DateTime endDate)
        {
            List<TickerPriceRecord> tickerPriceRecords = new List<TickerPriceRecord>();

            if (startDate.Year == endDate.Year)
            {
                tickerPriceRecords = await GetTickerPriceForYear(symbol, startDate.Year);
            }
            else
            {
                // Because I have selected 365 days as the max range, the range would span across a maximum of 2 years
                tickerPriceRecords = await GetTickerPriceForYear(symbol, startDate.Year);
                tickerPriceRecords.AddRange(await GetTickerPriceForYear(symbol, endDate.Year));
            }

            var prevRecord = await GetPreviousDayRecord(symbol,startDate);
            List<TickerPriceRecord> results = new List<TickerPriceRecord>();
            results.Add(prevRecord);
            results.AddRange(tickerPriceRecords.Where(x => x.Date >= startDate && x.Date <= endDate));
            return results;
        }
        private async Task<List<TickerPriceRecord>> GetTickerPriceForYear(string symbol, int forYear)
        {
            string uri = $"{baseUrl}/stock/{symbol}/chart/1y?token={token}&chartByDay=true&range={forYear}";


            List<TickerPriceRecord> tickerPriceRecords = new List<TickerPriceRecord>();
            string result = "";
            try
            {
                using (var response = await _client.GetAsync(uri))
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
                    throw new Exception($"The supplied symbol [{symbol}] is not a recognized symbol.", e);
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

        private async Task<TickerPriceRecord> GetPreviousDayRecord(string symbol, DateTime date)
        {
            bool recordFound = false;
            DateTime _date = date;

            try
            {
                // Brute forcing through this as querying for just the symbol name
                // will default to 2023-01-01 as the start date, which means the previous date falls
                // on previous year's 2022-12-31, which for argument is not a trading day, so we step back
                // by one more day to fetch 2022-12-30 record for it. So not fetching by a range but
                // going back day-by-day
                while (!recordFound)
                {
                    _date = _date.AddDays(-1);
                    var uri = $"{baseUrl}/stock/{symbol}/chart/date/{_date.ToString("yyyyMMdd")}?token={token}&chartByDay=true";

                    using (var response = await _client.GetAsync(uri))
                    using (Stream stream = response.Content.ReadAsStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var result = await reader.ReadToEndAsync();
                        var values = JsonSerializer.Deserialize<List<TickerPriceRecord>>(result);
                        if (values.Count > 0)
                        {
                            return values[0];
                        }
                    }
                }
                return null;    //We shouldn't end up here
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
