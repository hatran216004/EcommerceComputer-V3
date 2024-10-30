using Serilog;
using Store_EF.Models;
using System;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class CartController : Controller
    {
        StoreEntities store = new StoreEntities();

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            else
                return View(store.Carts.Where(x => x.UserId == userId));
        }

        [HttpPost]
        public ActionResult Add(int? product, int quantity = 0, string url = "")
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            if (!product.HasValue)
                return RedirectToAction("Index");
            else
            {
                if (store.Carts.Where(x => x.UserId == userId && x.ProductId == product.Value).Count() == 0)
                {
                    store.Carts.Add(new Cart()
                    {
                        UserId = userId,
                        ProductId = product.Value,
                        Quantity = 1,
                        CreatedAt = DateTime.Now,
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
                catch (DbUpdateException ex)
                {
                    Debug.WriteLine(ex.InnerException.InnerException.Message);
                    store = new StoreEntities();
                }
                if (url == "")
                    return Content(store.Carts.Where(x => x.UserId == userId && x.ProductId == product.Value).First().Quantity.ToString());
                else
                    return Redirect(url);
            }
        }

        [HttpPost]
        public ActionResult Remove(int? product)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            if (!product.HasValue)
                return HttpNotFound();
            else
            {
                try
                {
                    Cart c = store.Carts.Where(x => x.UserId == userId && x.ProductId == product.Value).First();
                    store.Carts.Remove(c);
                    store.SaveChanges();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult RemoveAll()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            try
            {
                store.Carts.RemoveRange(store.Carts.Where(x => x.UserId == userId));
                store.SaveChanges();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return RedirectToAction("Index");
        }
    }
}