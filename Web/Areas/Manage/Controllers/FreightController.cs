using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gllo.BLL;
using Gllo.Sys;
using Gllo.Models;

namespace Gllo.Web.Manage.Controllers
{
    public class FreightController : Controller
    {
        public ActionResult FreightModelList()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 22))
                return Redirect("/Manage/Error/2.html");

            ViewBag.express = new ExpressBLL().GetExpress();

            return View();
        }

        public ActionResult GetFreightModelList()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 22))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            string keywords = vld.Get("keywords");
            int expressID = vld.GetInt("expressID");
            int page = vld.GetInt("page");
            int pageSize = vld.GetInt("pageSize");

            FreightModel freightModel = new FreightModel();

            int total;
            var res = freightModel.GetModels(page, pageSize, out total, expressID, keywords);
            return Json(new { success = true, data = res, total = total });
        }

        public ActionResult AddFreightModel()
        {
            if (Request.HttpMethod == "GET")
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 2201))
                    return Redirect("/Manage/Error/2.html");

                ViewBag.express = new ExpressBLL().GetExpress();

                return View();
            }

            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 2201))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            FreightModelInfo freightModelInfo = new FreightModelInfo();
            freightModelInfo.AreaFreightList = new List<AreaFreightInfo>();

            Validation vld = new Validation();
            freightModelInfo.ModelName = vld.Get("name");
            freightModelInfo.ExpressID = vld.GetInt("expressID");
            freightModelInfo.Freight = vld.GetInt("freight");
            freightModelInfo.Freight1 = vld.GetInt("freight1");
            var data = vld.Get("data");
            if (!string.IsNullOrEmpty(data))
            {
                var datas = data.Split('|');
                datas.All<string>(s =>
                {
                    var a = s.Split('-');
                    freightModelInfo.AreaFreightList.Add(new AreaFreightInfo()
                    {
                        AreaType = byte.Parse(a[0]),
                        AreaID = a[1],
                        Freight = decimal.Parse(a[2]),
                        Freight1 = decimal.Parse(a[3]),
                    });
                    return true;
                });
            }

            FreightModel freightModel = new FreightModel();
            freightModel.AddModel(freightModelInfo);

            return Json(new { success = true });
        }

        public ActionResult GetFreightModel(int id)
        {
            FreightModel freightModel = new FreightModel();
            return Json(new { success = true, data = freightModel.GetModel(id) });
        }

        public ActionResult DeleteFreightModel(int id)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 2203))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });
            FreightModel freightModel = new FreightModel();

            freightModel.DeleteFreightModel(id);

            return Json(new { success = true });
        }

        public ActionResult ModifyFreightModel(int id)
        {
            FreightModel freightModel = new FreightModel();
            Validation vld = new Validation();

            if (Request.HttpMethod == "GET")
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 2202))
                    return Redirect("/Manage/Error/2.html");

                ViewBag.express = new ExpressBLL().GetExpress();
                ViewBag.model = freightModel.GetModel(id);

                return View();
            }

            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 2202))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            FreightModelInfo freightModelInfo = freightModel.GetModel(id);
            freightModelInfo.AreaFreightList = new List<AreaFreightInfo>();

            freightModelInfo.ModelName = vld.Get("name");
            freightModelInfo.ExpressID = vld.GetInt("expressID");
            freightModelInfo.Freight = vld.GetInt("freight");
            freightModelInfo.Freight1 = vld.GetInt("freight1");
            var data = vld.Get("data");
            if (!string.IsNullOrEmpty(data))
            {
                var datas = data.Split('|');
                datas.All<string>(s =>
                {
                    var a = s.Split('-');
                    freightModelInfo.AreaFreightList.Add(new AreaFreightInfo()
                    {
                        ModelID = freightModelInfo.ModelID,
                        AreaType = byte.Parse(a[0]),
                        AreaID = a[1],
                        Freight = decimal.Parse(a[2]),
                        Freight1 = decimal.Parse(a[3]),
                    });
                    return true;
                });
            }

            freightModel.ModifyModel(freightModelInfo);

            return Json(new { success = true });
        }


        public ActionResult GetArea()
        {
            AreaBLL areaBLL = new AreaBLL();
            var r = areaBLL.GetRegions();
            var c = areaBLL.GetCities();
            var p = areaBLL.GetProvince();
            return Json(new { success = true, r = r, c = c, p = p });
        }

    }
}
