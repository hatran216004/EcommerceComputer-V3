namespace Store_EF.Models.Extensions
{
    public static class OrderDetailExts
    {
        public static int TotalPrice(this OrderDetail detail)
        {
            return detail.Quantity * detail.Price;
        }
    }
}