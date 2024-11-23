namespace Store_EF.Models
{
    public class ViewReport
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalIncome { get; set; }
        public int TotalSold { get; set; }

        public ViewReport() { }
    }
}