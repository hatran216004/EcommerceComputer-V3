using Serilog;
using System;
using System.Globalization;
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
                var primaryGallery = store.Galleries.FirstOrDefault(x => x.IsPrimary == true && x.ProductId == p.ProductId);

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

        public static string FormattedPrice(this Product p, bool original = false, int quantity = 1)
        {
            NumberFormatInfo nfi = new CultureInfo("vi-VN", false).NumberFormat;
            if (original || !p.PromoPrice.HasValue)
                return (p.Price * quantity).ToString("C0", nfi).Replace(",", ".");
            else
                return (p.PromoPrice.Value * quantity).ToString("C0", nfi).Replace(",", ".");
        }

        public static int AutoPrice(this Product p, int quantity = 1) { 
            if (p.PromoPrice.HasValue)
                return p.PromoPrice.Value * quantity;
            else
                return p.Price * quantity;
        }

        public static bool AddToDb(this Product p, StoreEntities store, out int productId)
        {
            if (p == null)
            {
                productId = 0;
                return false;
            }
            store.Products.Add(p);
            try
            {
                store.SaveChanges();
                productId = store.Entry(p).GetDatabaseValues().GetValue<int>("ProductId");
                return true;
            }
            catch (Exception ex)
            {
                store.Products.Remove(p);
                Log.Error(ex.ToString());
                productId = 0;
                return false;
            }
        }
    }
}