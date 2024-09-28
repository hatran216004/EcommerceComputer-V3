using Serilog;
using Store_EF.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class AuthController : Controller
    {
        static StoreEntities store = new StoreEntities();

        public ActionResult SignUp()
        {
            return View();
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
                store.Users.Add(new User_
                {
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(password),
                    RoleName = "User"
                });
                try
                {
                    store.SaveChanges();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
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
            User_ user = null;
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
    }
}