using Serilog;
using Store_EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class HomeController : Controller
    {
        StoreEntities store = new StoreEntities();
        public ActionResult Index()
        {
            try
            {
                return View(store.Products.Where(x => x.Stock != 0 && x.PromoPrice != null).OrderByDescending(x => x.CreatedAt).Take(12).ToList());
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return View(new List<Product>());
            }
        }
    }
}