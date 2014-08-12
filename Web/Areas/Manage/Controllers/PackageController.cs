using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

using Gllo.Sys;
using Gllo.Models;
using Gllo.BLL;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace Gllo.Web.Manage.Controllers
{
    public class PackageController : Controller
    {
        #region Categories
        public ActionResult Categories()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 7))
                return Redirect("/Manage/Error/2.html");

            return View();
        }
        #endregion

        #region AddPackageCate
        public ActionResult AddPackageCate()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 701))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();

            PackageCateObj packageCateObj = new PackageCateObj();
            packageCateObj.CategoryName = vld.Get("categoryName", false, "类别名不可为空！");
            packageCateObj.Sort = vld.GetInt("sort");
            packageCateObj.ParentID = vld.GetInt("categoryID");

            if (vld.HasError)
            {
                return Json(new { success = false, msg = vld.GetError() });
            }

            PackageBLL packageBLL = new PackageBLL();
            packageBLL.AddPackageCate(packageCateObj);

            return Json(new { success = true });
        }
        #endregion

        #region ModifyPackageCate
        public ActionResult ModifyPackageCate()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 702))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            int categoryID = validation.GetInt("categoryID");
            PackageBLL packageBLL = new PackageBLL();
            PackageCateObj packageCateObj = packageBLL.GetPackageCate(categoryID);
            if (packageCateObj != null)
            {
                packageCateObj.CategoryName = validation.Get("categoryName", false, "分类名称不可为空！");
                packageCateObj.Sort = validation.GetInt("sort");
                if (validation.HasError)
                {
                    return Json(new { success = false, msg = validation.GetError() });
                }
                packageBLL.ModifyPackageCate(packageCateObj);
                return Json(new { success = true });
            }
            else
                return Json(new { success = false, msg = "该类别不存在！" });

        }
        #endregion

        #region DeletePackageCate
        public ActionResult DeletePackageCate()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 703))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            int categoryID = validation.GetInt("categoryID");
            if (validation.HasError || categoryID == 0)
                return Json(new { success = false, msg = "参数错误！" });

            PackageBLL packageBLL = new PackageBLL();
            packageBLL.DeletePackageCate(categoryID);
            return Json(new { success = true });
        }
        #endregion

        #region GetPackageCates
        public ActionResult GetPackageCates()
        {
            PackageBLL packageBLL = new PackageBLL();
            JsonArray result = packageBLL.GetPackageCates();

            return Json(new { success = true, data = result });
        }
        #endregion

        #region Packages
        public ActionResult Packages()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 9))
                return Redirect("/Manage/Error/2.html");

            return View();
        }
        #endregion

        #region GetPackages
        public ActionResult GetPackages()
        {
            Validation vld = new Validation();
            string categoryIDs = vld.Get("categoryID", regex: @"^\d+(\.\d+)*$", regexText: "类别参数错误");
            IList<int> categoryID;
            if (string.IsNullOrEmpty(categoryIDs))
            {
                categoryID = null;
            }
            else
            {
                string[] aCategoryIDs = categoryIDs.Split(',');
                categoryID = new List<int>();
                for (int i = 0; i < aCategoryIDs.Length; i++)
                {
                    categoryID.Add(int.Parse(aCategoryIDs[i]));
                }
            }
            string keywords = vld.Get("keywords");
            int buildingID = vld.GetInt("buildingID");
            int regionID = vld.GetInt("regionID");
            int cityID = vld.GetInt("cityID");
            int provinceID = vld.GetInt("provinceID");
            int page = vld.GetInt("page", defaultValue: 1);
            int pageSize = vld.GetInt("pageSize", defaultValue: 10);

            PackageBLL packageBLL = new PackageBLL();
            int total;
            var res = packageBLL.GetPackages(categoryID, keywords, buildingID, regionID, cityID, provinceID, page, pageSize, out total);

            return Json(new { success = true, data = res, total = total });
        }
        #endregion

        #region AddPackage
        public ActionResult AddPackage()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 901))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                Validation vld = new Validation();
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
                PackageObj packageObj = new PackageObj();
                packageObj.CategoryID = categoryIDs[0];
                packageObj.Categories = categoryIDs;
                packageObj.BuildingID = vld.GetInt("buildingID", false, "请填写楼盘");
                packageObj.Code = vld.Get("code", false, "请填写货号");
                packageObj.Intro = HttpUtility.UrlDecode(vld.Get("intro"), Encoding.UTF8);
                packageObj.Memo = vld.Get("memo");
                packageObj.PackageName = vld.Get("name", false, "请填写名称");
                packageObj.Material = vld.Get("material");

                string strPics = vld.Get("pics", false, "请上传商品图片", @"\d+(,\d+)*", "参数错误");

                if (vld.HasError)
                    return Json(new { success = false, msg = vld.GetError() });

                PackageBLL packageBLL = new PackageBLL();
                if (packageBLL.IsPackageExists(packageObj.PackageName))
                    return Json(new { success = false, msg = "该套餐名称已存在!" });

                string[] pics = strPics.Split(',');
                PackagePictureObj pictureObj;
                packageObj.Pictures = new List<PackagePictureObj>();
                for (int i = 0; i < pics.Length; i++)
                {
                    pictureObj = new PackagePictureObj();
                    pictureObj.PictureID = int.Parse(pics[i]);
                    packageObj.Pictures.Add(pictureObj);
                }
                packageObj.SaleNum = 0;

                packageBLL.AddPackage(packageObj);

                if (packageObj.PackageID != 0)
                    return Json(new { success = true, id = packageObj.PackageID });
                else
                    return Json(new { success = false, msg = "抱歉，添加失败!" });
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 901))
                    return Redirect("/Manage/Error/2.html");

                return View();
            }
        }
        #endregion

        #region ModifyPackage
        public ActionResult ModifyPackage()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 902))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                Validation vld = new Validation();
                int packageID = vld.GetInt("id");

                PackageBLL packageBLL = new PackageBLL();
                PackageObj packageObj = packageBLL.GetPackageByID(packageID);

                if (packageObj == null)
                    return Json(new { success = false, msg = "该套餐不存在" });

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
                packageObj.CategoryID = categoryIDs[0];
                packageObj.Categories = categoryIDs;
                packageObj.BuildingID = vld.GetInt("buildingID", false, "请填写楼盘");
                packageObj.Code = vld.Get("code", false, "请填写货号");
                packageObj.Intro = HttpUtility.UrlDecode(vld.Get("intro"), Encoding.UTF8);
                packageObj.Memo = vld.Get("memo");
                packageObj.PackageName = vld.Get("name", false, "请填写名称");
                packageObj.Material = vld.Get("material");
                string strPics = vld.Get("pics", false, "请上传商品图片", @"\d+(,\d+)*", "参数错误");

                if (vld.HasError)
                    return Json(new { success = false, msg = vld.GetError() });

                string[] pics = strPics.Split(',');
                PackagePictureObj pictureObj;
                packageObj.Pictures = new List<PackagePictureObj>();
                for (int i = 0; i < pics.Length; i++)
                {
                    pictureObj = new PackagePictureObj();
                    pictureObj.PictureID = int.Parse(pics[i]);
                    packageObj.Pictures.Add(pictureObj);
                }

                packageBLL.ModifyPackage(packageObj);

                return Json(new { success = true });
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 902))
                    return Redirect("/Manage/Error/2.html");

                ViewBag.mediaServer = Config.MediaServer;

                return View();
            }
        }
        #endregion

        #region DeletePackage
        public ActionResult DeletePackage()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 903))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            int packageID = vld.GetInt("id");

            PackageBLL packageBLL = new PackageBLL();
            PackageObj packageObj = packageBLL.GetPackageByID(packageID);

            if (packageObj == null)
                return Json(new { success = false, msg = "该套餐不存在" });

            packageBLL.DeletePackage(packageID);

            return Json(new { success = true });
        }
        #endregion

        #region GetPackageByID
        public ActionResult GetPackageByID()
        {
            Validation vld = new Validation();
            int packageID = vld.GetInt("id");

            PackageBLL packageBLL = new PackageBLL();
            var res = packageBLL.GetPackageByID(packageID);

            return Json(new { success = true, data = res });
        }
        #endregion

        #region InsertPackagePic
        public ActionResult InsertPackagePic()
        {
            ViewBag.success = false;
            if (!AppData.IsManagerLogin)
            {
                ViewBag.msg = "抱歉，您未登录后台或会话已过期!";
                return View();
            }
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 9))
            {
                ViewBag.msg = "抱歉，您没有执行该操作的权限!";
                return View();
            }

            Validation vld = new Validation();
            int picID = vld.GetInt("id");
            string desc = vld.Get("desc");

            HttpPostedFileBase pic = Request.Files.Count == 0 ? null : Request.Files[0];
            if (pic != null && !string.IsNullOrEmpty(pic.FileName))
            {
                string ext = Path.GetExtension(pic.FileName);
                if (!Regex.IsMatch(ext, @"^\.(gif|jpg|jpeg|png)$", RegexOptions.IgnoreCase))
                {
                    ViewBag.msg = "上传的图片格式不合要求,请上传gif,png,jpg格式的图片";
                    return View();
                }

                string dirPath = Config.MediaPath + @"\Package";
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff") + ext;
                string savePath = Path.Combine(dirPath, newFileName);

                PackagePictureObj packagePictureObj;
                PackageBLL packageBLL = new PackageBLL();
                if (picID == 0)
                {
                    packagePictureObj = new PackagePictureObj();
                    packagePictureObj.PackageID = string.IsNullOrEmpty(Request.Form["packageID"]) ? 0 : int.Parse(Request.Form["packageID"]);
                    packagePictureObj.SavePath = @"\Package\" + newFileName;
                    packagePictureObj.Url = "/Package/" + newFileName;
                    packagePictureObj.PictureDesc = desc;

                    packageBLL.AddPicture(packagePictureObj);
                }
                else
                {
                    packagePictureObj = packageBLL.GetPictureByID(picID);
                    System.IO.File.Delete(Config.MediaPath + packagePictureObj.SavePath);
                    packagePictureObj.SavePath = @"\Package\" + newFileName;
                    packagePictureObj.Url = "/Package/" + newFileName;
                    packagePictureObj.PictureDesc = desc;
                    packageBLL.ModifyPicture(packagePictureObj);
                }
                ViewBag.picId = packagePictureObj.PictureID;

                pic.SaveAs(savePath);

                ViewBag.success = true;
                return View();
            }
            else if (picID == 0)
            {
                ViewBag.msg = "请上传套餐图片!";
                return View();
            }
            else
            {
                ViewBag.picId = picID;
                ViewBag.success = true;
                return View();
            }

        }
        #endregion

        #region DeletePackagePic
        public ActionResult DeletePackagePic(int pictureID)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 9))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            PackageBLL packageBLL = new PackageBLL();
            PackagePictureObj packagePictureObj = packageBLL.GetPictureByID(pictureID);
            if (packagePictureObj == null)
                return Json(new { success = false, msg = "图片不存在" });

            System.IO.File.Delete(Config.MediaPath + packagePictureObj.SavePath);
            packageBLL.DeletePicture(pictureID);
            return Json(new { success = true });
        }
        #endregion


        #region Buildings
        public ActionResult Buildings()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 8))
                return Redirect("/Manage/Error/2.html");

            return View();
        }
        #endregion

        #region AddBuilding
        public ActionResult AddBuilding()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 801))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                Validation vld = new Validation();
                PackageBuildingObj packageBuildingObj = new PackageBuildingObj();
                packageBuildingObj.BuildingName = vld.Get("name", false, "请填写楼盘名称");
                packageBuildingObj.RegionID = vld.GetInt("regionID", false, "请选择省市区");

                if (vld.HasError)
                    return Json(new { success = false, msg = vld.GetError() });

                PackageBLL packageBLL = new PackageBLL();
                packageBLL.AddBuilding(packageBuildingObj);

                return Json(new { success = true });
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 801))
                    return Redirect("/Manage/Error/2.html");

                return View();
            }
        }
        #endregion

        #region ModifyBuilding
        public ActionResult ModifyBuilding()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 802))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                Validation vld = new Validation();
                int buildingID = vld.GetInt("id", false, "未传入楼盘ID");
                PackageBLL packageBLL = new PackageBLL();
                PackageBuildingObj packageBuildingObj = packageBLL.GetBuildingByID(buildingID);
                if (packageBuildingObj == null)
                    return Json(new { success = false, msg = "该楼盘不存在！" });

                packageBuildingObj.BuildingName = vld.Get("name", false, "请填写楼盘名称");
                packageBuildingObj.RegionID = vld.GetInt("regionID", false, "请选择省市区");

                if (vld.HasError)
                    return Json(new { success = false, msg = vld.GetError() });

                packageBLL.ModifyBuilding(packageBuildingObj);

                return Json(new { success = true });
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 802))
                    return Redirect("/Manage/Error/2.html");

                return View();
            }
        }
        #endregion

        #region DeleteBuilding
        public ActionResult DeleteBuilding()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 803))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            int buildingID = vld.GetInt("id", false, "未传入楼盘ID");
            PackageBLL packageBLL = new PackageBLL();
            PackageBuildingObj packageBuildingObj = packageBLL.GetBuildingByID(buildingID);
            if (packageBuildingObj == null)
                return Json(new { success = false, msg = "该楼盘不存在！" });

            if (vld.HasError)
                return Json(new { success = false, msg = vld.GetError() });

            packageBLL.DeleteBuilding(buildingID);

            return Json(new { success = true });
        }
        #endregion

        #region GetBuildingByID
        public ActionResult GetBuildingByID()
        {
            Validation vld = new Validation();
            int buildingID = vld.GetInt("id", false, "未传入楼盘ID");
            if (vld.HasError)
                return Json(new { success = false, msg = vld.GetError() });

            PackageBLL packageBLL = new PackageBLL();
            var data = packageBLL.GetJsonBuildingByID(buildingID);
            return Json(new { success = true, data = data });
        }
        #endregion

        #region GetBuildings
        public ActionResult GetBuildings()
        {
            Validation vld = new Validation();
            string keywords = vld.Get("keywords");
            int regionID = vld.GetInt("regionID");
            int cityID = vld.GetInt("cityID");
            int provinceID = vld.GetInt("provinceID");
            int page = vld.GetInt("page", defaultValue: 1);
            int pageSize = vld.GetInt("pageSize", defaultValue: 15);

            PackageBLL packageBLL = new PackageBLL();
            int total;
            var res = packageBLL.GetBuildings(keywords, regionID, cityID, provinceID, page, pageSize, out total);

            return Json(new { success = true, data = res, total = total });
        }
        #endregion

        #region GetBuildingsByRegionID
        public ActionResult GetBuildingsByRegionID()
        {
            Validation vld = new Validation();
            int regionID = vld.GetInt("regionID");

            PackageBLL packageBLL = new PackageBLL();
            var res = packageBLL.GetBuildings(regionID);

            return Json(new { success = true, data = res });
        }
        #endregion


        #region PackageDetails
        public ActionResult PackageDetails()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 904))
                return Redirect("/Manage/Error/2.html");

            return View();
        }
        #endregion

        #region GetPackageDetails
        public ActionResult GetPackageDetails()
        {
            Validation vld = new Validation();
            int packageID = vld.GetInt("id");

            PackageBLL packageBLL = new PackageBLL();
            var res = packageBLL.GetPackageDetails(packageID);

            return Json(new { success = true, data = res });
        }
        #endregion

        #region GetPackageDetailByID
        public ActionResult GetPackageDetailByID()
        {
            Validation vld = new Validation();
            int ppcID = vld.GetInt("id");

            PackageBLL packageBLL = new PackageBLL();
            var res = packageBLL.GetJsonPackageDetailByID(ppcID);

            return Json(new { success = true, data = res });
        }
        #endregion

        #region AddPackageDetail
        public ActionResult AddPackageDetail()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 904))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                Validation vld = new Validation();
                PackageProdCatesObj packageProdCatesObj = new PackageProdCatesObj();
                packageProdCatesObj.PackageID = vld.GetInt("packageID");
                packageProdCatesObj.PpcName = vld.Get("name", false, "请填写产品组名称");
                string sProducts = vld.Get("products", false, "请至少添加一个产品", regex: @"^\d+(,\d+)*$", regexText: "类别参数错误");

                if (vld.HasError)
                    return Json(new { success = false, msg = vld.GetError() });

                IList<int> products;
                if (string.IsNullOrEmpty(sProducts))
                {
                    products = null;
                }
                else
                {
                    string[] aCategoryIDs = sProducts.Split(',');
                    products = new List<int>();
                    for (int i = 0; i < aCategoryIDs.Length; i++)
                    {
                        products.Add(int.Parse(aCategoryIDs[i]));
                    }
                }
                packageProdCatesObj.PackageProducts = products;

                PackageBLL packageBLL = new PackageBLL();
                packageBLL.AddPackageProdCate(packageProdCatesObj);

                return Json(new { success = true });
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 904))
                    return Redirect("/Manage/Error/2.html");

                return View();
            }
        }
        #endregion

        #region ModifyPackageDetail
        public ActionResult ModifyPackageDetail()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 904))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                PackageBLL packageBLL = new PackageBLL();
                Validation vld = new Validation();
                int ppcId = vld.GetInt("id");

                PackageProdCatesObj packageProdCatesObj = packageBLL.GetPackageDetailByID(ppcId);

                //packageProdCatesObj.PackageID = vld.GetInt("packageID");
                packageProdCatesObj.PpcName = vld.Get("name", false, "请填写产品组名称");
                string sProducts = vld.Get("products", false, "请至少添加一个产品", regex: @"^\d+(,\d+)*$", regexText: "类别参数错误");

                if (vld.HasError)
                    return Json(new { success = false, msg = vld.GetError() });

                IList<int> products;
                if (string.IsNullOrEmpty(sProducts))
                {
                    products = null;
                }
                else
                {
                    string[] aCategoryIDs = sProducts.Split(',');
                    products = new List<int>();
                    for (int i = 0; i < aCategoryIDs.Length; i++)
                    {
                        products.Add(int.Parse(aCategoryIDs[i]));
                    }
                }
                packageProdCatesObj.PackageProducts = products;

                packageBLL.ModifyPackageProdCate(packageProdCatesObj);

                return Json(new { success = true });
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 904))
                    return Redirect("/Manage/Error/2.html");

                return View();
            }
        }
        #endregion

        #region DeletePackageDetail
        public ActionResult DeletePackageDetail()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 904))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            int ppcId = vld.GetInt("id");

            PackageBLL packageBLL = new PackageBLL();
            packageBLL.DeletePackageDetails(ppcId);

            return Json(new { success = true });
        }
        #endregion


    }
}
