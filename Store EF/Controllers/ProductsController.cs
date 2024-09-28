using PagedList;
using Serilog;
using Store_EF.Models;
using Store_EF.Models.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

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

        [HttpPost]
        public ActionResult Add(Product p, HttpPostedFileBase file)
        {
            if (ModelState.IsValid && Helpers.IsValidImage(file.InputStream))
            {
                int productId;
                if (!p.AddToDb(store, out productId))
                    return HttpNotFound();

                try
                {
                    Gallery g = store.Galleries.Where(x => x.ProductId == productId && x.IsPrimary == true).First();
                    string fName = $"{g.GalleryId}{Path.GetExtension(file.FileName)}";
                    g.Thumbnail = fName;
                    store.SaveChanges();
                    string path = Path.Combine(Server.MapPath("~"), $"Public\\Imgs\\Products\\{fName}");
                    if (!Directory.GetParent(path).Exists)
                        Directory.GetParent(path).Create();
                    file.SaveAs(path);
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
    }
}