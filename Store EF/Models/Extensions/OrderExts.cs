﻿using System.Linq;

namespace Store_EF.Models.Extensions
{
    public static class OrderExts
    {
        public static int TotalPrice(this Order order)
        {
            return order.OrderDetails.Sum(d => d.Price * d.Quantity);
        }

        public static int TotalQuantity(this Order order)
        {
            return order.OrderDetails.Sum(d => d.Quantity);
        }
    }
}