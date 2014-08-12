using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gllo.Models
{
    public class FreightModelInfo
    {
        public int ModelID { get; set; }
        public string ModelName { get; set; }
        public int ExpressID { get; set; }
        public decimal Freight { get; set; }
        public decimal Freight1 { get; set; }
        public IList<AreaFreightInfo> AreaFreightList { get; set; }
    }

    public class AreaFreightInfo
    {
        public int ModelID { get; set; }
        public string AreaID { get; set; }
        public byte AreaType { get; set; }
        public decimal Freight { get; set; }
        public decimal Freight1 { get; set; }
    }
}
