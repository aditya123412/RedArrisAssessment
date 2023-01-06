namespace Red_Arris_Assessment.Models
{
    public class ReturnSummary
    {
        public string Symbol { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ReturnRecord> ReturnRecords { get; set; }

        public ReturnSummary(string symbol, DateTime startDate, DateTime endDate)
        {
            Symbol = symbol;
            StartDate = startDate;
            EndDate = endDate;
            ReturnRecords = new List<ReturnRecord>();
        }
    }
}
