namespace Red_Arris_Assessment.Models
{
    public class AlphaRecord
    {
        private DateTime _date;
        private double _alpha;
        public string Date
        {
            get
            {
                return _date.ToString("yyyy-MM-dd");
            }
        }
        public string AlphaPercentage
        {
            get
            {
                return $"{_alpha:0.##}%";
            }
        }
        public AlphaRecord(DateTime date, double alphaPercentage)
        {
            _date = date;
            _alpha = alphaPercentage;
        }
    }
}
