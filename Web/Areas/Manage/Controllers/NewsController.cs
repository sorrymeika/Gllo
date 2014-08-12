using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

using Gllo.Models;
using Gllo.Sys;
using Gllo.BLL;
using System.Text;

namespace Gllo.Web.Manage.Controllers
{
    public class NewsController : Controller
    {
        #region [页面] 新闻分类管理
        public ActionResult NewsCates()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1))
                return Redirect("/Manage/Error/2.html");

            return View();
        }
        #endregion

        #region 新增新闻分类
        public ActionResult AddNewsCate()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 101))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            NewsCateObj newsCateObj = new NewsCateObj();
            newsCateObj.CategoryName = validation.Get("categoryName", false, "新闻分类名称不可为空！");
            newsCateObj.ParentID = validation.GetInt("categoryID");
            if (validation.HasError)
            {
                return Json(new { success = false, msg = "ValidationFailed", errors = validation.GetErrors() });
            }
            newsCateObj.Sort = DateTime.Now;

            NewsBLL newsBLL = new NewsBLL();
            if (newsBLL.IsNewsCateExists(newsCateObj.ParentID, newsCateObj.CategoryName))
                return Json(new { success = false, msg = "该类别已存在!" });

            newsBLL.AddNewsCate(newsCateObj);
            return Json(new { success = true });
        }
        #endregion

        #region 修改新闻分类
        public ActionResult ModifyNewsCate()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 102))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            int categoryID = validation.GetInt("categoryID");
            NewsBLL newsBLL = new NewsBLL();
            NewsCateObj newsCateObj = newsBLL.GetNewsCateByCateID(categoryID);
            if (newsCateObj != null)
            {
                newsCateObj.CategoryName = validation.Get("categoryName", false, "新闻分类名称不可为空！");
                if (validation.HasError)
                {
                    return Json(new { success = false, msg = "ValidationFailed", errors = validation.GetErrors() });
                }

                newsBLL.ModifyNewsCate(newsCateObj);
                return Json(new { success = true });
            }
            else
                return Json(new { success = false, msg = "该类别不存在！" });

        }
        #endregion

        #region 删除新闻分类
        public ActionResult DeleteNewsCate()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 103))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            int categoryID = validation.GetInt("categoryID");
            if (validation.HasError || categoryID == 0)
                return Json(new { success = false, msg = "参数错误！" });
            NewsBLL newsBLL = new NewsBLL();
            bool hasChild = newsBLL.IsNewsCateHasChildren(categoryID);
            //if (hasChild)
            //    return Json(new { success = false, msg = "该类别包含子类或新闻，无法删除！" });

            newsBLL.DeleteNewsCate(categoryID);
            return Json(new { success = true });
        }
        #endregion

        #region 获取所有新闻分类
        public ActionResult GetNewsCates()
        {
            NewsBLL newsBLL = new NewsBLL();
            JsonArray result = newsBLL.GetNewsCates();

            return Json(new { success = true, data = result });
        }
        #endregion

        #region [页面] 新闻管理
        public ActionResult News()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 2))
                return Redirect("/Manage/Error/2.html");

            NewsBLL newsBLL = new NewsBLL();

            return View();
        }
        #endregion

        #region [页面] 新闻管理1
        public ActionResult News1()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 2))
                return Redirect("/Manage/Error/2.html");

            NewsBLL newsBLL = new NewsBLL();

            return View();
        }
        #endregion

        #region [页面] 新闻添加
        public ActionResult NewsAdd(int categoryID)
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 201))
                return Redirect("/Manage/Error/2.html");

            ViewBag.categoryID = categoryID;
            return View();
        }
        #endregion

        #region [页面] 新闻修改
        public ActionResult NewsModify(int id)
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 202))
                return Redirect("/Manage/Error/2.html");

            ViewBag.newsID = id;
            return View();
        }
        #endregion

        #region 添加新闻
        public ActionResult AddNews()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 201))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();

            NewsObj newsObj = new NewsObj();
            newsObj.CategoryID = validation.GetInt("categoryID");
            newsObj.Title = validation.Get("title", false, "请填写新闻标题");
            newsObj.Writer = validation.Get("writer");
            newsObj.Tags = validation.Get("tags");
            newsObj.PublishDate = DateTime.Now;
            newsObj.Content = HttpUtility.UrlDecode(validation.Get("content", false, "请填写新闻内容"), Encoding.UTF8);

            if (validation.HasError || newsObj.CategoryID == 0)
            {
                return Json(new { success = false, msg = "参数错误", errors = validation.GetErrors() });
            }

            newsObj.Sort = DateTime.Now;
            newsObj.Clicks = 0;

            NewsBLL newsBLL = new NewsBLL();
            newsBLL.AddNews(newsObj);

            if (newsObj.NewsID != 0)
                return Json(new { success = true, id = newsObj.NewsID });
            else
                return Json(new { success = false, msg = "抱歉，添加失败！" });
        }
        #endregion

        #region 修改新闻
        public ActionResult ModifyNews()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 202))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            NewsBLL newsBLL = new NewsBLL();

            int newsId = validation.GetInt("id");
            NewsObj newsObj = newsBLL.GetNews(newsId);
            if (newsObj == null)
            {
                return Json(new { success = false, msg = "您要修改的新闻不存在" });
            }

            newsObj.Title = validation.Get("title", false, "请填写新闻标题");
            newsObj.Writer = validation.Get("writer");
            newsObj.Tags = validation.Get("tags");
            newsObj.Content = HttpUtility.UrlDecode(validation.Get("content", false, "请填写新闻内容"), Encoding.UTF8);

            if (validation.HasError)
            {
                return Json(new { success = false, msg = "参数错误", errors = validation.GetErrors() });
            }

            newsObj.Sort = DateTime.Now;
            newsObj.Clicks = 0;

            newsBLL.ModifyNews(newsObj);

            return Json(new { success = true });
        }
        #endregion

        #region 查询新闻
        public ActionResult SearchNews()
        {
            Validation validation = new Validation();
            int categoryID = validation.GetInt("categoryID");
            int page = validation.GetInt("page", defaultValue: 1);
            int pageSize = validation.GetInt("pageSize", defaultValue: 10);
            DateTime dtFrom = validation.GetDateTime("dtFrom");
            DateTime dtTo = validation.GetDateTime("dtTo");
            string keywords = validation.Get("keywords");

            NewsBLL newsBLL = new NewsBLL();
            int total;
            JsonArray result = newsBLL.GetNews(categoryID, keywords, dtFrom, dtTo, page, pageSize, out total);
            return Json(new { success = true, total = total, data = result });
        }
        #endregion

        #region 删除新闻
        public ActionResult DeleteNews(int id)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 203))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            NewsBLL newsBLL = new NewsBLL();

            newsBLL.DeleteNews(id);

            return Json(new { success = true });
        }
        #endregion

        #region 新闻排序
        public ActionResult SortNews(int id, int beforeID)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 204))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            NewsBLL newsBLL = new NewsBLL();

            newsBLL.SortNews(id, beforeID);

            return Json(new { success = true });
        }
        #endregion

        #region 根据新闻编号获取新闻信息
        public ActionResult GetNewsByID(int id)
        {
            NewsBLL newsBLL = new NewsBLL();
            NewsObj result = newsBLL.GetNews(id);
            if (result != null)
                return Json(new { success = true, data = result, date = result.PublishDate.ToString("yyyy-MM-dd"), time = result.PublishDate.ToString("HH:mm") });
            else
                return Json(new { success = false, msg = "该新闻不存在" });

        }
        #endregion
    }
}
