using System;
using System.Collections.Generic;
using System.Text;

namespace Gllo.Models
{
    [Serializable]
    public class OrderList : List<OrderObj>
    {
    }

    [Serializable]
    public class OrderObj
    {
        public int OrderID { get; set; }
        public string OrderCode { get; set; }
        public DateTime CreationTime { get; set; }

        //{ 0: "未确定", 1: "已确定", 2: "已取消"，3:"等待用户确认收货", 4: "用户已确认收货" }
        public int Status { get; set; }

        public int PayType { get; set; }

        //{ 0: "未付款", 1: "已付款" }
        public int PayStatus { get; set; }
        public DateTime PayedTime { get; set; }
        public string TradeNo { get; set; }
        public string BuyerEmail { get; set; }

        public string Memo { get; set; }
        public string ServiceMemo { get; set; }

        public int UserID { get; set; }
        public string Receiver { get; set; }
        public string IP { get; set; }
        public int CityID { get; set; }
        public int RegionID { get; set; }
        public string Zip { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal Freight { get; set; }
        public decimal EarnestMoney { get; set; }
        public int Measurement { get; set; }
        public bool IsDistribute { get; set; }
        public bool IsConsigned { get; set; }
        public string Logistics { get; set; }
        public string LogisticsCode { get; set; }
        public string TransportType { get; set; }

        public int LogisticStatus { get; set; }

        public bool SignIn { get; set; }
        public int Score { get; set; }
        public string Comments { get; set; }
        public OrderProductList OrderDetails { get; set; }
        public int ExpressID { get; set; }
        public string ExpressName { get; set; }
        public string ExpressUrl { get; set; }
        

        public int ProvinceID { get; set; }
        public string CityName { get; set; }
        public string RegionName { get; set; }
        public string ProvinceName { get; set; }

    }

    [Serializable]
    public class OrderProductList : List<OrderProductObj>
    {
    }

    [Serializable]
    public class OrderProductObj
    {
        public int DetailID { get; set; }
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        public decimal Discount { get; set; }
        public decimal OrignalPrice { get; set; }
        public decimal Freight { get; set; }
        public int Quantity { get; set; }
        public int Express { get; set; }
        public DateTime CreationTime { get; set; }
        public int Status { get; set; }
        public int PackageID { get; set; }
    }

    [Serializable]
    public class SurveyObj
    {
        public int OrderID { get; set; }
        public int SurveyLike { get; set; }
        public string CustomerName { get; set; }
        public bool Gender { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public int RegionID { get; set; }
        public string Address { get; set; }
        public int Type { get; set; }
        public int SquareType { get; set; }
        public string Options { get; set; }
        public int Days { get; set; }
        public string HopeTime { get; set; }
        public int HopeHour { get; set; }
        public string Pic { get; set; }
        public string Memo { get; set; }
        public int CityID { get; set; }
        public int ProvinceID { get; set; }
        public string RegionName { get; set; }
        public string ProvinceName { get; set; }
        public string CityName { get; set; }
    }
}
