using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Gllo.BLL;
using Gllo.Sys;
using System.Text;
using Gllo.Models;

namespace Gllo.Web.Controllers
{
    public class PackageController : Controller
    {
        public ActionResult Index(string id)
        {
            int cateID;
            int startPage;
            if (id.IndexOf('_') == -1)
            {
                cateID = int.Parse(id);
                startPage = 1;
            }
            else
            {
                string[] strId = id.Split('_');
                cateID = int.Parse(strId[0]);
                startPage = int.Parse(strId[1]);
            }
            ViewBag.page = startPage;
            ViewBag.categoryID = cateID;

            PackageBLL packageBLL = new PackageBLL();
            ViewBag.allCates = packageBLL.GetPackageCates();

            int pageSize = 24;
            int total;
            IList<int> categorieIDs = new List<int>() { cateID };
            ViewBag.data = packageBLL.GetPackages(categorieIDs, null, 0, 0, 0, 0, startPage, pageSize, out total);
            ViewBag.pageSize = pageSize;
            ViewBag.total = total;
            ViewBag.totalPages = total % pageSize == 0 ? total / pageSize : (total / pageSize + 1);

            return View();
        }

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

        public ActionResult Package(int id)
        {
            PackageBLL packageBLL = new PackageBLL();
            ViewBag.allCates = packageBLL.GetPackageCates();

            PackageObj packageObj = packageBLL.GetPackageByID(id);
            ViewBag.package = packageObj;

            return View();
        }

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

            ProductBLL productBLL = new ProductBLL();
            int totalRecommends;
            ViewBag.recommends = productBLL.GetProducts(0, null, 0, 0, 1, -1, -1, -1, 1, 10, out  totalRecommends);

            return Json(new { success = true, data = res, total = total });
        }
        #endregion

        public ActionResult GetPackageDetails(int id)
        {
            PackageBLL packageBLL = new PackageBLL();
            JsonArray res = packageBLL.GetPackageDetails(id);
            return Json(new { success = true, data = res });
        }

        public ActionResult PotoT(int id)
        {
            ViewBag.id = id;
            return View();
        }

        public ActionResult PotoTSwf(int id)
        {
            return File(Server.MapPath("~/Content/Show/potoT.swf"), "application/octet-stream");
        }

        public ActionResult PotoT1XML(int id)
        {
            StringBuilder sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n");
            sb.AppendLine("<root>");

            PhotoBLL photoBLL = new PhotoBLL();
            JsonArray res = photoBLL.GetPhotos(new List<int>() { id });

            if (res != null)
            {
                for (var i = 0; i < res.Count; i++)
                {
                    sb.Append("<list id=\"")
                        .Append(res[i]["PhotoID"])
                        .Append("\" pic=\"")
                        .Append(Config.MediaServer)
                        .Append(res[i]["Pic"])
                        .Append("\" name=\"")
                        .Append(res[i]["Name"] == null ? null : HttpUtility.HtmlEncode((string)res[i]["Name"]))
                        .Append("\" info=\"")
                        .Append(res[i]["Info"] == null ? null : HttpUtility.HtmlEncode((string)res[i]["Info"]))
                        .AppendLine("\" />");

                }
            }

            sb.Append("</root>");
            return Content(sb.ToString(), "text/xml");
        }

        public ActionResult PotoT3XML(int id)
        {
            StringBuilder sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n");
            sb.AppendLine("<root>");
            sb.Append("</root>");
            return Content(sb.ToString(), "text/xml");
        }

        public ActionResult PotoT4XML(int id)
        {
            StringBuilder sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n");
            sb.AppendLine("<root>");
            sb.Append("</root>");
            return Content(sb.ToString(), "text/xml");
        }

        public ActionResult PotoT2XML(int id)
        {
            StringBuilder sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>\r\n");
            sb.AppendLine("<root>");

            PhotoBLL photoBLL = new PhotoBLL();
            JsonArray res = photoBLL.GetPhotos(new List<int>() { id });

            if (res != null)
            {
                for (var i = 0; i < res.Count; i++)
                {
                    if (res[i]["Points"] != null)
                    {
                        JsonArray points = (JsonArray)res[i]["Points"];
                        for (var j = 0; j < points.Count; j++)
                        {
                            sb.Append("<list id=\"")
                                .Append(points[j]["PointID"])
                                .Append("\" hId=\"")
                                .Append(res[i]["PhotoID"])
                                .Append("\" pic=\"")
                                .Append(Config.MediaServer)
                                .Append(points[j]["Pic"])
                                .Append("\" name=\"")
                                .Append(points[j]["Name"] == null ? null : HttpUtility.HtmlEncode((string)points[j]["Name"]))
                                .Append("\" place=\"")
                                .Append(points[j]["X"] + "," + points[j]["Y"])
                                .Append("\" cap=\"")
                                .Append(points[j]["Cap"] == null ? null : HttpUtility.HtmlEncode((string)points[j]["Cap"]))
                                .AppendLine("\" />");
                        }
                    }

                }
            }

            sb.Append("</root>");
            return Content(sb.ToString(), "text/xml");
        }


    }
}
