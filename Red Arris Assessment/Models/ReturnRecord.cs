namespace Red_Arris_Assessment.Models
{
    public class ReturnRecord
    {
        public DateTime Date { get; set; }
        public double ReturnAbsolute { get; set; }
        public double ReturnPercentage { get; set; }
        public ReturnRecord(DateTime date, double returnAbsolute, double returnPercentage)
        {
            Date = date;
            ReturnAbsolute = returnAbsolute;
            ReturnPercentage = returnPercentage;
        }
    }
}
