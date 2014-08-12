using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Gllo.BLL;
using Gllo.Models;

namespace Gllo.Web.Controllers
{
    public class NewsController : Controller
    {
        public ActionResult HelpCenter(int id)
        {
            NewsBLL newsBLL = new NewsBLL();
            NewsObj newsObj = newsBLL.GetNews(id);
            if (newsObj != null)
            {
                ViewBag.news = newsObj;
                NewsCateObj newsCateObj = newsBLL.GetNewsCateByCateID(newsObj.CategoryID);
                int total;
                JsonArray cates = newsBLL.GetNewsCates(newsCateObj.ParentID);
                if (cates != null)
                {
                    for (int i = 0; i < cates.Count; i++)
                    {
                        int categoryID = (int)cates[i]["categoryID"];
                        cates[i]["news"] = newsBLL.GetNews(categoryID, null, DateTime.MinValue, DateTime.MinValue, 1, 100, out total);
                    }
                }
                ViewBag.cates = cates;
            }
            return View();
        }

        public ActionResult News(int id)
        {
            NewsBLL newsBLL = new NewsBLL();
            NewsObj newsObj = newsBLL.GetNews(id);
            if (newsObj != null)
            {
                ViewBag.news = newsObj;
                ViewBag.nextNews = newsBLL.GetNextNews(id, newsObj.CategoryID);
                ViewBag.prevNews = newsBLL.GetPrevNews(id, newsObj.CategoryID);
                NewsCateObj newsCateObj = newsBLL.GetNewsCateByCateID(newsObj.CategoryID);
                int total;
                JsonArray cates = newsBLL.GetNewsCates(newsCateObj.ParentID);
                if (cates != null)
                {
                    for (int i = 0; i < cates.Count; i++)
                    {
                        int categoryID = (int)cates[i]["categoryID"];
                        cates[i]["news"] = newsBLL.GetNews(categoryID, null, DateTime.MinValue, DateTime.MinValue, 1, 100, out total);
                    }
                }
                ViewBag.cates = cates;
            }
            return View();
        }

    }
}
