using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Gllo.Models
{
    public class NewsCateObj
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int ParentID { get; set; }
        public DateTime Sort { get; set; }
    }

    public class NewsCateList : List<NewsCateObj>
    {
        public string ToJsonString()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }

    public class NewsObj
    {
        public int NewsID { get; set; }
        public string Title { get; set; }
        public string Writer { get; set; }
        public int Clicks { get; set; }
        public DateTime PublishDate { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
        public int CategoryID { get; set; }
        public DateTime Sort { get; set; }
        public IList<NewsPageObj> Pages { get; set; }
    }

    public class NewsList : List<NewsObj>
    {
        public string ToJsonString()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }

    public class NewsPageObj
    {
        public int Page { get; set; }
        public string Content { get; set; }
    }

    public class NewsUploadedPictureList : List<NewsUploadedPictureObj>
    {
        public string ToJsonString()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }

    public class NewsUploadedPictureObj
    {
        public int PictureID { get; set; }
        public string Url { get; set; }
        public int NewsID { get; set; }
        public int TempID { get; set; }
        public bool IsConfirmed { get; set; }
    }

}
