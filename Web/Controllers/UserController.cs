using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Gllo.Sys;
using Gllo.BLL;
using Gllo.Models;
using System.Collections;
using System.Collections.Specialized;

namespace Gllo.Web.Controllers
{
    public class UserController : Controller
    {
        #region 判断是否登录
        public ActionResult IsUserLogin()
        {
            if (AppData.SessionUser == null)
                return Json(new { success = false });

            return Json(new { success = true });
        }
        #endregion

        #region 用户登录
        public ActionResult Login()
        {
            UserObj user;
            if (Request.HttpMethod == "POST")
            {
                string sessionCheckCode = Session["CheckCode"] == null ? null : Session["CheckCode"].ToString();
                Session["CheckCode"] = null;

                if (sessionCheckCode == null)
                    return Json(new { success = false, msg = "验证码已经过期，请刷新验证码！" });

                user = new UserObj();
                Validation validation = new Validation();
                string checkCode = validation.Get("checkCode", emptyAble: false, emptyText: "请填写验证码", compare: sessionCheckCode, compareText: "验证码错误！");
                if (validation.HasError)
                    return Json(new { success = false, msg = validation.GetErrors()["checkCode"] });

                user.UserName = validation.Get("userName", emptyAble: false, emptyText: "请填写用户名");
                user.Password = validation.Get("password", emptyAble: false, emptyText: "请填写密码");

                if (validation.HasError)
                    return Json(new { success = false, msg = "参数错误", errors = validation.GetErrors() });
                else
                {

                    bool result;
                    UserBLL bll = new UserBLL();

                    result = true;
                    UserObj cU = bll.GetUserByUserName(user.UserName);
                    if (cU == null)
                    {
                        user.Password = Md5.MD5(user.Password);
                        bll.Register(user);
                    }
                    else
                    {
                        if (cU.Password != Md5.MD5(user.Password))
                        {
                            cU.Password = Md5.MD5(user.Password);
                            bll.ModifyUser(cU);
                        }
                        user = cU;
                    }

                    if (result)
                    {
                        AppData.SessionUser = user;
                        return Json(new { success = true });
                    }
                    else
                    {
                        AppData.SessionUser = null;
                        return Json(new { success = false, msg = "用户名或密码错误,登录失败" });
                    }
                }
            }
            else
            {
                return View();
            }
        }
        #endregion

        public ActionResult IsRegist(string userName)
        {
            UserBLL bll = new UserBLL();

            return Json(new { success = true, exists = bll.IsRegist(userName) });
        }

        #region 用户注册
        public ActionResult Register()
        {
            if (Request.HttpMethod == "POST")
            {
                string sessionCheckCode = Session["CheckCode"] == null ? null : Session["CheckCode"].ToString();
                Session["CheckCode"] = null;

                if (sessionCheckCode == null)
                    return Json(new { success = false, msg = "验证码已经过期，请刷新验证码！" });

                Validation validation = new Validation();
                UserObj user = new UserObj();
                string checkCode = validation.Get("checkCode", emptyAble: false, emptyText: "请填写验证码", compare: sessionCheckCode, compareText: "验证码错误！");
                user.UserName = validation.Get("userName", emptyAble: false, emptyText: "请填写用户名", regex: @"[a-zA-Z0-9]{4,20}", regexText: "用户名格式错误");
                user.Password = validation.Get("password", emptyAble: false, emptyText: "请填写密码", regex: @"[a-zA-Z0-9]{6,20}", regexText: "密码格式错误");
                user.Email = validation.Get("email", emptyAble: false, emptyText: "请填写邮箱", regex: @"^[\w\._-]+@[\w\._-]+$", regexText: "邮箱格式错误");
                user.UserRole = UserRoles.Customer;

                if (validation.HasError)
                    return Json(new { success = false, msg = "参数错误", errors = validation.GetErrors() });


                UserBLL bll = new UserBLL();
                if (bll.IsRegist(user.UserName))
                    return Json(new { success = false, msg = "该用户名已被注册" });

                user.Password = Md5.MD5(user.Password);
                bll.Register(user);

                if (user.UserID != 0)
                {
                    AppData.SessionUser = user;
                    return Json(new { success = true });
                }
                else
                {
                    AppData.SessionUser = null;
                    return Json(new { success = false, msg = "抱歉，注册失败！" });
                }
            }
            else
                return View();
        }
        #endregion

