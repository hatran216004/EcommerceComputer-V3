using Store_EF.Models;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class OrderController : Controller
    {

        StoreEntities store = new StoreEntities();

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            return View(store.Orders);
        }
    }
}