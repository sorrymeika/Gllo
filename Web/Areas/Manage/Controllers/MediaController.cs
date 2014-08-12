using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Gllo.Sys;
using System.Text.RegularExpressions;
using System.IO;
using Gllo.BLL;
using Gllo.Models;

namespace Gllo.Web.Manage.Controllers
{
    public class MediaController : Controller
    {
        #region 首页设置
        public ActionResult Index()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 16))
                return Redirect("/Manage/Error/2.html");

            MediaBLL mediaBLL = new MediaBLL();
            MediaObj mediaObj = mediaBLL.GetMedia(1);
            ViewBag.media1 = mediaObj;
            mediaObj = mediaBLL.GetMedia(2);
            ViewBag.media2 = mediaObj;
            mediaObj = mediaBLL.GetMedia(3);
            ViewBag.media3 = mediaObj;
            mediaObj = mediaBLL.GetMedia(4);
            ViewBag.media4 = mediaObj;
            mediaObj = mediaBLL.GetMedia(5);
            ViewBag.media5 = mediaObj;
            mediaObj = mediaBLL.GetMedia(6);
            ViewBag.media6 = mediaObj;
            mediaObj = mediaBLL.GetMedia(7);
            ViewBag.media7 = mediaObj;
            mediaObj = mediaBLL.GetMedia(8);
            ViewBag.media8 = mediaObj;
            mediaObj = mediaBLL.GetMedia(9);
            ViewBag.media9 = mediaObj;
            mediaObj = mediaBLL.GetMedia(10);
            ViewBag.media10 = mediaObj;
            mediaObj = mediaBLL.GetMedia(11);
            ViewBag.media11 = mediaObj;
            mediaObj = mediaBLL.GetMedia(12);
            ViewBag.media12 = mediaObj;
            mediaObj = mediaBLL.GetMedia(13);
            ViewBag.media13 = mediaObj;
            mediaObj = mediaBLL.GetMedia(14);
            ViewBag.media14 = mediaObj;
            return View();
        }
        #endregion

        #region 产品页设置
        public ActionResult Products()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 17))
                return Redirect("/Manage/Error/2.html");

            MediaBLL mediaBLL = new MediaBLL();
            ViewBag.media15 = mediaBLL.GetMedia(15);
            ViewBag.media16 = mediaBLL.GetMedia(16);
            ViewBag.media17 = mediaBLL.GetMedia(17);
            ViewBag.media18 = mediaBLL.GetMedia(18);
            ViewBag.media19 = mediaBLL.GetMedia(19);
            ViewBag.media20 = mediaBLL.GetMedia(20);
            ViewBag.media21 = mediaBLL.GetMedia(21);
            ViewBag.media22 = mediaBLL.GetMedia(22);
            ViewBag.media23 = mediaBLL.GetMedia(23);
            ViewBag.media24 = mediaBLL.GetMedia(24);
            ViewBag.media25 = mediaBLL.GetMedia(25);
            ViewBag.media26 = mediaBLL.GetMedia(26);
            ViewBag.media27 = mediaBLL.GetMedia(27);
            ViewBag.media28 = mediaBLL.GetMedia(28);
            ViewBag.media29 = mediaBLL.GetMedia(29);
            ViewBag.media30 = mediaBLL.GetMedia(30);
            ViewBag.media31 = mediaBLL.GetMedia(31);
            ViewBag.media32 = mediaBLL.GetMedia(32);
            ViewBag.media33 = mediaBLL.GetMedia(33);
            ViewBag.media34 = mediaBLL.GetMedia(34);
            ViewBag.media35 = mediaBLL.GetMedia(35);

            return View();
        }
        #endregion

        #region 购物车设置
        public ActionResult Cart()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 18))
                return Redirect("/Manage/Error/2.html");

            SettingBLL settingBLL = new SettingBLL();
            ViewBag.text = settingBLL.Get("LogisticTips");

            return View();
        }
        #endregion

        #region 保存系统设置
        public ActionResult SetSetting()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1801))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            string key = vld.Get("key", false);
            string val = vld.Get("value", false);

            if (vld.HasError)
                return Json(new { success = false, msg = "参数错误！" });

            SettingBLL settingBLL = new SettingBLL();
            settingBLL.Set(key, val);

            return Json(new { success = true });
        }
        #endregion

        #region 媒体上传
        public ActionResult Upload()
        {
            ViewBag.success = false;
            if (!AppData.IsManagerLogin)
            {
                ViewBag.msg = "抱歉，您未登录后台或会话已过期!";
                return View();
            }
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1601))
            {
                ViewBag.msg = "您没有执行该操作的权限!";
                return View();
            }

            HttpPostedFileBase pic = Request.Files.Count == 0 ? null : Request.Files[0];
            int mediaID = string.IsNullOrEmpty(Request.Form["id"]) ? 0 : int.Parse(Request.Form["id"]);
            if (pic != null && !string.IsNullOrEmpty(pic.FileName))
            {
                int type = string.IsNullOrEmpty(Request.Form["type"]) ? 0 : int.Parse(Request.Form["type"]);

                string ext = Path.GetExtension(pic.FileName);
                if (type == 0)
                {
                    if (!Regex.IsMatch(ext, @"^\.(gif|jpg|jpeg|png)$", RegexOptions.IgnoreCase))
                    {
                        ViewBag.msg = "上传的图片格式不合要求,请上传gif,png,jpg格式的图片";
                        return View();
                    }
                }
                else if (type == 1)
                {
                    if (!Regex.IsMatch(ext, @"^\.(mp3|wma)$", RegexOptions.IgnoreCase))
                    {
                        ViewBag.msg = "上传的图片格式不合要求,请上传mp3,wma格式的文件";
                        return View();
                    }
                }

                string dirPath = Config.MediaPath + @"\Media";
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                MediaBLL mediaBLL = new MediaBLL();
                MediaObj mediaObj = mediaBLL.GetMedia(mediaID);
                if (mediaObj != null)
                {
                    System.IO.File.Delete(Config.MediaPath + mediaObj.SavePath);
                }
                else
                {
                    mediaObj = new MediaObj();
                    mediaObj.MediaID = mediaID;
                }
                string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff") + ext;
                string savePath = Path.Combine(dirPath, newFileName);

                mediaObj.SavePath = @"\Media\" + newFileName;
                mediaObj.Src = "/Media/" + newFileName;
                mediaObj.Description = Request.Form["desc"];
                mediaObj.Url = Request.Form["url"];
                mediaObj.Content = Request.Form["content"];

                mediaBLL.SetMedia(mediaObj);

                pic.SaveAs(savePath);
                ViewBag.success = true;
                return View();
            }
            else if (mediaID != 0)
            {
                MediaBLL mediaBLL = new MediaBLL();
                MediaObj mediaObj = mediaBLL.GetMedia(mediaID);

                mediaObj.Description = Request.Form["desc"];
                mediaObj.Url = Request.Form["url"];
                mediaObj.Content = Request.Form["content"];

                mediaBLL.SetMedia(mediaObj);
                ViewBag.success = true;
                return View();
            }
            else
            {
                ViewBag.msg = "请您选择一张图片上传";
                return View();
            }
        }
        #endregion

    }
}
