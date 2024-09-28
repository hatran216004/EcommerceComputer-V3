using Serilog;
using Store_EF.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class CartController : Controller
    {
        static StoreEntities store = new StoreEntities();

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            return View(store.Carts.Where(x => x.UserId == userId));

        }

        [HttpPost]
        public ActionResult Add(int? product)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            if (!product.HasValue)
                return RedirectToAction("Index", "Products");
            else
            {
                int userId = int.Parse(Session["UserId"].ToString());
                if (store.Carts.Where(x => x.UserId == userId && x.ProductId == product.Value).Count() == 0)
                {
                    store.Carts.Add(new Cart()
                    {
                        UserId = userId,
                        ProductId = product.Value,
                        Quantity = 1
                    });
                }
                else
                {
                    Cart cart = store.Carts.Where(x => x.UserId == userId && x.ProductId == product.Value).First();
                    cart.Quantity += 1;
                }
                try
                {
                    store.SaveChanges();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    return HttpNotFound();
                }
                return RedirectToAction("Detail", "Products", new { id = product });
            }
        }
    }
}