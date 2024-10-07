using Newtonsoft.Json;
using System.Collections.Generic;

namespace Store_EF.Models
{
    public class Province
    {
        string name;
        [JsonProperty("name")]
        public string Name { get { return name; } set { name = value; } }

        int code;
        [JsonProperty("code")]
        public int Code { get { return code; } set { code = value; } }

        List<Province> districts;
        [JsonProperty("districts")]
        public List<Province> Districts { get { return districts; } set { districts = value; } }

        List<Province> wards;
        [JsonProperty("wards")]
        public List<Province> Wards { get { return wards; } set { wards = value; } }

        public Province() { }

        public Province(string name, int code, List<Province> districts, List<Province> wards)
        {
            Name = name;
            Code = code;
            Districts = districts;
            Wards = wards;
        }
    }
}