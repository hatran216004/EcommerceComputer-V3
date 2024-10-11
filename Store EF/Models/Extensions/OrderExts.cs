using System.Linq;

namespace Store_EF.Models.Extensions
{
    public static class OrderExts
    {
        public static int TotalPrice(this Order_ order)
        {
            return order.OrderDetails.Sum(d => d.Price * d.Quantity);
        }
    }
}