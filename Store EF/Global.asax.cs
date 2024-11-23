using Newtonsoft.Json;
using Store_EF.Handlers;
using Store_EF.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Store_EF
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            new BackgroudHandler(() =>
            {
                var now = DateTime.Now;
                if (now.Hour == Helpers.BACKUP_HOUR || now.Hour % 6 == 0)
                {
                    SupportEntities support = new SupportEntities();
                    string day = now.DayOfWeek.ToString();
                    string folderPath = "E:\\Backup";
                    string fP = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), Path.GetFileName(Helpers.FILE_PATH));
                    if (!Directory.Exists(Path.GetDirectoryName(fP)))
                        Directory.CreateDirectory(Path.GetDirectoryName(fP));
                    if (!File.Exists(fP))
                        File.Create(fP).Close();
                    List<Backup> data = JsonConvert.DeserializeObject<List<Backup>>(File.ReadAllText(fP));
                    if (data == null)
                        data = new List<Backup>();

                    if (now.Hour == Helpers.BACKUP_HOUR)
                    {
                        Backup? db;
                        if (now.DayOfWeek == DayOfWeek.Monday)
                            db = support.BackupDB($"Db {day}", $"Regular full backups on {day} at {now}", folderPath);
                        else
                        {
                            if (data.Count == 0)
                                db = support.BackupDB($"Db {day}", $"Regular full backups on {day} at {now}", folderPath);
                            else
                                db = support.BackupDB($"Db {day}", $"Regular differential backups on {day} at {now}", folderPath, true);
                        }
                        if (db.HasValue)
                            data.Add(db.Value);
                    }
                    else if (now.Hour % 6 == 0)
                    {
                        Backup? log = support.BackupLog($"Log {day} at {now}", $"Regular log backups on {day} at {now}", folderPath);
                        if (log.HasValue)
                            data.Add(log.Value);
                    }
                    File.WriteAllText(fP, JsonConvert.SerializeObject(data));
                }
                Thread.Sleep(TimeSpan.FromHours(1));
            }).Start();
        }
    }
}
