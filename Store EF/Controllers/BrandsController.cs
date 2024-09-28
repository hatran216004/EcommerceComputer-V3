using Serilog;
using Store_EF.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class BrandsController : Controller
    {
        static StoreEntities store = new StoreEntities();

        public ActionResult Index()
        {
            try
            {
                return View(store.Brands.ToList());
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return View();
            }
        }

        [HttpPost]
        public ActionResult Add(Brand b)
        {
            if (ModelState.IsValid)
            {
                store.Brands.Add(b);
                try
                {
                    store.SaveChanges();
                    return View("Index");
                }
                catch (Exception ex)
                {
                    store.Brands.Remove(b);
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