namespace Red_Arris_Assessment.Models
{
    public class ReturnRecord
    {
        private DateTime _date;
        public string Date
        {
            get
            {
                return _date.ToString("yyyy-MM-dd");
            }
        }
        public double ReturnAbsolute { get; set; }
        public double ReturnPercentage { get; set; }
        public ReturnRecord(DateTime date, double returnAbsolute, double returnPercentage)
        {
            _date = date;
            ReturnAbsolute = returnAbsolute;
            ReturnPercentage = returnPercentage;
        }
    }
}
