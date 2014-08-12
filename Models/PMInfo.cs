using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gllo.Models
{
    public class PMInfo
    {
        public int PMID { get; set; }
        public int UserID { get; set; }
        public int UserIDFrom { get; set; }
        public DateTime PMTime { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
