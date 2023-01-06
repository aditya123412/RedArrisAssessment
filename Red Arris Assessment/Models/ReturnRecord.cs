namespace Red_Arris_Assessment.Models
{
    public class ReturnRecord
    {
        private DateTime _date;
        private double _returnPercentage;
        public string Date
        {
            get
            {
                return _date.ToString("yyyy-MM-dd");
            }
        }
        public double ReturnAbsolute { get; set; }
        public string ReturnPercentage
        {
            get
            {
                return $"{_returnPercentage:0.##}%";
            }
        }
        public ReturnRecord(DateTime date, double returnAbsolute, double returnPercentage)
        {
            _date = date;
            ReturnAbsolute = returnAbsolute;
            _returnPercentage = returnPercentage;
        }
    }
}
