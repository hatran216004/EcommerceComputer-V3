using Store_EF.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class CheckOutController : Controller
    {
        StoreEntities store = new StoreEntities();
        // GET: CheckOut
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            IEnumerable<Cart> cart = store.Carts.Where(x => x.UserId == userId);
            return View(cart);
        }
    }
}