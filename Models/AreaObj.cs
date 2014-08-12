using System;
using System.Collections.Generic;
using System.Text;

namespace Gllo.Models
{
    [Serializable]
    public class ProvinceObj
    {
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; }
    }

    [Serializable]
    public class CityObj
    {
        public int CityID { get; set; }
        public string CityName { get; set; }
        public int ProvinceID { get; set; }
    }

    [Serializable]
    public class RegionObj
    {
        public int CityID { get; set; }
        public int RegionID { get; set; }
        public string RegionName { get; set; }
    }
}
