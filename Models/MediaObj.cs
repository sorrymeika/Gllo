using System;
using System.Collections.Generic;
using System.Text;

namespace Gllo.Models
{
    [Serializable]
    public class MediaObj
    {
        public int MediaID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Src { get; set; }
        public string Url { get; set; }
        public int Type { get; set; }
        public string SavePath { get; set; }
        public string Content { get; set; }
    }

    [Serializable]
    public class MediaList : List<MediaObj>
    {
    }
}
