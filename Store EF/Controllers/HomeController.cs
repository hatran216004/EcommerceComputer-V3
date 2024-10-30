using Store_EF.Models;
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
            if (Session["UserId"] != null)
            {
                int userId = int.Parse(Session["UserId"].ToString());
                User user = store.Users.First(x => x.UserId == userId);
                if (!user.IsConfirm)
                    return RedirectToAction("Verify", "Home");
            }
            IEnumerable<Product> promoProducts = store.Products.Where(x => x.Stock != 0 && x.PromoPrice != null);
            if (promoProducts.Count() >= 12)
                return View(promoProducts.OrderByDescending(x => x.CreatedAt).Take(12));
            else
                return View(store.Products.Where(x => x.Stock != 0).OrderByDescending(x => x.CreatedAt));
        }

        public ActionResult Verify()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return View();
            else
                return RedirectToAction("Index");
        }
    }
}