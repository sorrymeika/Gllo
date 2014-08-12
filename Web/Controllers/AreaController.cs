using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Gllo.BLL;
using Gllo.Models;

namespace Gllo.Web.Controllers
{
    public class AreaController : Controller
    {
        #region 获取省
        public ActionResult GetProvince()
        {
            AreaBLL areaBLL = new AreaBLL();
            JsonArray result = areaBLL.GetProvince();
            return Json(new { success = true, data = result });
        }
        #endregion

        #region 获取市
        public ActionResult GetCity(int provinceId)
        {
            AreaBLL areaBLL = new AreaBLL();
            JsonArray result = areaBLL.GetCityByProvId(provinceId);
            return Json(new { success = true, data = result });
        }
        #endregion

        #region 获取区
        public ActionResult GetRegion(int cityId)
        {
            AreaBLL areaBLL = new AreaBLL();
            JsonArray result = areaBLL.GetRegionByCityId(cityId);
            return Json(new { success = true, data = result });
        }
        #endregion

    }
}