        #region 获取配送地址
        public ActionResult GetAddress()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Json(new { success = false, msg = "您还未登录！" });

            UserBLL userBLL = new UserBLL();

            var result = userBLL.GetUserAddress(user.UserID);

            return Json(new { success = true, data = result });
        }
        #endregion

        #region 保存配送地址
        public ActionResult SaveAddress()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Json(new { success = false, msg = "您还未登录！" });

            UserBLL userBLL = new UserBLL();
            Validation validation = new Validation();
            int addressId = validation.GetInt("addressId");
            AddressObj addressObj;
            if (addressId == 0)
            {
                addressObj = new AddressObj();
                addressObj.AddressID = addressId;
                addressObj.UserID = user.UserID;
            }
            else
            {
                addressObj = userBLL.GetAddress(addressId);
                if (addressObj == null || addressObj.UserID != user.UserID)
                    return Json(new { success = false, msg = "您无法修改该地址！" });
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
        public ActionResult DeleteAddress(int addressId)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Json(new { success = false, msg = "您还未登录！" });

            UserBLL userBLL = new UserBLL();
            AddressObj addressObj = userBLL.GetAddress(addressId);
            if (addressObj == null || addressObj.UserID != user.UserID)
                return Json(new { success = false, msg = "您无法删除该地址！" });

            userBLL.DeleteAddress(user.UserID, addressId);
            return Json(new { success = true });
        }
        #endregion

        #region 加入收藏夹
        public ActionResult AddFav(string ids, int type)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Json(new { success = false, msg = "IS_NOT_LOGIN" });

            string[] aIds = ids.Split(',');

