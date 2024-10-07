using Newtonsoft.Json.Linq;
using Store_EF.Handlers;
using Store_EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class HelpersController : Controller
    {
        public JsonResult Provinces()
        {
            JArray result = new JArray();
            foreach (Province p in ProvincesHandler.Provinces) { 
                JObject o = new JObject
                {
                    { "code", p.Code },
                    { "name", p.Name }
                };
                result.Add(o);
            }
            return Json(Regex.Replace(result.ToString(), @"\s(?=([^""]*""[^""]*"")*[^""]*$)", string.Empty), JsonRequestBehavior.AllowGet);
        }
    }
}