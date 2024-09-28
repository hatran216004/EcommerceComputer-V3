using System.Collections.Generic;
using System.Linq;

namespace Store_EF.Models.Extensions
{
    public static class ProductsExts
    {
        public static int MaxPage(this IEnumerable<Product> products, int pageSize)
        {
            int count = products.Count();
            if (pageSize <= 0 || count == 0)
            {
                return 1;
            }
            else
            {
                return (count % pageSize != 0) ? (count / pageSize) + 1 : count / pageSize;
            }
        }
    }
}