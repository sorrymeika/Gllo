using System;
using System.Collections.Generic;
using System.Text;

namespace Gllo.Models
{
    [Serializable]
    public class PhotoObj
    {
        public int PhotoID { get; set; }
        public string Name { get; set; }
        public string Pic { get; set; }
        public string Info { get; set; }

        public IList<int> Categories { get; set; }
    }

    [Serializable]
    public class PhotoPointObj
    {
        public int PointID { get; set; }
        public int PhotoID { get; set; }
        public string Pic { get; set; }
        public string Name { get; set; }
        public string Cap { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

}
