using Store_EF.Models;
using System.Linq;
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
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            store.UpdatePaymentStatus(userId);
            return View(store.Orders.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt));
        }

        public ActionResult Invoice(int id = 0)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            Order order = store.Orders.FirstOrDefault(x => x.OrderId == id && x.UserId == userId);
            if (order == null)
                return RedirectToAction("Index");
            else
            {
                Payment payment = store.Payments.FirstOrDefault(x => x.OrderId == id && x.Status == "Succeeded");
                if (payment == null)
                    return RedirectToAction("Index");
                else
                {
                    ViewBag.Payment = payment;
                    return PartialView(order);
                }
            }
        }

        public ActionResult Detail(int id = 0)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            Order detail = store.Orders.FirstOrDefault(x => x.OrderId == id && x.UserId == userId);
            if (detail == null)
                return RedirectToAction("Index");
            return View(detail);
        }
    }
}