namespace Red_Arris_Assessment.Models
{
    public class AlphaSummary
    {
        public string Symbol { get; set; }
        public string BenchMark { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<AlphaRecord> AlphaRecords { get; set; }

        public AlphaSummary(string symbol, string benchmark, DateTime startDate, DateTime endDate)
        {
            Symbol = symbol;
            BenchMark = benchmark;
            StartDate = startDate;
            EndDate = endDate;
            AlphaRecords = new List<AlphaRecord>();
        }
    }
}
