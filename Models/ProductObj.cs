using System;
using System.Collections.Generic;
using System.Text;

namespace Gllo.Models
{
    public class ProductCateObj
    {
        public int CategoryID { get; set; }
        public int ParentID { get; set; }
        public string CategoryName { get; set; }
        public int Sort { get; set; }
    }

    public class ProductObj
    {
        public int ProductID { get; set; }
        public int CategoryID { get; set; }
        public IList<int> Categories { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Serial { get; set; }
        public string Model { get; set; }
        public string Code { get; set; }
        public string Material { get; set; }
        public string Weight { get; set; }
        public string Description { get; set; }
        public string Characteristic { get; set; }
        public string Designer { get; set; }
        public decimal Price { get; set; }
        public decimal SpecialPrice { get; set; }
        public bool IsNew { get; set; }
        public bool IsOnSale { get; set; }
        public bool IsRecommend { get; set; }
        public bool CanPurchasedSeparately { get; set; }

        public decimal Freight { get; set; }
        public decimal Freight1 { get; set; }
        public int Quantity { get; set; }
        public int Inventory { get; set; }
        public string Tags { get; set; }
        public int Points { get; set; }
        public DateTime Sort { get; set; }
        /// <summary>
        /// 产品状态 { 0: "下架", 1: "在线", 2: "已删除" }
        /// </summary>
        public int Status { get; set; }

        public int SellNum { get; set; }

        public IList<ProductPictureObj> ProductPictures { get; set; }
        public IList<ProductPictureObj> Colors { get; set; }
        public List<int> FreightModels { get; set; }
        public List<string> FreightModelNames { get; set; }
        
    }

    public class ProductPictureObj
    {
        public int PictureID { get; set; }
        public int ProductID { get; set; }
        public string SavePath { get; set; }
        public string PictureDesc { get; set; }
        public string Url { get; set; }

        /// <summary>
        /// 图片类型 { 0: "产品图片", 1: "颜色图片" }
        /// </summary>
        public int Type { get; set; }
    }
}
