using Newtonsoft.Json;
using Store_EF.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Store_EF.Handlers
{
    public static class ProvincesHandler
    {
        public static readonly List<Province> Provinces;

        static ProvincesHandler()
        {
            Provinces = JsonConvert.DeserializeObject<List<Province>>(File.ReadAllText(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~"), "Public", "provinces.json")));
        }

        public static IEnumerable<Province> Districts(int provinceCode)
        {
            return Provinces.Where(x => x.Code == provinceCode).First().Districts;
        }

        public static IEnumerable<Province> Wards(int provinceCode, int districtCode)
        {
            return Districts(provinceCode).Where(x => x.Code == districtCode).First().Wards;
        }
    }
}