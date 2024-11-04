using PagedList;
using Serilog;
using Store_EF.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class BrandsController : Controller
    {
        StoreEntities store = new StoreEntities();

        public ActionResult Index(int page = 1)
        {
            int pageSize = 9;
            if (page < 1)
                page = 1;
            try
            {
                var brands = store.Brands.ToList();
                int maxPage = (int)Math.Ceiling((double)brands.Count / pageSize); ;
                if (page > maxPage)
                    page = maxPage;
                ViewBag.MaxPage = maxPage;
                return View(brands.OrderBy(x => x.BrandId).ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return View();
            }
        }


        [HttpPost]
        public ActionResult Add(string name)
        {
            if (ModelState.IsValid)
            {
                Brand tmp = store.Brands.FirstOrDefault(t => t.Name.ToUpper() == name.ToUpper());
                if (tmp == null)
                {
                    try
                    {
                        Brand c = new Brand
                        {
                            Name = name
                        };
                        store.Brands.Add(c);
                        store.SaveChanges();
                        TempData["SuccessMessage"] = "Brand added successfully!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["FailMessage"] = "Brand already exists!";
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }


        public ActionResult Detele(int id)
        {
            var products = store.Products.Where(t => t.CategoryId == id);
            if (products.Count() == 0)
            {
                try
                {
                    Brand brand = store.Brands.FirstOrDefault(t => t.BrandId == id);
                    store.Brands.Remove(brand);
                    store.SaveChanges();
                    TempData["SuccesMessage"] = "Brand deleted successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    return RedirectToAction("Index");
                }

            }
            else
            {
                TempData["FailMessage"] = "Brand is being used with the product!";
                return RedirectToAction("Index");
            }
        }
        public ActionResult Update(int id)
        {
            Brand brand = store.Brands.FirstOrDefault(t => t.BrandId == id);
            ViewBag.BrandFind = brand;
            return View("Update");
        }
        [HttpPost]
        public ActionResult Update(Brand tmp)
        {
            Brand brand = store.Brands.FirstOrDefault(t => t.BrandId == tmp.BrandId);
            brand.Name = tmp.Name;
            store.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}