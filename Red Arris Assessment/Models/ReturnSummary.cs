namespace Red_Arris_Assessment.Models
{
    public class ReturnSummary
    {
        private DateTime _startDate;
        private DateTime _endDate;
        public string Symbol { get; set; }
        public List<ReturnRecord> ReturnRecords { get; set; }
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
        public ReturnSummary(string symbol, DateTime startDate, DateTime endDate)
        {
            Symbol = symbol;
            _startDate = startDate;
            _endDate = endDate;
            ReturnRecords = new List<ReturnRecord>();
        }
    }
}
