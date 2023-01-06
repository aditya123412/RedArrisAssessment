using Microsoft.AspNetCore.Mvc;
using Red_Arris_Assessment.Models;
using Red_Arris_Assessment.Services;

namespace Red_Arris_Assessment.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CommoditySummaryController : ControllerBase
    {
        private TickerPriceFetcherService _tickerPriceFetcher;

        public CommoditySummaryController( TickerPriceFetcherService tickerPriceFetcherService)
        {
            _tickerPriceFetcher = tickerPriceFetcherService;
        }

        [HttpGet("~/api/GetReturns/{symbol}")]
        public async Task<ReturnSummary> GetReturns(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return await _tickerPriceFetcher.GetReturn(symbol, startDate, endDate);
        }

        [HttpGet("~/api/GetAlpha/{symbol}/{benchMark}")]
        public async Task<AlphaSummary> GetAlpha(string symbol, string benchMark, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return await _tickerPriceFetcher.GetAlpha(symbol, benchMark, startDate, endDate);
        }
    }
}