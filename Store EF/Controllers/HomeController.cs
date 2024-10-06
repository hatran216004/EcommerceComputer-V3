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
                IEnumerable<Product> promoProducts = store.Products.Where(x => x.Stock != 0 && x.PromoPrice != null);
                if (promoProducts.Count() >= 12)
                    return View(promoProducts.OrderByDescending(x => x.CreatedAt).Take(12));
                else
                    return View(store.Products.Where(x => x.Stock != 0).OrderByDescending(x => x.CreatedAt));
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return View(new List<Product>());
            }
        }
    }
}