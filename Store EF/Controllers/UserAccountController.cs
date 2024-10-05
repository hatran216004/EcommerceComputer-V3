using Serilog;
using Store_EF.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class UserAccountController : Controller
    {
        StoreEntities store = new StoreEntities();

        public ActionResult Profile(int userId)
        {
            var userDetails = store.UserDetails.FirstOrDefault(x => x.UserId == userId);
            return View(userDetails);
        }

        [HttpPost]
        public ActionResult UpdateProfile(UserDetail user)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUserDetail = store.UserDetails.Find(user.UserId);
                    if (existingUserDetail != null)
                    {
                        existingUserDetail.Name = user.Name;
                        existingUserDetail.Gender = user.Gender;
                        existingUserDetail.Phone = user.Phone;
                        existingUserDetail.Address = user.Address;
                        existingUserDetail.DateOfBirth = user.DateOfBirth;
                        store.SaveChanges();
                    }

                    return RedirectToAction("Profile", new { userId = user.UserId });
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    ModelState.AddModelError("", "Lỗi khi cập nhật thông tin!");
                }
            }

            return View(user);
        }

        public ActionResult UpdatePassword(string userId, string currPassword, string newPassword, string passwordConfirm)
        {
            if (newPassword != passwordConfirm)
            {
                ModelState.AddModelError("", "Mật khẩu mới không khớp.");
            }

            var user = store.Users.Find(int.Parse(userId));
            if (user == null)
            {
                return RedirectToAction("Profile");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (BCrypt.Net.BCrypt.Verify(currPassword, user.Password))
                    {
                        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                        user.PasswordChangedAt = DateTime.Now;
                        store.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                    ModelState.AddModelError("", "Lỗi khi cập nhật thông tin!");
                }
            }

            return RedirectToAction("Profile", new { userId = int.Parse(userId) });
        }
    }
}