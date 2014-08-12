using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Gllo.Sys;
using Gllo.Models;
using Gllo.BLL;
using System.IO;
using System.Globalization;
using System.Web.Script.Serialization;

namespace Gllo.Web.Manage.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/");

            return View();
        }

        public ActionResult Logout()
        {
            AppData.SessionUser = null;
            return Redirect("/Manage/");
        }

        #region 用户登录
        public ActionResult Login()
        {
            UserObj user;
            if (Request.HttpMethod == "POST")
            {
                string sessionCheckCode = Session["CheckCode"] == null ? null : Session["CheckCode"].ToString();
                Session["CheckCode"] = null;

                if (sessionCheckCode == null)
                {
                    ViewBag.Success = false;
                    ViewBag.Msg = new Dictionary<string, string>()
                    { 
                        {"checkCode", "验证码已经过期，请刷新验证码！"}
                    };
                    return View();
                }

                user = new UserObj();
                Validation validation = new Validation();
                string checkCode = validation.Get("checkCode", emptyAble: false, emptyText: "请填写验证码", compare: sessionCheckCode, compareText: "验证码错误！");
                user.UserName = validation.Get("userName", emptyAble: false, emptyText: "请填写用户名");
                user.Password = validation.Get("password", emptyAble: false, emptyText: "请填写密码");

                //安全码
                //string safeCode = validation.Get("safeCode");

                if (validation.HasError)
                {
                    ViewBag.Success = false;
                    ViewBag.Msg = validation.GetErrors();
                }
                else
                {
                    user.Password = Md5.MD5(user.Password);

                    UserBLL bll = new UserBLL();
                    ViewBag.Success = bll.Login(user);
                    if (ViewBag.Success)
                    {
                        AppData.SessionUser = user;
                        return Redirect(user.UserRole == UserRoles.Manager || user.UserRole == UserRoles.System || user.UserRole == UserRoles.Administrator ? "/Manage/" : "/");
                    }
                    else
                    {
                        AppData.SessionUser = null;
                        ViewBag.Success = false;
                        ViewBag.Msg = new Dictionary<string, string>()
                        { 
                            {"password", "用户名或密码错误,登录失败"}
                        };
                    }
                }
            }
            else
            {
                if (AppData.IsManagerLogin)
                {
                    return View("Index");
                }
                else
                {
                    ViewBag.Success = true;
                }
            }
            return View();
        }
        #endregion

        #region 图片上传
        public ActionResult Upload()
        {
            if (!AppData.IsManagerLogin)
                return showError("请先登录后台系统！");

            HttpPostedFileBase imgFile = Request.Files["imgFile"];
            if (imgFile == null)
            {
                return showError("请选择文件。");
            }
            string dir = DateTime.Today.ToString("yy-MM-dd");
            String dirPath = Config.MediaPath + "/Upload/" + dir + "/";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            String fileName = imgFile.FileName;
            String fileExt = Path.GetExtension(fileName).ToLower();
            int maxSize = 1000000;

            if (imgFile.InputStream == null || imgFile.InputStream.Length > maxSize)
            {
                return showError("上传文件大小超过限制。");
            }

            String fileTypes = "gif,jpg,jpeg,png,bmp";
            if (String.IsNullOrEmpty(fileExt) || Array.IndexOf(fileTypes.Split(','), fileExt.Substring(1).ToLower()) == -1)
            {
                return showError("上传文件扩展名是不允许的扩展名。");
            }

            String newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff", DateTimeFormatInfo.InvariantInfo) + fileExt;
            String filePath = Path.Combine(dirPath, newFileName);

            imgFile.SaveAs(filePath);

            String fileUrl = Config.MediaServer + "/upload/" + dir + "/" + newFileName;

            return Content(new JavaScriptSerializer().Serialize(new { error = 0, url = fileUrl }));
        }

        private ActionResult showError(string msg)
        {

            return Content(new JavaScriptSerializer().Serialize(new { error = 1, message = msg }));
        }
        #endregion

        #region 错误页面
        public ActionResult Error(int id)
        {
            ViewBag.errorType = id;
            return View();
        }
        #endregion

    }
}
