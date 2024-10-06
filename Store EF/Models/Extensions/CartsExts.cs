using System.Collections.Generic;

namespace Store_EF.Models.Extensions
{
    public static class CartsExts
    {
        public static int TotalPrice(this IEnumerable<Cart> carts, StoreEntities store) {
            int result = 0;
            foreach (var cart in carts) {
                result += cart.Product.AutoPrice(cart.Quantity);
            }
            return result;
        }
    }
}