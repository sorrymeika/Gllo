using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Gllo.Sys;
using Gllo.BLL;
using Gllo.Models;

namespace Gllo.Web.Areas.Manage.Controllers
{
    public class AreaController : Controller
    {
        #region [页面] 地区管理
        public ActionResult Area()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");

            return View();
        }
        #endregion

    }
}
