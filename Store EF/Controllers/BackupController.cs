using CsvHelper;
using Newtonsoft.Json;
using Store_EF.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class BackupController : Controller
    {
        StoreEntities store = new StoreEntities();
        SupportEntities support = new SupportEntities();

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            if (!Helpers.IsUserAdmin(userId, store))
                return RedirectToAction("Index");
            string fP = Path.Combine(Server.MapPath("~"), Helpers.FILE_PATH);
            if (!Directory.Exists(Path.GetDirectoryName(fP)))
                Directory.CreateDirectory(Path.GetDirectoryName(fP));
            if (!System.IO.File.Exists(fP))
                System.IO.File.Create(fP).Close();
            return View(JsonConvert.DeserializeObject<IEnumerable<Backup>>(System.IO.File.ReadAllText(fP)));
        }

        [HttpGet]
        public ActionResult Export()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            if (!Helpers.IsUserAdmin(userId, store))
                return RedirectToAction("Index");
            string fP = Path.Combine(Server.MapPath("~"), Helpers.FILE_PATH);
            var history = JsonConvert.DeserializeObject<IEnumerable<Backup>>(System.IO.File.ReadAllText(fP));
            if (history != null)
            {
                var memory = new MemoryStream();
                var writer = new StreamWriter(memory);
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(history);
                }
                return File(memory.ToArray(), "application/json", "backup.csv");
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Create(string name, string desc, string folderPath, string type)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("SignIn", "Auth");
            int userId = int.Parse(Session["UserId"].ToString());
            User user = store.Users.First(x => x.UserId == userId);
            if (!user.IsConfirm)
                return RedirectToAction("Verify", "Home");
            if (!Helpers.IsUserAdmin(userId, store))
                return RedirectToAction("Index");
            string fP = Path.Combine(Server.MapPath("~"), Helpers.FILE_PATH);
            List<Backup> data = JsonConvert.DeserializeObject<List<Backup>>(System.IO.File.ReadAllText(fP));
            if (data == null)
                data = new List<Backup>();
            Nullable<Backup> tmp;
            if (type == "Log")
                tmp = support.BackupLog(name, desc, folderPath);
            else if (type == "Differential")
                tmp = support.BackupDB(name, desc, folderPath, true);
            else
                tmp = support.BackupDB(name, desc, folderPath, false);
            if (tmp.HasValue)
            {
                data.Add(tmp.Value);
                System.IO.File.WriteAllText(fP, JsonConvert.SerializeObject(data));
            }
            return RedirectToAction("Index");
        }
    }
}