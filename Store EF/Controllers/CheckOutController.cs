﻿using Store_EF.Handlers;
using Store_EF.Models;
using Store_EF.Models.Extensions;
using StuceSoftware.RandomStringGenerator;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;
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
            User_ user = store.Users.First(x => x.UserId == userId);
            if (user.CurrentPayment() != null)
                return RedirectToAction("Payment");
            if (user.TotalCartPrice() == 0)
                return RedirectToAction("Index", "Products");
            CheckOutForm checkOut = new CheckOutForm()
            {
                FullName = user.UserDetail.Name,
                Phone = user.UserDetail.Phone,
            };
            return View(checkOut);
        }

        [HttpPost]
        public ActionResult Payment(CheckOutForm checkOut)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            if (store.Users.First(x => x.UserId == userId).CurrentPayment() != null)
                return RedirectToAction("Payment");
            if (ModelState.IsValid)
            {
                Province p = ProvincesHandler.Provinces.First(x => x.Code == checkOut.Province);
                Province d = ProvincesHandler.Districts(p.Code).First(x => x.Code == checkOut.District);
                Province w = ProvincesHandler.Wards(p.Code, d.Code).First(x => x.Code == checkOut.Ward);
                string address = $"{checkOut.Home}, {w.Name}, {d.Name}, {p.Name}";
                Order_ order = new Order_()
                {
                    Name = checkOut.FullName,
                    Address = address,
                    Phone = checkOut.Phone,
                    Note = checkOut.Note,
                    Status = "",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UserId = userId,
                };
                store.Orders.Add(order);
                int orderId = -1;
                string code = "DH" + RandomStringGenerator.GetString(CharClasses.Uppercase | CharClasses.Numbers, maxLength: 10, randomLength: false, false);
                Payment payment = new Payment();
                try
                {
                    store.SaveChanges();
                    orderId = store.Entry(order).GetDatabaseValues().GetValue<int>("OrderId");
                    payment = new Payment()
                    {
                        OrderId = orderId,
                        Code = code,
                        Method = checkOut.PaymentMethod,
                        Status = "Waitting",
                        Expiry = DateTime.Now.AddDays(1),
                        Bank = ConfigurationManager.AppSettings["Bank"],
                        Account = ConfigurationManager.AppSettings["Account"]
                    };
                    store.Payments.Add(payment);
                    store.SaveChanges();
                    return RedirectToAction("Payment");
                }
                catch
                {
                    store.Orders.Remove(order);
                    if (payment != new Payment())
                        store.Payments.Remove(payment);
                    return View("Index");
                }
            }
            else
                return View("Index");
        }

        public ActionResult Payment()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            Payment payment = store.Users.Where(x => x.UserId == userId).First().CurrentPayment();
            if (payment == null)
                return RedirectToAction("Index", "Home");
            else
                return View(payment);
        }

        public ActionResult Status(string code)
        {
            Payment payment = store.Payments.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
            if (payment == null)
                return new HttpStatusCodeResult(404);
            Response.ContentType = "text/event-stream";
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
            Response.BufferOutput = false;
            while (true)
            {
                payment = new StoreEntities().Payments.FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                switch (payment.Status)
                {
                    case "Succeeded":
                        {
                            try
                            {
                                Response.Write($"data: {payment.Status}\n\n");
                                Response.Flush();
                                Response.End();
                            }
                            catch
                            {
                                return new HttpStatusCodeResult(200);
                            }
                        }
                        break;
                    case "Failed":
                        {
                            try
                            {
                                Response.Write($"data: {payment.Status}\n\n");
                                Response.Flush();
                                Response.End();
                            }
                            catch
                            {
                                return new HttpStatusCodeResult(200);
                            }
                        }
                        break;
                }
                if (payment.Status == "Succeeded" || payment.Status == "Failed" || !Response.IsClientConnected)
                    break;
                Thread.Sleep(5000);
            }
            return new HttpStatusCodeResult(200);
        }
    }
}