namespace Red_Arris_Assessment.Models
{
    public class AlphaSummary
    {
        private DateTime _startDate;
        private DateTime _endDate;
        public string Symbol { get; set; }
        public string BenchMark { get; set; }
        public string StartDate
        {
            get
            {
                return _startDate.ToString("yyyy-MM-dd");
            }
        }
        public string EndDate
        {
            get
            {
                return _endDate.ToString("yyyy-MM-dd");
            }
        }
        public List<AlphaRecord> AlphaRecords { get; set; }

        public AlphaSummary(string symbol, string benchmark, DateTime startDate, DateTime endDate)
        {
            Symbol = symbol;
            BenchMark = benchmark;
            _startDate = startDate;
            _endDate = endDate;
            AlphaRecords = new List<AlphaRecord>();
        }
    }
}
