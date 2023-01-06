namespace Red_Arris_Assessment.Models
{
    public class AlphaRecord
    {
        private DateTime _date;
        public string Date
        {
            get
            {
                return _date.ToString("yyyy-MM-dd");
            }
        }
        public double AlphaPercentage { get; set; }
        public AlphaRecord(DateTime date,  double alphaPercentage)
        {
            _date = date;
            AlphaPercentage = alphaPercentage;
        }
    }
}
