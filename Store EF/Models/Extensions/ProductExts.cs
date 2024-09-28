using System;
using System.IO;
using System.Linq;
using System.Web;

namespace Store_EF.Models.Extensions
{
    public static class ProductExts
    {
        static StoreEntities store = new StoreEntities();

        public static string Thumbnail(this Product p)
        {
            try
            {
                var primaryGallery = store.Galleries.FirstOrDefault(x => x.IsPrimary == true);

                // Kiểm tra nếu primaryGallery không phải là null
                if (primaryGallery != null)
                {
                    string thumbnail = primaryGallery.Thumbnail;
                    string thumbnailPath = Path.Combine(HttpContext.Current.Server.MapPath("~"), "Public/Imgs/Products", thumbnail);

                    if (File.Exists(thumbnailPath))
                        return thumbnail;
                }

                // Trả về hình ảnh mặc định nếu không tìm thấy thumbnail
                return "null.png";
            }
            catch
            {
                return "null.png"; // Trả về hình ảnh mặc định nếu có ngoại lệ
            }
        }


        public static double DiscountPercentage(this Product p)
        {
            if (p.PromoPrice.HasValue)
                return Math.Round((double)(p.Price - p.PromoPrice.Value) / p.Price * 100);
            else
                return 0;
        }

        public static string FormattedPrice(this Product p, bool original = false)
        {
            if (original)
            {
                return p.Price.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
            }

            if (p.PromoPrice.HasValue)
                return p.PromoPrice.Value.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
            else
                return p.Price.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
        }
    }
}