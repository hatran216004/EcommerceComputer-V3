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
        public ActionResult Add(int? product, int quantity = 0)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            if (!product.HasValue)
                return HttpNotFound();
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
                    if (quantity != 0)
                        cart.Quantity = quantity;
                    else
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

        [HttpPost]
        public ActionResult Remove(int? product, bool confirm = false) {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            if (!product.HasValue)
                return HttpNotFound();
            else {
                try { 
                    Product p = store.Products.Where(x => x.ProductId == product).First();
                    if (confirm)
                    {
                        store.Products.Remove(p);
                        store.SaveChanges();
                    }
                    return RedirectToAction("Index", "Products");
                } catch (Exception ex) {
                    Log.Error(ex.ToString());
                    return HttpNotFound();
                }
            }
        }
    }
}