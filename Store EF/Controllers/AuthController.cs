using MimeKit;
using Store_EF.Handlers;
using Store_EF.Models;
using Store_EF.Models.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class AuthController : Controller
    {
        StoreEntities store = new StoreEntities();

        public ActionResult SignUp()
        {
            return View();
        }

        public ActionResult Verify(string email, string code)
        {
            User user = store.Users.FirstOrDefault(x => x.Email == email);
            if (user == null)
                return View("Error");
            if (!user.IsConfirm)
            {
                if (user.Email == email && user.UniqueCode == code)
                {
                    user.IsConfirm = true;
                    try
                    {
                        store.SaveChanges();
                        object data = "Xác thực email thành công!";
                        return View(data);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
                else
                {
                    object data = "Link không hợp lệ!";
                    return View(data);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        static void SendVerifyEmail(User user, string absoluteUri, string pathUri)
        {
            var message = new MimeMessage();
            message.To.Add(new MailboxAddress("Khách hàng", user.Email));
            message.From.Add(new MailboxAddress("Store EF", GmailHandler.Email));
            message.Subject = "Kích hoạt tài khoản";
            message.Body = new TextPart("plain")
            {
                Text = $"Vào link sau để xác nhận email: {Regex.Match(absoluteUri, "^(https?:\\/\\/[^\\/]+)").Value}{pathUri}"
            };
            GmailHandler.SendMail(message);
        }

        [HttpPost]
        public ActionResult SignUp(string email, string password)
        {
            bool err = false;
            if (password.Length < 6)
            {
                err = true;
                ModelState.AddModelError("Password", "Mật khẩu tối thiểu 6 ký tự!");
            }
            if (!Helpers.IsValidEmail(email))
            {
                err = true;
                ModelState.AddModelError("Email", "Email không hợp lệ!");
            }
            if (!err)
            {
                User user = new User
                {
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    RoleName = "User",
                    UniqueCode = email.GenUniqueWithSalt(),
                    CreatedAt = DateTime.Now,
                };
                store.Users.Add(user);
                try
                {
                    store.SaveChanges();
                    new Thread(() => SendVerifyEmail(user, Request.Url.AbsoluteUri, Url.Action("Verify", "Auth", new { email = user.Email, code = user.UniqueCode }))).Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    ModelState.AddModelError("Db", "Lỗi cơ sở dữ liệu!");
                    return View();
                }
                return RedirectToAction("SignIn", "Auth");
            }

            else
                return View();
        }

        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignIn(string email, string password)
        {
            User user = null;
            try
            {
                user = store.Users.Where(x => x.Email.Equals(email)).First();
                if (BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    Session["UserId"] = user.UserId;
                    Session["Email"] = user.Email;
                    Session["RoleName"] = user.RoleName;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("Password", "Sai mật khẩu.");
                    return View();
                }
            }
            catch
            {
                ModelState.AddModelError("Email", "Email không tồn tại!");
                return View();
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("SignIn", "Auth");
        }

        public ActionResult ForgotPassword(string email)
        {
            // Logic
            return View();
        }
    }
}