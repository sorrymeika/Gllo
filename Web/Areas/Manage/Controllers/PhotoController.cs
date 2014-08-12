
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Gllo.Sys;
using Gllo.BLL;
using Gllo.Models;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Gllo.Web.Manage.Controllers
{
    public class PhotoController : Controller
    {
        public ActionResult HandleResult(bool success, string msg)
        {
            StringBuilder result = new StringBuilder("<!DOCTYPE html><html><head><title></title></head><body><script type='text/javascript'>if(window.frameElement&&window.parent.handleResult) {");

            if (success)
            {
                result.Append("window.parent.handleResult({ success:true,id:")
                    .Append(msg)
                    .Append(" });");
            }
            else
            {
                result.Append("window.parent.handleResult({ success:false, msg:\"")
                    .Append(msg == null ? null : msg.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\" + "r").Replace("\n", "\\" + "n"))
                    .Append("\" });");
            }

            result.Append("}</script></body></html>");

            return Content(result.ToString(), "text/html");
        }

        #region Photos
        public ActionResult Photos()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 10))
                return Redirect("/Manage/Error/2.html");

            return View();
        }
        #endregion

        #region AddPhoto
        public ActionResult AddPhoto()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                {
                    return HandleResult(false, "您未登录后台或会话已过期");
                }
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1001))
                    return HandleResult(false, "您没有执行该操作的权限");

                Validation vld = new Validation();
                PhotoObj photoObj = new PhotoObj();
                photoObj.Name = vld.Get("name", false, "请填写照片名称");
                photoObj.Info = vld.Get("info");
                string sCategoryIDs = vld.Get("categoryIDs", false, "请至少选择一个类别", regex: @"^\d+(,\d+)*$", regexText: "类别参数错误");
                IList<int> categoryIDs;
                if (string.IsNullOrEmpty(sCategoryIDs))
                {
                    categoryIDs = null;
                }
                else
                {
                    string[] aCategoryIDs = sCategoryIDs.Split(',');
                    categoryIDs = new List<int>();
                    for (int i = 0; i < aCategoryIDs.Length; i++)
                    {
                        categoryIDs.Add(int.Parse(aCategoryIDs[i]));
                    }
                }
                HttpPostedFileBase pic = Request.Files.Count == 0 ? null : Request.Files[0];
                if (pic == null || pic.ContentLength == 0)
                    return HandleResult(false, "请选择一张照片");

                if (vld.HasError)
                    return HandleResult(false, vld.GetError());

                photoObj.Categories = categoryIDs;

                string ext = Path.GetExtension(pic.FileName);
                if (!Regex.IsMatch(ext, @"^\.(gif|jpg|jpeg|png)$", RegexOptions.IgnoreCase))
                {
                    return HandleResult(false, "上传的图片格式不合要求,请上传gif,png,jpg格式的图片");
                }

                string path = DateTime.Now.ToString("yyyyMMdd");
                string dirPath = Config.MediaPath + @"\Package\" + path;
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff") + ext;
                string savePath = Path.Combine(dirPath, newFileName);

                pic.SaveAs(savePath);
                photoObj.Pic = "/Package/" + path + "/" + newFileName;

                PhotoBLL photoBLL = new PhotoBLL();
                photoBLL.AddPhoto(photoObj);

                return HandleResult(true, photoObj.PhotoID.ToString());
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1001))
                    return Redirect("/Manage/Error/2.html");

                return View();
            }
        }
        #endregion

        #region ModifyPhoto
        public ActionResult ModifyPhoto()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                {
                    return HandleResult(false, "您未登录后台或会话已过期");
                }
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1002))
                    return HandleResult(false, "您没有执行该操作的权限");

                Validation vld = new Validation();
                int photoID = vld.GetInt("id", false, "请传入照片编号");
                PhotoBLL photoBLL = new PhotoBLL();
                PhotoObj photoObj = photoBLL.GetPhotoByID(photoID);

                photoObj.Name = vld.Get("name", false, "请填写照片名称");
                photoObj.Info = vld.Get("info");
                string sCategoryIDs = vld.Get("categoryIDs", false, "请至少选择一个类别", regex: @"^\d+(,\d+)*$", regexText: "类别参数错误");
                IList<int> categoryIDs;
                if (string.IsNullOrEmpty(sCategoryIDs))
                {
                    categoryIDs = null;
                }
                else
                {
                    string[] aCategoryIDs = sCategoryIDs.Split(',');
                    categoryIDs = new List<int>();
                    for (int i = 0; i < aCategoryIDs.Length; i++)
                    {
                        categoryIDs.Add(int.Parse(aCategoryIDs[i]));
                    }
                }
                if (vld.HasError)
                    return HandleResult(false, vld.GetError());

                photoObj.Categories = categoryIDs;

                HttpPostedFileBase pic = Request.Files.Count == 0 ? null : Request.Files[0];
                if (pic != null && pic.ContentLength != 0)
                {
                    string ext = Path.GetExtension(pic.FileName);
                    if (!Regex.IsMatch(ext, @"^\.(gif|jpg|jpeg|png)$", RegexOptions.IgnoreCase))
                    {
                        return HandleResult(false, "上传的图片格式不合要求,请上传gif,png,jpg格式的图片");
                    }

                    string path = DateTime.Now.ToString("yyyyMMdd");
                    string dirPath = Config.MediaPath + @"\Package\" + path;
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);

                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff") + ext;
                    string savePath = Path.Combine(dirPath, newFileName);

                    System.IO.File.Delete(Config.MediaPath + photoObj.Pic.Replace("/", "\\"));

                    pic.SaveAs(savePath);
                    photoObj.Pic = "/Package/" + path + "/" + newFileName;
                }

                photoBLL.ModifyPhoto(photoObj);

                return HandleResult(true, photoObj.PhotoID.ToString());
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1002))
                    return Redirect("/Manage/Error/2.html");

                ViewBag.mediaServer = Config.MediaServer;
                return View();
            }
        }
        #endregion

        #region DeletePhoto
        public ActionResult DeletePhoto()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1003))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            int photoID = vld.GetInt("id", false, "请传入照片编号");
            PhotoBLL photoBLL = new PhotoBLL();
            PhotoObj photoObj = photoBLL.GetPhotoByID(photoID);

            System.IO.File.Delete(Config.MediaPath + photoObj.Pic.Replace("/", "\\"));

            JsonArray points = photoBLL.GetPhotoPoints(photoID);
            if (points != null)
            {
                string pic;
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i]["Pic"] != null)
                    {
                        pic = (string)points[i]["Pic"];
                        System.IO.File.Delete(Config.MediaPath + pic.Replace("/", "\\"));
                    }
                }
            }

            photoBLL.DeletePhoto(photoID);

            return Json(new { success = true });
        }
        #endregion

        #region GetPhotos
        public ActionResult GetPhotos()
        {
            Validation vld = new Validation();
            string sCategoryIDs = vld.Get("categoryIDs", false, "请至少选择一个类别", regex: @"^\d+(\.\d+)*$", regexText: "类别参数错误");
            IList<int> categoryIDs;
            if (string.IsNullOrEmpty(sCategoryIDs))
            {
                categoryIDs = null;
            }
            else
            {
                string[] aCategoryIDs = sCategoryIDs.Split(',');
                categoryIDs = new List<int>();
                for (int i = 0; i < aCategoryIDs.Length; i++)
                {
                    categoryIDs.Add(int.Parse(aCategoryIDs[i]));
                }
            }
            string keywords = vld.Get("keywords");
            int page = vld.GetInt("page", defaultValue: 1);
            int pageSize = vld.GetInt("pageSize", defaultValue: 10);

            PhotoBLL photoBLL = new PhotoBLL();
            int total;
            var res = photoBLL.GetPhotos(categoryIDs, keywords, page, pageSize, out total);

            return Json(new { success = true, data = res, total = total });
        }
        #endregion

        #region GetPhotoByID
        public ActionResult GetPhotoByID()
        {
            Validation vld = new Validation();
            int photoID = vld.GetInt("id", false, "请传入照片编号");
            PhotoBLL photoBLL = new PhotoBLL();
            PhotoObj photoObj = photoBLL.GetPhotoByID(photoID);

            return Json(new { success = true, data = photoObj });
        }
        #endregion

        #region GetPhotoPoints
        public ActionResult GetPhotoPoints()
        {
            Validation vld = new Validation();
            int photoID = vld.GetInt("id", false, "请传入照片编号");

            PhotoBLL photoBLL = new PhotoBLL();
            var res = photoBLL.GetPhotoPoints(photoID);

            return Json(new { success = true, data = res });

        }
        #endregion

        #region AddPhotoPoint
        public ActionResult AddPhotoPoint()
        {
            if (!AppData.IsManagerLogin)
            {
                return HandleResult(false, "您未登录后台或会话已过期");
            }
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1001))
                return HandleResult(false, "您没有执行该操作的权限");

            Validation vld = new Validation();
            PhotoPointObj photoPointObj = new PhotoPointObj();
            photoPointObj.PhotoID = vld.GetInt("photoID", false, "请传入照片编号");
            photoPointObj.X = vld.GetInt("x", false, "请传入照片X坐标");
            photoPointObj.Y = vld.GetInt("y", false, "请传入照片Y坐标");
            photoPointObj.Name = vld.Get("name", false, "请填写照片名称");
            photoPointObj.Cap = vld.Get("cab");

            HttpPostedFileBase pic = Request.Files.Count == 0 ? null : Request.Files[0];
            if (pic == null || pic.ContentLength == 0)
                return HandleResult(false, "请选择一张照片");

            if (vld.HasError)
                return HandleResult(false, vld.GetError());

            string ext = Path.GetExtension(pic.FileName);
            if (!Regex.IsMatch(ext, @"^\.(gif|jpg|jpeg|png)$", RegexOptions.IgnoreCase))
            {
                return HandleResult(false, "上传的图片格式不合要求,请上传gif,png,jpg格式的图片");
            }

            string path = DateTime.Now.ToString("yyyyMMdd");
            string dirPath = Config.MediaPath + @"\Package\" + path;
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff") + ext;
            string savePath = Path.Combine(dirPath, newFileName);

            pic.SaveAs(savePath);
            photoPointObj.Pic = "/Package/" + path + "/" + newFileName;

            PhotoBLL photoBLL = new PhotoBLL();
            photoBLL.AddPhotoPoint(photoPointObj);

            return HandleResult(true, photoPointObj.PhotoID.ToString());
        }
        #endregion

        #region ModifyPhotoPoint
        public ActionResult ModifyPhotoPoint()
        {
            if (!AppData.IsManagerLogin)
            {
                return HandleResult(false, "您未登录后台或会话已过期");
            }
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1002))
                return HandleResult(false, "您没有执行该操作的权限");

            Validation vld = new Validation();
            int pointID = vld.GetInt("id", false, "请传入照片编号");
            PhotoBLL photoBLL = new PhotoBLL();

            PhotoPointObj photoPointObj = photoBLL.GetPhotoPointByID(pointID);
            photoPointObj.X = vld.GetInt("x", false, "请传入照片X坐标");
            photoPointObj.Y = vld.GetInt("y", false, "请传入照片Y坐标");
            photoPointObj.Name = vld.Get("name", false, "请填写照片名称");
            photoPointObj.Cap = vld.Get("cab");

            HttpPostedFileBase pic = Request.Files.Count == 0 ? null : Request.Files[0];

            if (vld.HasError)
                return HandleResult(false, vld.GetError());
            if (pic != null && pic.ContentLength != 0)
            {
                string ext = Path.GetExtension(pic.FileName);
                if (!Regex.IsMatch(ext, @"^\.(gif|jpg|jpeg|png)$", RegexOptions.IgnoreCase))
                {
                    return HandleResult(false, "上传的图片格式不合要求,请上传gif,png,jpg格式的图片");
                }

                string path = DateTime.Now.ToString("yyyyMMdd");
                string dirPath = Config.MediaPath + @"\Package\" + path;
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff") + ext;
                string savePath = Path.Combine(dirPath, newFileName);

                System.IO.File.Delete(Config.MediaPath + photoPointObj.Pic.Replace("/", "\\"));

                pic.SaveAs(savePath);
                photoPointObj.Pic = "/Package/" + path + "/" + newFileName;
            }

            photoBLL.ModifyPhotoPoint(photoPointObj);

            return HandleResult(true, photoPointObj.PhotoID.ToString());
        }
        #endregion

        #region DeletePhotoPoint
        public ActionResult DeletePhotoPoint()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1003))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            int pointID = vld.GetInt("id", false, "请传入照片编号");

            PhotoBLL photoBLL = new PhotoBLL();
            PhotoPointObj photoPointObj = photoBLL.GetPhotoPointByID(pointID);

            System.IO.File.Delete(Config.MediaPath + photoPointObj.Pic.Replace("/", "\\"));

            photoBLL.DeletePhotoPoint(pointID);

            return Json(new { success = true });
        }
        #endregion

    }
}
