using PagedList;
using Serilog;
using Store_EF.Models;
using Store_EF.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Store_EF.Controllers
{
    public class ProductsController : Controller
    {
        static StoreEntities store = new StoreEntities();

        public ActionResult Index(int page = 1)
        {
            int pageSize = 8;
            if (page < 1)
                page = 1;
            try
            {
                var products = store.Products.Where(x => x.Stock != 0);
                int maxPage = products.ToList().MaxPage(pageSize);
                if (page > maxPage)
                    page = maxPage;
                ViewBag.MaxPage = maxPage;
                return View(products.OrderByDescending(x => x.CreatedAt).ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return View(new List<Product>().ToPagedList(1, pageSize));
            }
        }



        public ActionResult Search(string product, int page = 1)
        {
            int pageSize = 8;
            if (page < 1)
                page = 1;
            try
            {
                var products = store.Products.Where(x => x.Stock != 0).ToList().Where(c => Regex.IsMatch(c.Title.ToLower(), $"({product})"));
                ViewBag.MaxPage = products.MaxPage(pageSize);
                return View("Index", products.OrderByDescending(x => x.CreatedAt).ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return View(new List<Product>().ToPagedList(1, pageSize));
            }
        }

        public ActionResult Detail(int id = 1)
        {
            if (id < 1)
                return RedirectToAction("Index");
            try
            {
                Product p = store.Products.ToList().Where(x => x.ProductId == id).First();
                ViewBag.Galleries = store.Galleries.ToList().Where(x => x.ProductId == p.ProductId);
                return View(p);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Add()
        {
            try
            {
                ViewBag.Categories = store.Categories.ToList();
                ViewBag.Brands = store.Brands.ToList();
                return View();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return RedirectToAction("Index");
            }
        }

        // Updated upstream
        //[HttpPost]
        //public ActionResult Add(Product p, HttpPostedFileBase file)
        //{
        //    if (ModelState.IsValid && Helpers.IsValidImage(file.InputStream))
        //    {
        //        p = store.Products.Add(p);
        //        Gallery g = new Gallery
        //        {
        //            ProductId = p.ProductId,
        //            IsPrimary = true
        //        };
        //        try
        //        {
        //            store.SaveChanges();
        //            g = store.Galleries.Add(g);
        //            store.SaveChanges();
        //            int galleryId = store.Entry(g).GetDatabaseValues().GetValue<int>("GalleryId");
        //            string fName = $"{galleryId}{Path.GetExtension(file.FileName)}";
        //            string path = Path.Combine(Server.MapPath("~"), $"Public\\Imgs\\Products\\{fName}");
        //            if (!Directory.GetParent(path).Exists)
        //                Directory.GetParent(path).Create();
        //            file.SaveAs(path);
        //            g.Thumbnail = fName;
        //            store.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            store.Galleries.Remove(g);
        //            store.Products.Remove(p);
        //            Log.Error(ex.ToString());
        //            return HttpNotFound();
        //        }
        //        return RedirectToAction("Add");
        //    }
        //    else
        //    {
        //        ModelState.AddModelError("Err", "Thêm sản phẩm thất bại");
        //        return Add();
        //    }
        //}



// Stashed changes
        [HttpPost]
        public ActionResult Add(Product p, HttpPostedFileBase thumbnail, IEnumerable<HttpPostedFileBase> galleries = null)
        {
            if (p.IsValid() && Helpers.IsValidImage(thumbnail.InputStream))
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
                    foreach (var item in galleries) {
                        if (item == null) continue;
                        fName = $"{Guid.NewGuid()}{Path.GetExtension(item.FileName)}";
                        Gallery gallery = new Gallery() { 
                            ProductId = productId,
                            Thumbnail = fName
                        };
                        store.Galleries.Add(gallery);
                        item.SaveAs(Path.Combine(Server.MapPath("~"), $"Public\\Imgs\\Products\\{fName}"));
                    }
                    p.CreatedAt = DateTime.Now;
                    store.SaveChanges();
                }
                catch (Exception ex)
                {
                    store.Products.Remove(p);
                    Log.Error(ex.ToString());
                    return HttpNotFound();
                }
                return RedirectToAction("Add");
            }
            else
            {
                ModelState.AddModelError("Err", "Thêm sản phẩm thất bại");
                return Add();
            }
        }

        public ActionResult ProductManagement(int page = 1)
        {
            int pageSize = 8;
            if (page < 1)
                page = 1;
            try
            {
                var products = store.Products.Where(x => x.Stock != 0);
                int maxPage = products.ToList().MaxPage(pageSize);
                if (page > maxPage)
                    page = maxPage;
                ViewBag.MaxPage = maxPage;
                return View(products.OrderByDescending(x => x.CreatedAt).ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return View(new List<Product>().ToPagedList(1, pageSize));
            }
        }

        public ActionResult Delete (int id)
        {
            try
            {
                var product = store.Products.FirstOrDefault(p => p.ProductId == id);

                if (product == null)
                {
                    ModelState.AddModelError("DeleteError", "Đã xảy ra lỗi khi xóa sản phẩm");
                    return RedirectToAction("ProductManagement");
                }
                store.Products.Remove(product);
                store.SaveChanges();

                return RedirectToAction("ProductManagement");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DeleteError", "Đã xảy ra lỗi khi xóa sản phẩm: " + ex.Message);
                return RedirectToAction("ProductManagement");
            }
        }

        public ActionResult Update (int id)
        {
            ViewBag.Categories = store.Categories.ToList();
            ViewBag.Brands = store.Brands.ToList();
            ViewBag.Galleries = store.Galleries.Where(p => p.ProductId == id).ToList();
            var product = store.Products.FirstOrDefault(p => p.ProductId == id);
            
            if (product == null)
            {
                TempData["UpdateError"] = "Không tìm thấy sản phẩm để cập nhật.";
                return RedirectToAction("ProductManagement");
            }
            return View(product);
        }

        [HttpPost]
        public ActionResult Update(Product product, HttpPostedFileBase thumbnailFile, IEnumerable<HttpPostedFileBase> galleries, int[] galleryIds)
        {
            // Cập nhật thông tin sản phẩm trong cơ sở dữ liệu
            int productId;
            if (!product.UpdateInDb(store, out productId))
            {
                return HttpNotFound();
            }

            // Xử lý ảnh thumbnail
            var galleryThumb = store.Galleries.FirstOrDefault(g => g.ProductId == product.ProductId && g.IsPrimary == true);
            if (thumbnailFile != null && thumbnailFile.ContentLength > 0)
            {
                try
                {
                    string fileName = Path.GetFileName(thumbnailFile.FileName);
                    string path = Path.Combine(Server.MapPath("~\\public\\imgs\\products\\"), fileName);

                    // Lưu tệp thumbnail
                    thumbnailFile.SaveAs(path);
                    galleryThumb.Thumbnail = fileName;
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi lưu thumbnail: " + ex.Message;
                    return RedirectToAction("ProductManagement");
                }
            }

            // Xử lý các ảnh gallery
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
                                string fileName = Path.GetFileName(file.FileName);
                                string path = Path.Combine(Server.MapPath("~\\public\\imgs\\products\\"), fileName);

                                file.SaveAs(path);
                                galleryItem.Thumbnail = fileName;
                                galleryItem.IsPrimary = false;
                            }
                            catch (Exception ex)
                            {
                                TempData["ErrorMessage"] = "Lỗi khi lưu ảnh gallery: " + ex.Message;
                                return RedirectToAction("ProductManagement");
                            }
                        }
                    }
                    galleryIndex++;
                }
            }
            try
            {
                store.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi lưu thay đổi vào cơ sở dữ liệu: " + ex.Message;
                return RedirectToAction("ProductManagement");
            }

            TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("ProductManagement");
        }


    }
}