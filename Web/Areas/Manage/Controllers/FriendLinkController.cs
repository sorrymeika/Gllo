using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gllo.Sys;
using Gllo.BLL;
using Gllo.Models;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace Gllo.Web.Manage.Controllers
{
    public class FriendLinkController : Controller
    {
        public ActionResult FriendLinks()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 19))
                return Redirect("/Manage/Error/2.html");

            return View();
        }

        private ActionResult Result(bool success, string msg = null)
        {
            StringBuilder sb = new StringBuilder("<html><body><script language=\"javascript\">var result={ success:");
            if (success)
            {
                sb.Append("true");
            }
            else
            {
                sb.Append("false, msg=\"")
                   .Append(msg.Replace("\"", "\\\""))
                   .Append("\"");
            }
            sb.Append(" }; if (window.parent&&window.parent.handleResult) window.parent.handleResult(result);</script></body></html>");

            return Content(sb.ToString(), "text/html");
        }

        public ActionResult AddFriendLink()
        {
            if (Request.HttpMethod != "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1901))
                    return Redirect("/Manage/Error/2.html");

                return View();
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Result(false, "您未登录后台或会话已过期");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1901))
                    return Result(false, "您没有执行该操作的权限");

                FriendLinkBLL friendLinkBLL = new FriendLinkBLL();

                HttpPostedFileBase pic = Request.Files["pic"];
                if (pic == null || pic.ContentLength == 0)
                    return Result(false, "请选择一张图片上传");

                string ext = System.IO.Path.GetExtension(pic.FileName);
                if (!Regex.IsMatch(ext, @"^\.(gif|jpg|jpeg|png)$", RegexOptions.IgnoreCase))
                    return Result(false, "上传的图片格式不合要求,请上传gif,png,jpg格式的图片");


                Validation vld = new Validation();
                FriendLinkObj friendLinkObj = new FriendLinkObj();
                friendLinkObj.FriendName = vld.Get("name", false);
                friendLinkObj.Url = vld.Get("url", false);

                if (vld.HasError)
                    return Result(false, "数据填写不完整");

                string dirPath = Config.MediaPath + @"\Upload";
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff") + ext;
                string savePath = Path.Combine(dirPath, newFileName);
                friendLinkObj.Pic = "/Upload/" + newFileName;

                friendLinkBLL.AddFriendLink(friendLinkObj);

                pic.SaveAs(savePath);
                return Result(true);
            }
        }

        public ActionResult ModifyFriendLink()
        {
            if (Request.HttpMethod != "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1901))
                    return Redirect("/Manage/Error/2.html");

                int id = int.Parse(Request.QueryString["id"]);
                FriendLinkBLL friendLinkBLL = new FriendLinkBLL();
                ViewBag.data = friendLinkBLL.GetFriendLinkByID(id);
                return View();
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Result(false, "您未登录后台或会话已过期");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1901))
                    return Result(false, "您没有执行该操作的权限");

                Validation vld = new Validation();
                int id = vld.GetInt("id");
                FriendLinkBLL friendLinkBLL = new FriendLinkBLL();
                FriendLinkObj friendLinkObj = friendLinkBLL.GetFriendLinkByID(id);

                HttpPostedFileBase pic = Request.Files["pic"];
                if (pic != null && pic.ContentLength != 0)
                {
                    string ext = System.IO.Path.GetExtension(pic.FileName);
                    if (!Regex.IsMatch(ext, @"^\.(gif|jpg|jpeg|png)$", RegexOptions.IgnoreCase))
                        return Result(false, "上传的图片格式不合要求,请上传gif,png,jpg格式的图片");

                    System.IO.File.Delete(Config.MediaPath + friendLinkObj.Pic);

                    string dirPath = Config.MediaPath + @"\Upload";
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);

                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff") + ext;
                    string savePath = Path.Combine(dirPath, newFileName);
                    friendLinkObj.Pic = "/Upload/" + newFileName;
                    pic.SaveAs(savePath);
                }

                friendLinkObj.FriendName = vld.Get("name", false);
                friendLinkObj.Url = vld.Get("url", false);

                if (vld.HasError)
                    return Result(false, "数据填写不完整");

                friendLinkBLL.ModifyFriendLink(friendLinkObj);

                return Result(true);
            }
        }

        public ActionResult GetFriendLink()
        {
            Validation vld = new Validation();
            int id = vld.GetInt("id");

            FriendLinkBLL friendLinkBLL = new FriendLinkBLL();
            dynamic result;
            if (id == 0)
                result = friendLinkBLL.GetFriendLinks();
            else
                result = friendLinkBLL.GetFriendLinkByID(id);

            return Json(new { success = true, data = result });
        }


        public ActionResult DeleteFriendLink()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1903))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            FriendLinkBLL friendLinkBLL = new FriendLinkBLL();

            Validation vld = new Validation();
            int id = vld.GetInt("id");
            if (id != 0)
            {
                FriendLinkObj friendLinkObj = friendLinkBLL.GetFriendLinkByID(id);
                System.IO.File.Delete(Config.MediaPath + friendLinkObj.Pic);
                friendLinkBLL.DeleteFriendLink(id);
            }

            return Json(new { success = true });
        }
    }
}
