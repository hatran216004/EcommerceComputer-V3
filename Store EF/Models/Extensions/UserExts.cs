using System;
using System.Linq;

namespace Store_EF.Models.Extensions
{
    public static class UserExts
    {
        public static Payment CurrentPayment(this User_ user)
        {
            if (user == null)
                return null;
            else
            {
                foreach (var order in user.Order_)
                {
                    var find = order.Payments.Where(x => x.Expiry >= DateTime.Now && x.Status == "Waitting");
                    if (find.Count() > 0)
                        return find.First();
                }
                return null;
            }
        }

        public static int TotalCartPrice(this User_ user)
        {
            int result = 0;
            foreach (var cart in user.Carts)
            {
                result += cart.Product.AutoPrice(cart.Quantity);
            }
            return result;
        }
    }
}