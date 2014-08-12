using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gllo.Sys;
using System.Drawing;
using Gllo.BLL;
using Gllo.Models;
using System.Text.RegularExpressions;

namespace Gllo.Web.Controllers
{
    public class HomeController : Controller
    {
        #region 引导页
        public ActionResult Entrance()
        {
            ViewBag.cates = new ProductBLL().GetParentCates();
            MediaBLL mediaBLL = new MediaBLL();
            MediaObj mediaObj = mediaBLL.GetMedia(1);
            ViewBag.media1 = mediaObj;
            mediaObj = mediaBLL.GetMedia(2);
            ViewBag.media2 = mediaObj;
            mediaObj = mediaBLL.GetMedia(3);
            ViewBag.media3 = mediaObj;
            return View();
        }
        #endregion

        #region 首页
        public ActionResult Index()
        {
            MediaBLL mediaBLL = new MediaBLL();
            IDictionary<int, MediaObj> flash = new Dictionary<int, MediaObj>();
            flash.Add(4, mediaBLL.GetMedia(4));
            flash.Add(5, mediaBLL.GetMedia(5));
            flash.Add(6, mediaBLL.GetMedia(6));
            flash.Add(7, mediaBLL.GetMedia(7));
            flash.Add(8, mediaBLL.GetMedia(8));
            flash.Add(9, mediaBLL.GetMedia(9));
            flash.Add(10, mediaBLL.GetMedia(10));
            flash.Add(11, mediaBLL.GetMedia(11));
            flash.Add(12, mediaBLL.GetMedia(12));
            flash.Add(13, mediaBLL.GetMedia(13));
            flash.Add(14, mediaBLL.GetMedia(14));

            ViewBag.flash = flash;

            return View();
        }
        #endregion

        public ActionResult Logout()
        {
            AppData.SessionUser = null;
            return Redirect("/Index.html");
        }

        public ActionResult Test()
        {
            return View();
        }

        #region 验证码
        public ActionResult CheckCode()
        {
            string checkCode;
            byte[] imageBuffer = CheckCodeCreater.CreateImage(out checkCode);
            Session["CheckCode"] = checkCode;

            return File(imageBuffer, "image/Jpeg");
        }
        #endregion

        #region QQ验证码
        private static Gllo.Sys.HttpWebClient QQTuanWeb()
        {
            Gllo.Sys.HttpWebClient web;
            if (System.Web.HttpRuntime.Cache["QQTuanWeb"] == null)
            {
                web = new Gllo.Sys.HttpWebClient();
                System.Web.HttpRuntime.Cache["QQTuanWeb"] = web;
            }
            else
            {
                web = (Gllo.Sys.HttpWebClient)System.Web.HttpRuntime.Cache["QQTuanWeb"];
            }
            return web;
        }

        public ActionResult TestImage()
        {
            var web = QQTuanWeb();

            byte[] imageBuffer = web.GetImage("http://captcha.qq.com/getimage?aid=8000201&r=0.05799194785264128&uin=1765598293&vc_type=");
            return File(imageBuffer, "image/Jpeg");
        }

        public ActionResult QQTuan()
        {
            var web = QQTuanWeb();
            string result = web.GetString("http://yz.tuan.qq.com/deal/verify?stoken=5920315656&igrouponid=130353", null);

            if (string.IsNullOrEmpty(result) || Regex.IsMatch(result, @"^RET_COMMON\.retcode = -\d+$"))
            {
                ViewBag.vc = true;
            }
            else
            {
                ViewBag.vc = false;
            }
            return View();
        }

        public ActionResult QQTuanValidate(string verifycode, string acode)
        {
            var web = QQTuanWeb();

            string result = web.GetString("http://ptlogin2.qq.com/login", new Dictionary<string, string>()
            {
                { "u", "1765598293" },
                { "p", System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile("7344EA081051AC4625C86767D0F7912F"+verifycode.ToUpper(), "md5") },
                { "aid", "8000201" },
                { "u1", "http://imgcache.qq.com/club/portal_new/redirect.html?jump_url=http%3A//yz.tuan.qq.com/" },
                { "fp", "loginerroralert" },
                { "h", "1" },
                { "ptredirect", "0" },
                { "ptlang", "2052" },
                { "from_ui", "1" },
                { "dumy", "" },
                { "mibao_css", "" },
                { "verifycode", verifycode },
            });
            result = web.GetString("http://yz.tuan.qq.com/deal/verify?stoken=" + acode + "&igrouponid=130353", null);

            if (string.IsNullOrEmpty(result) || Regex.IsMatch(result, @"^RET_COMMON\.retcode = -\d+$"))
            {
                return Json(new { success = false, msg = "网页已过期或验证码错误，请刷新后再试" });
            }
            else
            {
                return Json(new { success = true, data = result });
            }

        }
        #endregion

        #region 图片预览
        [HttpPost]
        public ActionResult ViewPic()
        {
            HttpPostedFileBase pic = Request.Files.Count == 0 ? null : Request.Files[0];
            if (pic != null && pic.ContentLength != 0)
            {
                int width = int.Parse(Request.QueryString["width"]), height = int.Parse(Request.QueryString["height"]);
                Image originalImage = new Bitmap(pic.InputStream);
                Bitmap image = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(image);
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

                g.DrawImage(originalImage, new Rectangle(0, 0, width, height), new Rectangle(0, 0, originalImage.Width, originalImage.Height), GraphicsUnit.Pixel);

                byte[] imageBuffer;
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                    g.Dispose();
                    image.Dispose();

                    imageBuffer = ms.ToArray();
                }

                string guid = System.Guid.NewGuid().ToString("N");

                AppData.SessionPicViews.Add(guid, imageBuffer);
                ViewBag.success = true;
                ViewBag.guid = guid;
                ViewBag.name = Request.Files.Keys[0];
                ViewBag.id = string.IsNullOrEmpty(Request.QueryString["id"]) ? "0" : Request.QueryString["id"];
            }
            else
            {
                ViewBag.success = false;
                ViewBag.msg = "您还未选择图片";
            }
            return View();
        }

        public ActionResult ViewPic(string picGuid)
        {
            if (AppData.SessionPicViews.ContainsKey(picGuid))
            {
                byte[] imageBuffer = AppData.SessionPicViews[picGuid];
                AppData.SessionPicViews.Remove(picGuid);
                return File(imageBuffer, "image/Jpeg");
            }
            else
                return Content("图片不存在！");

        }

        public ActionResult ClearViewPic()
        {
            AppData.SessionPicViews = null;
            return Content("{\"success\":true}");
        }
        #endregion

    }
}
