using Serilog;
using Store_EF.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class CategoriesController : Controller
    {
        static StoreEntities store = new StoreEntities();

        public ActionResult Index()
        {
            try
            {
                return View(store.Categories.ToList());
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return View();
            }
        }

        [HttpPost]
        public ActionResult Add(Category c)
        {
            if (ModelState.IsValid)
            {
                store.Categories.Add(c);
                try
                {
                    store.SaveChanges();
                    return View("Index");
                }
                catch (Exception ex)
                {
                    store.Categories.Remove(c);
                    Log.Error(ex.ToString());
                    return HttpNotFound();
                }
            }
            else
            {
                return Index();
            }
        }
    }
}