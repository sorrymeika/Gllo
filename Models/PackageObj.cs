using System;
using System.Collections.Generic;
using System.Text;

namespace Gllo.Models
{
    [Serializable]
    public class PackageCateObj
    {
        public int ParentID { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int Sort { get; set; }
    }

    [Serializable]
    public class PackageBuildingObj
    {
        public int BuildingID { get; set; }
        public string BuildingName { get; set; }
        public int RegionID { get; set; }
    }

    [Serializable]
    public class PackageObj
    {
        public int PackageID { get; set; }
        public int CategoryID { get; set; }
        public string PackageName { get; set; }
        public string Code { get; set; }
        public string Material { get; set; }
        public int SaleNum { get; set; }
        public string Memo { get; set; }
        public string Intro { get; set; }
        public int BuildingID { get; set; }
        public int RegionID { get; set; }
        public int CityID { get; set; }
        public int ProvinceID { get; set; }
        
        public IList<PackagePictureObj> Pictures { get; set; }
        public IList<int> Categories { get; set; }
    }

    [Serializable]
    public class PackageProdCatesObj
    {
        public int PpcID { get; set; }
        public string PpcName { get; set; }
        public int PackageID { get; set; }

        public IList<int> PackageProducts { get; set; }
    }

    [Serializable]
    public class PackageProductsObj
    {
        public int PpcID { get; set; }
        public int ProductID { get; set; }
        public int PpID { get; set; }
    }

    [Serializable]
    public class PackagePictureObj
    {
        public int PictureID { get; set; }
        public int PackageID { get; set; }
        public string PictureDesc { get; set; }
        public string SavePath { get; set; }
        public string Url { get; set; }
    }

    [Serializable]
    public class LPackageCate
    {
        public int PackageID { get; set; }
        public int CategoryID { get; set; }
    }
}
