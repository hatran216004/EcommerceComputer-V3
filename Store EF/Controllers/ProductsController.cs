using PagedList;
using Store_EF.Models;
using Store_EF.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class ProductsController : Controller
    {
        StoreEntities store = new StoreEntities();

        public ActionResult Index(int page = 1, string category = "", string brand = "")
        {
            if (Session["UserId"] != null)
            {
                int userId = int.Parse(Session["UserId"].ToString());
                User user = store.Users.First(x => x.UserId == userId);
                if (!user.IsConfirm)
                    return RedirectToAction("Verify", "Home");
            }
            int pageSize = 8;
            if (page < 1)
                page = 1;
            try
            {
                var products = store.Products.Where(x => x.Stock != 0);
                if (category.Length != 0)
                    products = products.Where(x => x.Category.Name.Equals(category, StringComparison.OrdinalIgnoreCase));
                if (brand.Length != 0)
                    products = products.Where(x => x.Brand.Name.Equals(brand, StringComparison.OrdinalIgnoreCase));
                int maxPage = products.ToList().MaxPage(pageSize);
                if (page > maxPage)
                    page = maxPage;
                ViewBag.MaxPage = maxPage;
                return View(products.OrderByDescending(x => x.CreatedAt).ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return View(new List<Product>().ToPagedList(1, pageSize));
            }
        }

        public ActionResult Search(string product = "", int page = 1)
        {
            if (product.Length == 0)
                return RedirectToAction("Index");
            if (Session["UserId"] != null)
            {
                int userId = int.Parse(Session["UserId"].ToString());
                User user = store.Users.First(x => x.UserId == userId);
                if (!user.IsConfirm)
                    return RedirectToAction("Verify", "Home");
            }
            int pageSize = 8;
            if (page < 1)
                page = 1;
            try
            {
                var products = store.Products.Where(x => x.Stock != 0).ToList().Where(c => c.Title.ToLower().Contains(product.ToLower()));
                ViewBag.MaxPage = products.MaxPage(pageSize);
                return View("Index", products.OrderByDescending(x => x.CreatedAt).ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return View("Index", new List<Product>().ToPagedList(1, pageSize));
            }
        }

        public ActionResult Detail(int? id)
        {
            if (Session["UserId"] != null)
            {
                int userId = int.Parse(Session["UserId"].ToString());
                User user = store.Users.First(x => x.UserId == userId);
                if (!user.IsConfirm)
                    return RedirectToAction("Verify", "Home");
            }
            if (!id.HasValue)
                return RedirectToAction("Index");
            try
            {
                Product p = store.Products.Where(x => x.ProductId == id).FirstOrDefault();
                if (p == null)
                    return RedirectToAction("Index");
                ViewBag.Galleries = store.Galleries.Where(x => x.ProductId == p.ProductId);
                ViewBag.Similar = store.Products.Where(x => x.CategoryId == p.CategoryId).ToList().OrderByDescending(x => x.DiscountPercentage()).Take(7);
                return View(p);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Add()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            if (!Helpers.IsUserAdmin(userId, store))
                return RedirectToAction("Index");
            try
            {
                ViewBag.Categories = store.Categories.ToList();
                ViewBag.Brands = store.Brands.ToList();
                return View();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return RedirectToAction("Index");
            }
        }

        // Stashed changes
        [HttpPost]
        public ActionResult Add(Product p, HttpPostedFileBase thumbnail, IEnumerable<HttpPostedFileBase> galleries = null)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            if (!Helpers.IsUserAdmin(userId, store))
                return RedirectToAction("Index");
            if (p.IsValid() && Helpers.IsValidImage(thumbnail))
            {
                int productId;
                if (!p.AddToDb(store, out productId))
                    return HttpNotFound();

                try
                {
                    Gallery g = store.Galleries.Where(x => x.ProductId == productId && x.IsPrimary == true).First();
                    string fName = $"{Guid.NewGuid()}{Path.GetExtension(thumbnail.FileName)}";
                    g.Thumbnail = fName;
                    string path = Path.Combine(Server.MapPath("~"), $"Public\\Imgs\\Products\\{fName}");
                    if (!Directory.GetParent(path).Exists)
                        Directory.GetParent(path).Create();
                    thumbnail.SaveAs(path);
                    foreach (var item in galleries)
                    {
                        if (item == null) continue;
                        fName = $"{Guid.NewGuid()}{Path.GetExtension(item.FileName)}";
                        Gallery gallery = new Gallery()
                        {
                            ProductId = productId,
                            Thumbnail = fName
                        };
                        store.Galleries.Add(gallery);
                        item.SaveAs(Path.Combine(Server.MapPath("~"), $"Public\\Imgs\\Products\\{fName}"));
                    }
                    store.SaveChanges();
                }
                catch (Exception ex)
                {
                    store.Products.Remove(p);
                    Debug.WriteLine(ex);
                    return HttpNotFound();
                }
                return RedirectToAction("ProductManagement");
            }
            else
            {
                ModelState.AddModelError("Err", "Add failed product");
                return Add();
            }
        }

        public ActionResult ProductManagement(int page = 1)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            if (!Helpers.IsUserAdmin(userId, store))
                return RedirectToAction("Index");
            int pageSize = 8;
            if (page < 1)
                page = 1;
            try
            {
                var products = store.Products;
                int maxPage = products.ToList().MaxPage(pageSize);
                if (page > maxPage)
                    page = maxPage;
                ViewBag.MaxPage = maxPage;
                return View(products.OrderBy(x => x.Stock).ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return View(new List<Product>().ToPagedList(1, pageSize));
            }
        }

        public ActionResult Delete(int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            if (!Helpers.IsUserAdmin(userId, store))
                return RedirectToAction("Index");
            try
            {
                var product = store.Products.FirstOrDefault(p => p.ProductId == id);

                if (product == null)
                {
                    ModelState.AddModelError("DeleteError", "An error occurred while deleting the product.");
                    return RedirectToAction("ProductManagement");
                }
                store.Products.Remove(product);
                store.SaveChanges();

                return RedirectToAction("ProductManagement");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DeleteError", "An error occurred while deleting the product.: " + ex.Message);
                return RedirectToAction("ProductManagement");
            }
        }

        public ActionResult Update(int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            if (!Helpers.IsUserAdmin(userId, store))
                return RedirectToAction("Index");
            ViewBag.Categories = store.Categories.ToList();
            ViewBag.Brands = store.Brands.ToList();
            ViewBag.Galleries = store.Galleries.Where(p => p.ProductId == id).ToList();
            var product = store.Products.FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                TempData["UpdateError"] = "No products found to update.";
                return RedirectToAction("ProductManagement");
            }
            return View(product);
        }

        [HttpPost]
        public ActionResult Update(Product product, HttpPostedFileBase thumbnailFile, IEnumerable<HttpPostedFileBase> galleries, int[] galleryIds, IEnumerable<HttpPostedFileBase> newGalleries = null)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            if (!Helpers.IsUserAdmin(userId, store))
                return RedirectToAction("Index");
            // Cập nhật thông tin sản phẩm trong cơ sở dữ liệu
            int productId;
            if (!product.UpdateInDb(store, out productId))
            {
                return HttpNotFound();
            }

            // Xử lý ảnh thumbnail
            var galleryThumb = store.Galleries.FirstOrDefault(g => g.ProductId == product.ProductId && g.IsPrimary == true);

            // Nếu không tìm thấy thumbnail của sp
            if (galleryThumb == null)
            {
                galleryThumb = new Gallery
                {
                    ProductId = product.ProductId,
                    IsPrimary = true
                };

                store.Galleries.Add(galleryThumb);
            }
            if (thumbnailFile != null && thumbnailFile.ContentLength > 0)
            {
                try
                {
                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(thumbnailFile.FileName)}";
                    string path = Path.Combine(Server.MapPath("~\\public\\imgs\\products\\"), fileName);

                    // Lưu tệp thumbnail
                    thumbnailFile.SaveAs(path);
                    galleryThumb.Thumbnail = fileName;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error saving thumbnail: " + ex.Message;
                    return RedirectToAction("ProductManagement");
                }
            }

            // Xử lý các ảnh gallery đã có
            if (galleryIds != null && galleries != null)
            {
                int galleryIndex = 0;
                foreach (int galleryId in galleryIds)
                {
                    var galleryItem = store.Galleries.FirstOrDefault(g => g.ProductId == product.ProductId && g.GalleryId == galleryId && !g.IsPrimary);
                    if (galleryItem != null && galleries.ElementAtOrDefault(galleryIndex) != null)
                    {
                        var file = galleries.ElementAt(galleryIndex);
                        if (file != null && file.ContentLength > 0)
                        {
                            try
                            {
                                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                                string path = Path.Combine(Server.MapPath("~\\public\\imgs\\products\\"), fileName);

                                file.SaveAs(path);
                                galleryItem.Thumbnail = fileName;
                            }
                            catch (Exception ex)
                            {
                                TempData["ErrorMessage"] = "Error saving gallery image: " + ex.Message;
                                return RedirectToAction("ProductManagement");
                            }
                        }
                    }
                    galleryIndex++;
                }
            }

            // Xử lý các ảnh mới
            if (newGalleries != null)
            {
                foreach (var newGallery in newGalleries)
                {
                    if (newGallery != null && newGallery.ContentLength > 0)
                    {
                        try
                        {
                            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(newGallery.FileName)}";
                            string path = Path.Combine(Server.MapPath("~\\public\\imgs\\products\\"), fileName);

                            newGallery.SaveAs(path);

                            // Tạo và thêm một gallery mới cho ảnh này
                            var newGalleryItem = new Gallery
                            {
                                ProductId = productId,
                                Thumbnail = fileName,
                                IsPrimary = false // Đánh dấu là ảnh phụ
                            };
                            store.Galleries.Add(newGalleryItem);
                        }
                        catch (Exception ex)
                        {
                            TempData["ErrorMessage"] = "Error saving new image: " + ex.Message;
                            return RedirectToAction("ProductManagement");
                        }
                    }
                }
            }

            try
            {
                store.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error saving changes to database: " + ex.Message;
                return RedirectToAction("ProductManagement");
            }

            TempData["SuccessMessage"] = "Product updated successfully!";
            return RedirectToAction("ProductManagement");
        }


        public ActionResult UpdatePromo()
        {
            ViewBag.Categories = store.Categories.ToList();
            ViewBag.Brands = store.Brands.ToList();
            ViewBag.Products = store.Products.ToList();
            return View();
        }

        [HttpPost]
        public ActionResult UpdatePromo(Nullable<int> brandID, Nullable<int> categoryID, Nullable<int> percent, Nullable<int> price)
        {
            try
            {
                int rowsAffected = store.UpdatePromoPrice(brandID, categoryID, percent, price);

                if (rowsAffected > 0)
                {
                    TempData["SuccessMessage"] = "Successfully updated promotional prices for " + rowsAffected + " products";
                }
                else
                {
                    TempData["SuccessMessage"] = "No products have been updated. ";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating promotional price: " + ex.Message;
            }

            return RedirectToAction("UpdatePromo", "Products");
        }

    }
}