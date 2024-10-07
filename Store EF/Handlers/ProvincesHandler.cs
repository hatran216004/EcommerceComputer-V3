using Newtonsoft.Json;
using Store_EF.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Store_EF.Handlers
{
    public static class ProvincesHandler
    {
        public static readonly List<Province> Provinces;

        static ProvincesHandler()
        {
            using (HttpClient client = new HttpClient())
            {
                Provinces = JsonConvert.DeserializeObject<List<Province>>(client.GetStringAsync(@"https://provinces.open-api.vn/api/?depth=3").Result);
            }
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