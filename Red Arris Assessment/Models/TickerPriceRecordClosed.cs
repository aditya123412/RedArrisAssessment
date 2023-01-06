namespace Red_Arris_Assessment.Models
{
    public class TickerPriceRecordClosed
    {
        public string date { get; set; }
        public double close { get; set; }
        public int volume { get; set; }
        public double change { get; set; }
        public double changePercent { get; set; }
        public double changeOverTime { get; set; }

        public DateTime Date
        {
            get
            {
                try
                {
                    return DateTime.Parse(date);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }

}
