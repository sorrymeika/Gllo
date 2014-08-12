using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gllo.Sys;
using Gllo.BLL;
using Gllo.Models;

namespace Gllo.Web.Manage.Controllers
{
    public class ExpressController : Controller
    {
        public ActionResult AddExpress()
        {
            if (Request.HttpMethod != "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1201))
                    return Redirect("/Manage/Error/2.html");

                return View();
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1201))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                ExpressBLL expressBLL = new ExpressBLL();

                Validation vld = new Validation();
                ExpressObj expressObj = new ExpressObj();
                expressObj.ExpressName = vld.Get("name", false);
                expressObj.ExpressUrl = vld.Get("url");

                if (vld.HasError)
                    return Json(new { success = false, msg = "必须填写快递名称" });

                expressBLL.AddExpress(expressObj);

                return Json(new { success = true });
            }
        }

        public ActionResult ModifyExpress()
        {
            if (Request.HttpMethod != "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1202))
                    return Redirect("/Manage/Error/2.html");

                Validation vld = new Validation(false);
                int id = vld.GetInt("id");

                ExpressBLL expressBLL = new ExpressBLL();
                ExpressObj expressObj = expressBLL.GetExpress(id);

                ViewBag.id = id;
                ViewBag.name = expressObj.ExpressName;
                ViewBag.url = expressObj.ExpressUrl;

                return View();
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1202))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                ExpressBLL expressBLL = new ExpressBLL();

                Validation vld = new Validation();
                int id = vld.GetInt("id");
                ExpressObj expressObj = expressBLL.GetExpress(id);
                expressObj.ExpressName = vld.Get("name", false);
                expressObj.ExpressUrl = vld.Get("url");

                if (vld.HasError)
                    return Json(new { success = false, msg = "必须填写快递名称" });

                expressBLL.ModifyExpress(expressObj);

                return Json(new { success = true });
            }
        }

        public ActionResult ExpressList()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 12))
                return Redirect("/Manage/Error/2.html");

            return View();
        }

        public ActionResult GetExpress()
        {
            Validation vld = new Validation();
            int express = vld.GetInt("id");

            ExpressBLL expressBLL = new ExpressBLL();
            dynamic result;
            if (express == 0)
                result = expressBLL.GetExpress();
            else
                result = expressBLL.GetExpress(express);

            return Json(new { success = true, data = result });
        }

        public ActionResult DeleteExpress()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1203))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            ExpressBLL expressBLL = new ExpressBLL();

            Validation vld = new Validation();
            int id = vld.GetInt("id");
            if (id != 0)
                expressBLL.DeleteExpress(id);

            return Json(new { success = true });
        }


    }
}

