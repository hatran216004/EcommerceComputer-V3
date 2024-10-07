using Newtonsoft.Json.Linq;
using Store_EF.Handlers;
using Store_EF.Models;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace Store_EF.Controllers
{
    public class HelpersController : Controller
    {
        public JsonResult Provinces()
        {
            JArray result = new JArray();
            foreach (Province p in ProvincesHandler.Provinces)
            {
                JObject o = new JObject
                {
                    { "code", p.Code },
                    { "name", p.Name }
                };
                result.Add(o);
            }
            return Json(Regex.Replace(result.ToString(), @"\s(?=([^""]*""[^""]*"")*[^""]*$)", string.Empty), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Districts(int provinceCode = 0)
        {
            JArray result = new JArray();
            if (provinceCode > 0)
            {
                foreach (Province p in ProvincesHandler.Districts(provinceCode))
                {
                    JObject o = new JObject
                    {
                        { "code", p.Code },
                        { "name", p.Name }
                    };
                    result.Add(o);
                }
                return Json(Regex.Replace(result.ToString(), @"\s(?=([^""]*""[^""]*"")*[^""]*$)", string.Empty), JsonRequestBehavior.AllowGet);
            }
            return Json(result.ToString(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult Wards(int provinceCode = 0, int districtCode = 0)
        {
            JArray result = new JArray();
            if (provinceCode > 0 && districtCode > 0)
            {
                foreach (Province p in ProvincesHandler.Wards(provinceCode, districtCode))
                {
                    JObject o = new JObject
                    {
                        { "code", p.Code },
                        { "name", p.Name }
                    };
                    result.Add(o);
                }
                return Json(Regex.Replace(result.ToString(), @"\s(?=([^""]*""[^""]*"")*[^""]*$)", string.Empty), JsonRequestBehavior.AllowGet);
            }
            return Json(result.ToString(), JsonRequestBehavior.AllowGet);
        }
    }
}