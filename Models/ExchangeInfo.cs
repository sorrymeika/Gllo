using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gllo.Models
{
    [Serializable]
    public class ExchangeInfo
    {
        public int ExchangeID { get; set; }
        public int OrderID { get; set; }
        public DateTime AddTime { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int RegionID { get; set; }
        public string Zip { get; set; }
        public int ReturnProductID { get; set; }
        public int ReturnQty { get; set; }
        public int ExchangeProductID { get; set; }
        public int ExchangeQty { get; set; }
        public string Reason { get; set; }
    }
}
