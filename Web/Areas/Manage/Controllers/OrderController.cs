using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

using Gllo.Sys;
using Gllo.BLL;
using Gllo.Models;
using System.Xml;
using AlipayClass;

namespace Gllo.Web.Manage.Controllers
{
    public class OrderController : Controller
    {
        public ActionResult Orders()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 11))
                return Redirect("/Manage/Error/2.html");

            return View();
        }

        #region 查询订单
        public ActionResult GetOrders()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 11))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            string receiver = validation.Get("receiver");
            int orderId = validation.GetInt("orderId");
            string orderCode = validation.Get("orderCode");
            DateTime dtFrom = validation.GetDateTime("dtFrom");
            DateTime dtTo = validation.GetDateTime("dtTo");
            int payStatus = validation.GetInt("payStatus", defaultValue: -1);
            int status = validation.GetInt("status", defaultValue: -1);
            int isSurvey = validation.GetInt("isSurvey", defaultValue: -1);
            int page = validation.GetInt("page", defaultValue: 1);
            int pageSize = validation.GetInt("pageSize", defaultValue: 10);

            OrderBLL orderBLL = new OrderBLL();
            int total;
            JsonArray result = orderBLL.GetOrders(0, orderId, orderCode, isSurvey, dtFrom, dtTo, payStatus, status, receiver, page, pageSize, out total);

            return Json(new { success = true, total = total, data = result, express = new ExpressBLL().GetExpress() });
        }
        #endregion

        #region [页面] 修改订单
        public ActionResult OrderModify(int id)
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1101))
                return Redirect("/Manage/Error/2.html");

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(id);
            if (orderObj.Status == 2)
                return Content("已取消的订单不可修改！");

            ViewBag.orderInfo = orderObj;
            if (orderObj.PayStatus == 0)
            {
                return View("OrderModify");
            }
            else
            {
                return View("PayedOrderModify");
            }

        }
        #endregion

        #region ModifyLogistic
        public ActionResult ModifyLogistic()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1102))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            int orderId = validation.GetInt("orderId");

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(orderId);
            if (orderObj == null)
                return Json(new { success = false, msg = "该订单不存在" });
            if (orderObj.PayStatus != 1)
                return Json(new { success = false, msg = "未付款的订单不可发货" });

            orderObj.Logistics = validation.Get("logistics");
            orderObj.LogisticsCode = validation.Get("logisticsCode");
            orderObj.IsConsigned = validation.GetBool("isConsigned");
            orderObj.TransportType = validation.Get("transportType");
            orderObj.ExpressID = validation.GetInt("expressId");

            if (orderObj.IsConsigned)
            {

                ///////////////////////以下参数是需要设置的相关配置参数，设置后不会更改的////////////////////////////
                AlipayConfig con = new AlipayConfig();
                string partner = con.Partner;
                string key = con.Key;
                string input_charset = con.Input_charset;
                string sign_type = con.Sign_type;

                ////////////////////////////////////////////////////////////////////////////////////////////////////

                ///////////////////////请求参数/////////////////////////////////////////////////////////////////////
                //--------------必填参数--------------
                //支付宝交易号。它是登陆支付宝网站在交易管理中查询得到，一般以8位日期开头的纯数字（如：20100419XXXXXXXXXX） 
                string trade_no = orderObj.TradeNo;

                //物流公司名称
                string logistics_name = orderObj.Logistics;

                //物流发货单号
                string invoice_no = orderObj.LogisticsCode;

                //物流发货时的运输类型，三个值可选：POST（平邮）、EXPRESS（快递）、EMS（EMS）
                string transport_type = orderObj.TransportType;

                //--------------选填参数--------------
                //卖家本地电脑IP地址
                string seller_ip = "";

                /////////////////////////////////////////////////////////////////////////////////////////////////////

                //构造请求函数
                AlipayServiceFH aliService = new AlipayServiceFH(
                    partner,
                    trade_no,
                    logistics_name,
                    invoice_no,
                    transport_type,
                    seller_ip,
                    key,
                    input_charset,
                    sign_type);

                /***********************无XML远程解析***********************/
                //string sHtmlText = aliService.Build_Form();
                //Response.Write(sHtmlText);
                //Response.End();


                /***********************含XML远程解析***********************/
                //注意：远程解析XML出错，与IIS服务器配置有关
                string url = aliService.Create_url();

                XmlTextReader Reader = new XmlTextReader(url);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Reader);

                //解析XML，获取XML返回的数据，如：请求处理是否成功、商家网站唯一订单号、支付宝交易号、发货时间等
                string nodeIs_success = xmlDoc.SelectSingleNode("/alipay/is_success").InnerText;
                string nodeOut_trade_no = "";
                string nodeTrade_no = "";
                string nodeTrade_status = "";
                string nodeLast_modified_time = "";
                string nodeError = "";

                if (nodeIs_success == "T")
                {
                    nodeOut_trade_no = xmlDoc.SelectSingleNode("/alipay/response/tradeBase/out_trade_no").InnerText;
                    nodeTrade_no = xmlDoc.SelectSingleNode("/alipay/request").ChildNodes[2].InnerText;
                    nodeTrade_status = xmlDoc.SelectSingleNode("/alipay/response/tradeBase/trade_status").InnerText;
                    nodeLast_modified_time = xmlDoc.SelectSingleNode("/alipay/response/tradeBase/last_modified_time").InnerText;
                }
                else
                {
                    nodeError = xmlDoc.SelectSingleNode("/alipay/error").InnerText;
                }

                if (nodeIs_success == "T")
                {
                    orderObj.Status = 3;
                    orderBLL.ModifyLogistic(orderObj);
                    return Json(new { success = true });
                }
                else
                    return Json(new { success = false, msg = nodeError });

            }
            else
            {
                orderBLL.ModifyLogistic(orderObj);
                return Json(new { success = true });
            }
        }
        #endregion

        #region ModifyServiceMemo
        public ActionResult ModifyServiceMemo()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1103))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            int orderId = validation.GetInt("orderId");

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(orderId);
            if (orderObj == null)
                return Json(new { success = false, msg = "该订单不存在" });

            orderObj.ServiceMemo = validation.Get("serviceMemo");

            orderBLL.ModifyServiceMemo(orderObj);
            return Json(new { success = true });
        }
        #endregion

        #region CancelOrder
        public ActionResult CancelOrder()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1104))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            int orderId = validation.GetInt("orderId");

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(orderId);
            if (orderObj == null)
                return Json(new { success = false, msg = "该订单不存在" });
            if (orderObj.Status == 2)
                return Json(new { success = false, msg = "该订单已被取消了" });

            orderBLL.CancelOrder(orderObj);

            return Json(new { success = true });

        }
        #endregion

        #region 获取订单明细
        public ActionResult GetOrderDetails(int orderId, int userId)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 11))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            OrderBLL orderBLL = new OrderBLL();
            var result = orderBLL.GetOrderDetails(orderId, userId);

            return Json(new { success = true, data = result });
        }
        #endregion

        #region ModifyOrder
        public ActionResult ModifyOrder()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1101))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            int orderId = validation.GetInt("orderId");

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(orderId);
            if (orderObj == null)
                return Json(new { success = false, msg = "该订单不存在" });
            if (orderObj.Status == 2)
                return Json(new { success = false, msg = "已取消的订单不可修改" });

            if (orderObj.PayStatus == 0)
            {
                orderObj.Address = validation.Get("address", false, "详细地址不可为空");
                orderObj.Receiver = validation.Get("receiver", false, "收货人不可为空");
                orderObj.Zip = validation.Get("zip");
                orderObj.CityID = validation.GetInt("cityId", false, "收货人不可为空");
                orderObj.RegionID = validation.GetInt("regionId");
                orderObj.Phone = validation.Get("phone");
                orderObj.Mobile = validation.Get("mobile");
                orderObj.Freight = validation.GetDecimal("freight");
                string data = validation.Get("data", false, "", @"^\d+\|\d+(\.\d+){0,1}(,\d+\|\d+(\.\d+){0,1})*$");

                if (validation.HasError)
                    return Json(new { success = false, msg = "请查看您的填写是否完整，或输入的信息是否有误！", errors = validation.GetErrors() });

                IDictionary<int, decimal> perf = new Dictionary<int, decimal>();

                string[] arrData = data.Split(',');
                string[] nameValue;
                for (int i = 0; i < arrData.Length; i++)
                {
                    nameValue = arrData[i].Split('|');
                    perf.Add(int.Parse(nameValue[0]), decimal.Parse(nameValue[1]));
                }

                orderBLL.ModifyOrder(orderObj, perf);
            }

            return Json(new { success = true });
        }
        #endregion

        #region DeleteDetail
        public ActionResult DeleteDetail()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1101))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            int orderId = validation.GetInt("orderId");
            int detailId = validation.GetInt("detailId");

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(orderId);

            if (orderObj == null)
                return Json(new { success = false, msg = "该订单不存在" });
            if (orderObj.Status == 2)
                return Json(new { success = false, msg = "已取消的订单不可修改" });

            orderBLL.DeleteDetail(orderId, detailId);

            return Json(new { success = true });
        }
        #endregion

        #region OrderServiceMemo
        public ActionResult OrderServiceMemo()
        {
            if (Request.HttpMethod != "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1103))
                    return Redirect("/Manage/Error/2.html");

                Validation vld = new Validation(false);
                int id = vld.GetInt("id");

                OrderBLL orderBLL = new OrderBLL();
                OrderObj orderObj = orderBLL.GetOrderByID(id);

                ViewBag.id = id;
                ViewBag.serviceMemo = orderObj.ServiceMemo;

                return View();
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1103))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                OrderBLL orderBLL = new OrderBLL();

                Validation vld = new Validation();
                OrderObj orderObj = new OrderObj();
                orderObj.OrderID = vld.GetInt("id");
                orderObj.ServiceMemo = vld.Get("serviceMemo");

                orderBLL.ModifyServiceMemo(orderObj);

                return Json(new { success = true });
            }
        }
        #endregion

        public ActionResult Survey(int id)
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1105))
                return Redirect("/Manage/Error/2.html");

            return View();
        }

        #region GetSurveyByID
        public ActionResult GetSurveyByID(int orderID)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1105))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(orderID);
            if (orderObj == null)
                return Json(new { success = false, msg = "该订单不存在！" });

            var res = orderBLL.GetSurveyByID(orderID);
            return Json(new { success = true, data = res });
        }
        #endregion

        public ActionResult OrderSetExpress(int id)
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1102))
                return Redirect("/Manage/Error/2.html");

            OrderBLL orderBLL = new OrderBLL();
            ViewBag.order = orderBLL.GetOrderByID(id);
            ViewBag.express = new ExpressBLL().GetExpress();

            return View();
        }

        #region ReturnExchange
        public ActionResult ReturnExchange()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 20))
                return Redirect("/Manage/Error/2.html");

            return View();
        }
        #endregion

        public ActionResult GetReturnExchangeList()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 13))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            int page = validation.GetInt("page", defaultValue: 1);
            int pageSize = validation.GetInt("pageSize", defaultValue: 10);
            string orderCode = validation.Get("orderCode");
            string userName = validation.Get("userName");
            DateTime dtFrom = validation.GetDateTime("dtFrom");
            DateTime dtTo = validation.GetDateTime("dtTo");

            DateTime? addTimeFrom = dtFrom == DateTime.MinValue ? null : (DateTime?)dtFrom;
            DateTime? addTimeTo = dtFrom == DateTime.MinValue ? null : (DateTime?)dtTo;

            ExchangeBLL exchangeBLL = new ExchangeBLL();

            int total;
            var data = exchangeBLL.GetExchange(page, pageSize, out total, orderCode: orderCode, userName: userName, addTimeFrom: addTimeFrom, addTimeTo: addTimeTo);

            return Json(new
            {
                success = true,
                data = data,
                total = total
            });
        }

        public ActionResult ViewReturnExchange(int id)
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 20))
                return Redirect("/Manage/Error/2.html");

            ExchangeBLL exchangeBLL = new ExchangeBLL();
            var exchangeInfo = exchangeBLL.GetExchangeByID(id);

            UserBLL userBLL = new UserBLL();
            OrderBLL orderBLL = new OrderBLL();
            ProductBLL productBLL = new ProductBLL();
            AreaBLL areaBLL = new AreaBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(exchangeInfo.OrderID);

            ViewBag.returnProduct = productBLL.GetProduct(exchangeInfo.ReturnProductID);


            if (exchangeInfo.ExchangeProductID != 0)
            {
                ViewBag.exchangeProduct = productBLL.GetProduct(exchangeInfo.ExchangeProductID);
            }

            ViewBag.exchangeInfo = exchangeInfo;
            ViewBag.order = orderObj;
            ViewBag.userInfo = userBLL.GetUserByID(orderObj.UserID);
            ViewBag.userInfo = userBLL.GetUserByID(orderObj.UserID);
            ViewBag.area = areaBLL.GetAreaByRetionID(exchangeInfo.RegionID);

            return View();
        }

        public ActionResult DeleteReturnExchange(int id)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 2001))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            ExchangeBLL exchangeBLL = new ExchangeBLL();
            exchangeBLL.DeleteExchange(id);
            exchangeBLL.SubmitChanges();

            return Json(new { success = true });
        }
    }
}

