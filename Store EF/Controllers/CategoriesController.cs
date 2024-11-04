using PagedList;
using Serilog;
using Store_EF.Models;
using Store_EF.Models.Extensions;
using System;
using System.Drawing.Printing;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Store_EF.Controllers
{
    public class CategoriesController : Controller
    {
        StoreEntities store = new StoreEntities();

        public ActionResult Index(int page = 1)
        {
            int pageSize = 9;
            if (page < 1)
                page = 1;
            try
            {
                var categories = store.Categories.ToList();
                int maxPage = (int)Math.Ceiling((double)categories.Count / pageSize); ;
                if (page > maxPage)
                    page = maxPage;
                ViewBag.MaxPage = maxPage;
                return View(categories.OrderBy(x => x.CategoryId).ToPagedList(page, pageSize));
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
                Category tmp = store.Categories.FirstOrDefault(t => t.Name.ToUpper() == name.ToUpper());
                if (tmp == null)
                {
                    try
                    {
                        Category c = new Category
                        {
                            Name = name
                        };
                        store.Categories.Add(c);
                        store.SaveChanges();
                        TempData["SuccessMessage"] = "Category added successfully!";
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
                    TempData["FailMessage"] = "Category already exists!";
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
                    Category cate = store.Categories.FirstOrDefault(t => t.CategoryId == id);
                    store.Categories.Remove(cate);
                    store.SaveChanges();
                    TempData["SuccesMessage"] = "Category deleted successfully!";
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
                TempData["FailMessage"] = "Category is being used with the product!";
                return RedirectToAction("Index");
            }
        }
        public ActionResult Update(int id)
        {
            Category cate = store.Categories.FirstOrDefault(t => t.CategoryId == id);
            ViewBag.test = cate;
            return View("Update"); 
        }
        [HttpPost]
        public ActionResult Update(Category cate)
        {
            Category tmpCate = store.Categories.FirstOrDefault(t => t.CategoryId == cate.CategoryId);
            tmpCate.Name = cate.Name;
            store.SaveChanges();
            return RedirectToAction("Index"); 
        }
    }
}