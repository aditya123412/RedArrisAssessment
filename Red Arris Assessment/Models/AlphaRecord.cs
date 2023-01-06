namespace Red_Arris_Assessment.Models
{
    public class AlphaRecord
    {
        public DateTime Date { get; set; }
        public double AlphaPercentage { get; set; }
        public AlphaRecord(DateTime date, double alphaPercentage)
        {
            Date = date;
            AlphaPercentage = alphaPercentage;
        }
    }
}
