using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Gllo.Sys;
using Gllo.BLL;
using Gllo.Models;
using System.Text;

namespace Gllo.Web.Manage.Controllers
{
    public class UserController : Controller
    {
        #region [页面] 会员管理
        public ActionResult Users()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 13))
                return Redirect("/Manage/Error/2.html");

            return View();
        }
        #endregion

        public ActionResult GetUsers()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 13))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            string keywords = vld.Get("keywords");
            DateTime regDateFrom = vld.GetDateTime("regDateFrom");
            DateTime regDateTo = vld.GetDateTime("regDateTo");
            int page = vld.GetInt("page", defaultValue: 1);
            int pageSize = vld.GetInt("pageSize", defaultValue: 15);

            UserBLL userBLL = new UserBLL();

            int total;
            JsonArray result = userBLL.GetUsers(UserRoles.Customer, keywords, regDateFrom, regDateTo, page, pageSize, out total);
            return Json(new { success = true, data = result });
        }

        public ActionResult DeleteUser()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1301))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            UserBLL userBLL = new UserBLL();
            Validation vld = new Validation();
            int id = vld.GetInt("id");

            var user = userBLL.GetUserByID(id);
            if (user == null)
                return Json(new { success = false, msg = "该用户不存在" });

            userBLL.DeleteUser(id);
            return Json(new { success = true });
        }


        public ActionResult ResetPassword()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1302))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            UserBLL userBLL = new UserBLL();
            Validation vld = new Validation();
            int id = vld.GetInt("id");

            var user = userBLL.GetUserByID(id);
            if (user == null)
                return Json(new { success = false, msg = "该用户不存在" });

            user.Password = Md5.MD5("123456");
            userBLL.ModifyUser(user);
            return Json(new { success = true });
        }

        public ActionResult ModifyUser(int id)
        {
            UserBLL userBLL = new UserBLL();
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1303))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                Validation vld = new Validation();
                var user = userBLL.GetUserByID(id);
                if (user == null)
                    return Json(new { success = false, msg = "该用户不存在" });

                user.NickName = vld.Get("nickName");
                user.BirthDay = vld.GetDateTime("birthDay");
                user.Gender = vld.GetBool("gender");

                userBLL.ModifyUser(user);
                return Json(new { success = true });
            }

            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1303))
                return Redirect("/Manage/Error/2.html");

            ViewBag.userInfo = userBLL.GetUserByID(id);

            return View();
        }

        public ActionResult ModifyAddress(int id)
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1304))
                return Redirect("/Manage/Error/2.html");

            return View();
        }

        #region 获取配送地址
        public ActionResult GetAddress(int id)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 13))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            UserBLL userBLL = new UserBLL();

            var result = userBLL.GetUserAddress(id);

            return Json(new { success = true, data = result });
        }
        #endregion

        #region 保存配送地址
        public ActionResult SaveAddress(int id)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1304))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            UserBLL userBLL = new UserBLL();
            Validation validation = new Validation();
            int addressId = validation.GetInt("addressId");
            AddressObj addressObj;
            if (addressId == 0)
            {
                addressObj = new AddressObj();
                addressObj.AddressID = addressId;
                addressObj.UserID = id;
            }
            else
            {
                addressObj = userBLL.GetAddress(addressId);
            }

            addressObj.Receiver = validation.Get("receiver", false, "收货人姓名不可为空！");
            addressObj.Address = validation.Get("address", false, "详细地址不可为空！");
            addressObj.Zip = validation.Get("zip");
            addressObj.Mobile = validation.Get("mobile");
            addressObj.Phone = validation.Get("phone");
            addressObj.CityID = validation.GetInt("cityId");
            addressObj.RegionID = validation.GetInt("regionId");

            if (validation.HasError)
                return Json(new { success = false, msg = "参数错误！", errors = validation.GetErrors() });

            if (string.IsNullOrEmpty(addressObj.Phone) && string.IsNullOrEmpty(addressObj.Mobile))
                return Json(new { success = false, msg = "手机或固话必填一项！" });

            if (addressObj.AddressID == 0)
            {
                userBLL.AddAddress(addressObj);
            }
            else
            {
                userBLL.ModifyAddress(addressObj);
            }

            return Json(new { success = true });
        }
        #endregion

        #region 删除配送地址
        public ActionResult DeleteAddress(int id, int addressId)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1305))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            UserBLL userBLL = new UserBLL();
            AddressObj addressObj = userBLL.GetAddress(addressId);
            if (addressObj == null)
                return Json(new { success = false, msg = "您无法删除该地址！" });

            userBLL.DeleteAddress(id, addressId);
            return Json(new { success = true });
        }
        #endregion

        public ActionResult PM()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 21))
                return Redirect("/Manage/Error/2.html");

            return View();
        }

        public ActionResult GetPM()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 21))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();

            dynamic result;
            PM pm = new PM();
            int pmID = vld.GetInt("pmID");
            if (pmID != 0)
            {
                result = pm.GetPM(pmID);
                return Json(new { success = true, data = result });
            }
            else
            {
                int page = vld.GetInt("page");
                int pageSize = vld.GetInt("pageSize");
                string userName = vld.Get("userName");
                DateTime dtFrom = vld.GetDateTime("dtFrom");
                DateTime dtTo = vld.GetDateTime("dtTo");
                int total;
                result = pm.GetPM(page, pageSize, out total, userName: userName, dtFrom: dtFrom, dtTo: dtTo);
                return Json(new { success = true, data = result, total = total });
            }

        }

        public ActionResult AddPM()
        {
            if (Request.HttpMethod == "GET")
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 2101))
                    return Redirect("/Manage/Error/2.html");

                return View();
            }

            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 2101))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            PMInfo pmInfo = new PMInfo()
            {
                UserID = vld.GetInt("userID"),
                UserIDFrom = AppData.SessionUser.UserID,
                Title = vld.Get("title"),
                Content = HttpUtility.UrlDecode(vld.Get("content"), Encoding.UTF8),
                PMTime = DateTime.Now
            };

            UserBLL userBLL = new UserBLL();
            PM pm = new PM();
            pm.AddPM(pmInfo);

            return Json(new { success = true });
        }


        public ActionResult ModifyPM()
        {
            if (Request.HttpMethod == "GET")
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 2103))
                    return Redirect("/Manage/Error/2.html");

                return View();
            }

            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 2103))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            int pmID = vld.GetInt("id");
            PM pm = new PM();
            PMInfo pmInfo = pm.GetPM(pmID);
            if (pmInfo == null)
            {
                return Json(new { success = false, msg = "该短消息不存在！" });
            }
            pmInfo.Title = vld.Get("title");
            pmInfo.Content = HttpUtility.UrlDecode(vld.Get("content"), Encoding.UTF8);
            pmInfo.UserIDFrom = AppData.SessionUser.UserID;

            UserBLL userBLL = new UserBLL();
            pm.ModifyPM(pmInfo);

            return Json(new { success = true });
        }

        public ActionResult DeletePM(int id)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 2102))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            PM pm = new PM();
            PMInfo pmInfo = pm.GetPM(id);
            if (pmInfo == null)
            {
                return Json(new { success = false, msg = "该短消息不存在！" });
            }

            pm.DeletePM(id);
            return Json(new { success = true });
        }
    }
}
