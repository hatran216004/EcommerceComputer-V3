using PagedList;
using Store_EF.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class UserAccountController : Controller
    {
        StoreEntities store = new StoreEntities();

        public new ActionResult Profile()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            var userDetails = store.UserDetails.FirstOrDefault(x => x.UserId == userId);
            return View(userDetails);
        }

        [HttpPost]
        public ActionResult UpdateProfile(UserDetail detail)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUserDetail = store.UserDetails.Find(detail.UserId);
                    if (existingUserDetail != null)
                    {
                        existingUserDetail.Name = detail.Name;
                        existingUserDetail.Gender = detail.Gender;
                        existingUserDetail.Phone = detail.Phone;
                        existingUserDetail.Address = detail.Address;
                        existingUserDetail.DateOfBirth = detail.DateOfBirth;
                        store.SaveChanges();
                    }

                    return RedirectToAction("Profile");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    ModelState.AddModelError("", "Lỗi khi cập nhật thông tin!");
                }
            }

            return View(detail);
        }

        public ActionResult UpdatePassword(string userId, string currPassword, string newPassword, string passwordConfirm)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int cuserId = int.Parse(Session["UserId"].ToString());
            User cuser = store.Users.First(x => x.UserId == cuserId);
            if (!cuser.IsConfirm)
                return RedirectToAction("Verify", "Home");
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
                    Debug.WriteLine(ex);
                    ModelState.AddModelError("", "Lỗi khi cập nhật thông tin!");
                }
            }

            return RedirectToAction("Profile");
        }

        public ActionResult UserManagement(int page = 1)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            int pageSize = 8;
            if (page < 1)
                page = 1;
            try
            {
                List<User> listUsers = store.Users.ToList();
                // Tính số trang dựa trên tổng số người dùng và kích thước trang
                int maxPage = (int)Math.Ceiling((double)listUsers.Count / pageSize);
                if (page > maxPage)
                    page = maxPage;

                ViewBag.MaxPage = maxPage;
                if (TempData["SuccessMessage"] != null)
                {
                    ViewBag.Message = TempData["SuccessMessage"];
                }

                return View(listUsers.ToPagedList(page, pageSize));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return View();
            }
        }

        public ActionResult DeleteUser(int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            try
            {
                User findUser = store.Users.Find(id);
                UserDetail findUserDetail = store.UserDetails.Find(id);
                if (findUser != null && findUserDetail != null)
                {
                    store.Users.Remove(findUser);
                    store.UserDetails.Remove(findUserDetail);
                    store.SaveChanges();
                    TempData["SuccessMessage"] = "User deleted successfully!";
                }
                return RedirectToAction("UserManagement");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return RedirectToAction("UserManagement");
            }
        }

        public ActionResult AddUser(string email, string password, string role)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            try
            {
                User newUser = new User
                {
                    Email = email,
                    Password = BCrypt.Net.BCrypt.HashPassword(password), // Bạn nên mã hóa mật khẩu trước khi lưu
                    RoleName = role
                };
                store.Users.Add(newUser);
                store.SaveChanges();
                TempData["SuccessMessage"] = "User added successfully!";
                return RedirectToAction("UserManagement");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return RedirectToAction("UserManagement");
            }
        }

        public ActionResult GetUser(int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            try
            {
                User findUser = store.Users.Find(id);
                return View(findUser);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return RedirectToAction("UserManagement");
            }
        }

        public ActionResult UpdateUser(User user, int id)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User cuser = store.Users.First(x => x.UserId == userId);
            if (!cuser.IsConfirm)
                return RedirectToAction("Verify", "Home");
            try
            {
                User findUser = store.Users.Find(id);
                findUser.Email = user.Email;
                findUser.RoleName = user.RoleName;

                TempData["SuccessMessage"] = "User updated successfully!";
                store.SaveChanges();
                return RedirectToAction("UserManagement");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                TempData["SuccessMessage"] = "Lỗi cmnr...";
                return RedirectToAction("UserManagement");
            }
        }
    }
}