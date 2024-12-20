﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace Store_EF.Models.Extensions
{
    public static class ProductExts
    {
        public static bool IsValid(this Product p)
        {
            try
            {
                if (p.Title.Length >= 3 && p.Price >= 1000 && p.Stock != 0)
                    return true;
                else return false;
            }
            catch
            {
                return false;
            }
        }

        public static string Thumbnail(this Product p)
        {
            StoreEntities store = new StoreEntities();
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

        public static int CountReviews(this Product p)
        {
            SupportEntities support = new SupportEntities();
            return support.CountReviews(p.ProductId);
        }
        public static int DiscountPercentage(this Product p)
        {
            SupportEntities support = new SupportEntities();
            return support.GetProductDiscountPercent(p.ProductId);
        }

        public static double StarAVG(this Product p)
        {
            SupportEntities support = new SupportEntities();
            return support.StarAVG(p.ProductId);
        }

        public static string FormattedPrice(this Product p, bool original = false, int quantity = 1)
        {
            NumberFormatInfo nfi = new CultureInfo("vi-VN", false).NumberFormat;
            if (original || !p.PromoPrice.HasValue)
                return (p.Price * quantity).ToString("C0", nfi).Replace(",", ".");
            else
                return (p.PromoPrice.Value * quantity).ToString("C0", nfi).Replace(",", ".");
        }

        public static int AutoPrice(this Product p, int quantity = 1)
        {
            if (p.PromoPrice.HasValue)
                return p.PromoPrice.Value * quantity;
            else
                return p.Price * quantity;
        }

        public static bool AddToDb(this Product p, StoreEntities store, out int productId)
        {
            p.CreatedAt = p.UpdatedAt = DateTime.Now;
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
                Debug.WriteLine(ex);
                productId = 0;
                return false;
            }
        }

        public static bool UpdateInDb(this Product productUpdated, StoreEntities store, out int productId)
        {

            if (productUpdated == null || store == null)
            {
                productId = 0;
                return false;
            }


            var product = store.Products.FirstOrDefault(p => p.ProductId == productUpdated.ProductId);
            if (product == null)
            {
                productId = 0;
                return false;
            }


            product.Title = productUpdated.Title;
            product.Stock = productUpdated.Stock;
            product.Price = productUpdated.Price;
            product.PromoPrice = productUpdated.PromoPrice;
            product.Description = productUpdated.Description;
            product.UpdatedAt = DateTime.Now;
            product.BrandId = productUpdated.BrandId;
            product.CategoryId = productUpdated.CategoryId;

            try
            {
                store.SaveChanges();
                productId = product.ProductId;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                productId = 0;
                return false;
            }
        }
    }
}