using System.Collections.Generic;
using System.Linq;

namespace Store_EF.Models.Extensions
{
    public static class ProductsExts
    {
        public static int MaxPage(this IEnumerable<Product> products, int pageSize)
        {
            ;
            if (pageSize <= 0)
            {
                return 0;
            }
            else
            {
                int count = products.Count();
                return (count % pageSize != 0) ? (count / pageSize) + 1 : count / pageSize;
            }
        }
    }
}