            UserBLL userBLL = new UserBLL();
            for (int i = 0; i < aIds.Length; i++)
            {
                if (!string.IsNullOrEmpty(aIds[i]))
                {
                    userBLL.AddFavorite(user.UserID, int.Parse(aIds[i]), type);
                }
            }
            return Json(new { success = true });
        }
        #endregion

        #region [页面] 我的账户
        public ActionResult Account()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Redirect("/Login.html");
            }

            return View();
        }
        #endregion

        #region [页面] 我的订单
        public ActionResult Orders()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Redirect("/Login.html");
            }

            return View();
        }
        #endregion

        #region [页面] 我的收藏
        public ActionResult Fav()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Redirect("/Login.html");
            }

            return View();
        }
        #endregion

        #region 获取收藏的商品
        public ActionResult GetFavs()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Json(new { success = false, msg = "您还未登录" });
            }

            Validation vld = new Validation();
            int page = vld.GetInt("page", defaultValue: 1);
            int pageSize = vld.GetInt("pageSize", defaultValue: 10);

            UserBLL userBLL = new UserBLL();
            int total;
            var res = userBLL.GetFav(user.UserID, page, pageSize, out total);
            return Json(new { success = true, data = res, total = total });
        }
        #endregion

        #region 删除收藏的商品
        public ActionResult DelFav()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Json(new { success = false, msg = "您还未登录" });
            }

            Validation vld = new Validation();
            int id = vld.GetInt("id");

            UserBLL userBLL = new UserBLL();
            userBLL.DelFav(user.UserID, id);
            return Json(new { success = true });
        }
        #endregion

        #region [页面] 个人设置
        public ActionResult Info()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Redirect("/Login.html");
            }
            UserBLL userBLL = new UserBLL();
            ViewBag.userInfo = userBLL.GetUserByID(user.UserID);

            return View();
        }
        #endregion

        #region [页面] 修改密码
        public ActionResult Password()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Redirect("/Login.html");
            }

            return View();
        }
        #endregion

        #region [页面] 信息管理
        public ActionResult Msgs()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Redirect("/Login.html");
            }

            return View();
        }
        #endregion

        #region [页面] 信息
        public ActionResult Msg()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Redirect("/Login.html");
            }

            return View();
        }
        #endregion

        #region [页面] 我的评论
        public ActionResult Comments()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Redirect("/Login.html");
            }

            return View();
        }
        #endregion

        #region 修改基本信息
        public ActionResult ModifyUser()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Json(new { success = false, msg = "您还未登录" });
            }

            UserBLL userBLL = new UserBLL();
            UserObj userObj = userBLL.GetUserByID(user.UserID);

            Validation vld = new Validation();
            userObj.NickName = vld.Get("nickName");
            userObj.BirthDay = vld.GetDateTime("birthDay");
            userObj.Gender = vld.GetBool("gender");

            userBLL.ModifyUser(userObj);

            return Json(new { success = true });

        }
        #endregion

        #region 修改密码
        public ActionResult ModifyPassword()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Json(new { success = false, msg = "您还未登录" });
            }

            UserBLL userBLL = new UserBLL();
            UserObj userObj = userBLL.GetUserByID(user.UserID);

            Validation vld = new Validation();
            string oldPassword = vld.Get("oldPassword");
            string newPassword = vld.Get("newPassword");

            oldPassword = Md5.MD5(oldPassword);

            if (oldPassword.ToUpper() != userObj.Password.ToUpper())
            {
                return Json(new { success = false, msg = "旧密码输入不正确！" });
            }

            userObj.Password = Md5.MD5(newPassword);

            userBLL.ModifyUser(userObj);

            return Json(new { success = true });
        }
        #endregion

        #region 获取评论
        public ActionResult GetComments(int page, int pageSize)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Json(new { success = false, msg = "您还未登录" });
            }
            ProductBLL productBLL = new ProductBLL();

            int total;
            var res = productBLL.GetCommentsByUserID(user.UserID, page, pageSize, out total);
            return Json(new { success = true, data = res, total = total });
        }
        #endregion

        public ActionResult ReturnExchangeList()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Redirect("/Login.html");
            }

            return View();
        }

        public ActionResult GetExchange(int page, int pageSize)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Json(new { success = false, msg = "您还未登录" });
            }

            ExchangeBLL exchangeBLL = new ExchangeBLL();

            int total;

            var data = exchangeBLL.GetExchange(page, pageSize, out total, user.UserID);

            return Json(new { success = true, total = total, data = data });

        }

        public ActionResult UserExchange(int id)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Redirect("/Login.html");

            ExchangeBLL exchangeBLL = new ExchangeBLL();
            var exchangeInfo = exchangeBLL.GetExchangeByID(id);
            if (exchangeInfo == null)
                return Redirect("/Error/2.html");

            OrderBLL orderBLL = new OrderBLL();
            ProductBLL productBLL = new ProductBLL();
            AreaBLL areaBLL = new AreaBLL();

            OrderObj orderObj = orderBLL.GetOrderByID(exchangeInfo.OrderID);
            if (orderObj == null || user.UserID != orderObj.UserID)
                return Redirect("/Error/2.html");

            ViewBag.returnProduct = productBLL.GetProduct(exchangeInfo.ReturnProductID);


            if (exchangeInfo.ExchangeProductID != 0)
            {
                ViewBag.exchangeProduct = productBLL.GetProduct(exchangeInfo.ExchangeProductID);
            }

            ViewBag.exchangeInfo = exchangeInfo;
            ViewBag.order = orderObj;
            ViewBag.area = areaBLL.GetAreaByRetionID(exchangeInfo.RegionID);

            return View();
        }


        public ActionResult DeletePM(int id)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Json(new { success = false, msg = "您还未登录" });
            }

            PM pm = new PM();
            PMInfo pmInfo = pm.GetPM(id);
            if (pmInfo == null)
            {
                return Json(new { success = false, msg = "该短消息不存在！" });
            }
            if (pmInfo.UserID != user.UserID)
            {
                return Json(new { success = false, msg = "该短消息不存在1！" });
            }

            pm.DeletePM(id);
            return Json(new { success = true });
        }

        public ActionResult PMList()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Redirect("/Login.html");
            }
            return View();
        }

        public ActionResult PM(int id)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Redirect("/Login.html");
            }
            PM pm = new PM();

            pm.ReadPM(id, user.UserID);

            ViewBag.pmInfo = pm.GetPM(id);
            return View();
        }

        [HttpPost]
        public ActionResult GetPM(int page, int pageSize)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Json(new { success = false, msg = "您还未登录" });
            }
            PM pm = new PM();

            int total;
            var res = pm.GetPM(page, pageSize, out total, userID: (int?)user.UserID);
            return Json(new { success = true, data = res, total = total });
        }

    }
}
