using Newtonsoft.Json;
using System.Collections.Generic;

namespace Store_EF.Models
{
    public class Province
    {
        string name;
        [JsonProperty("name")]
        public string Name { get { return name; } set { name = value; } }

        uint code;
        [JsonProperty("code")]
        public uint Code { get { return code; } set { code = value; } }

        List<Province> districts;
        [JsonProperty("districts")]
        public List<Province> Districts { get { return districts; } set { districts = value; } }

        List<Province> wards;
        [JsonProperty("wards")]
        public List<Province> Wards { get { return wards; } set { wards = value; } }

        public Province() { }

        public Province(string name, uint code, List<Province> districts, List<Province> wards)
        {
            Name = name;
            Code = code;
            Districts = districts;
            Wards = wards;
        }
    }
}