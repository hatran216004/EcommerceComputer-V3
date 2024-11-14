using System;
using Store_EF.Models;
using System.Diagnostics;
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

        public ActionResult Management()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            return View(store.Orders.OrderByDescending(x => x.CreatedAt));
        }

        [HttpPost]
        public ActionResult Accept(int orderId)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            Order order = store.Orders.FirstOrDefault(x => x.OrderId == orderId);
            if (order != null)
            {
                Payment payment = order.Payments.First();
                if (payment.Status == "Succeeded")
                {
                    payment.Status = "Accepted";
                    try
                    {
                        store.SaveChanges();
                        return RedirectToAction("Detail", "Order", new { id = orderId });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
            return RedirectToAction("Management", "Order");
        }

        [HttpPost]
        public ActionResult Refund(int orderId)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            Order order = store.Orders.FirstOrDefault(x => x.OrderId == orderId);
            if (order != null)
            {
                Payment payment = order.Payments.First();
                if (payment.Status == "Refunding")
                {
                    payment.Status = "Refunded";
                } else if (payment.Status == "Succeeded")
                {
                    payment.Status = "Refunding";
                } else
                {
                    return RedirectToAction("Index");
                }
                try
                {
                    store.SaveChanges();
                    return RedirectToAction("Detail", "Order", new { id = orderId });
                }
                catch (Exception ex) { 
                    Debug.WriteLine(ex);
                }
            }
            return RedirectToAction("Management", "Order");
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
            Order detail;
            if (user.RoleName == "Admin")
            {
                detail = store.Orders.FirstOrDefault(x => x.OrderId == id);
            } else
            {
                detail = store.Orders.FirstOrDefault(x => x.OrderId == id && x.UserId == userId);
            }
            if (detail == null)
                return RedirectToAction("Index");
            return View(detail);
        }
    }
}