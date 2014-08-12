using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Linq;

using Gllo.BLL;
using Gllo.Models;
using Gllo.Sys;
using AlipayClass;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Gllo.Web.Controllers
{
    public class OrderController : Controller
    {
        #region [页面] 购物车
        public ActionResult Cart()
        {
            int total;
            ProductBLL productBLL = new ProductBLL();
            ViewBag.onSaleProducts = WriteProducts(productBLL.GetProducts(0, null, 0, 0, 1, -1, -1, -1, 1, 6, out total));
            ViewBag.logisticText = new SettingBLL().Get("LogisticTips");
            return View();
        }

        private MvcHtmlString WriteProducts(Gllo.Models.JsonArray products)
        {
            if (products != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                decimal specialPrice, price;
                for (var i = 0; i < products.Count; i++)
                {
                    int id = (int)products[i]["ProductID"];
                    specialPrice = (decimal)products[i]["SpecialPrice"];
                    price = (decimal)products[i]["Price"];

                    sb.Append("<dl class=\"pi\"><dt><a href=\"/product/")
                        .Append(id)
                        .Append(".html\"><img src=\"")
                        .Append(Gllo.Sys.Config.MediaServer)
                        .Append(products[i]["PictureUrl"])
                        .Append("\" alt=\"")
                        .Append(products[i]["PictureDesc"])
                        .Append("\" /></a></dt><dd><a href=\"/product/")
                        .Append(id)
                        .Append(".html\">")
                        .Append(products[i]["Name"])
                        .Append("</a></dd>");
                    if (specialPrice != 0 && specialPrice < price)
                    {
                        sb.Append("<dd>原价：<del>￥")
                            .Append(price.ToString("0"))
                            .Append("</del></dd><dd>现价：<span>￥")
                            .Append(specialPrice.ToString("0"))
                            .Append("</span></dd>");
                    }
                    else
                        sb.Append("<dd>价格：<span>￥")
                            .Append(price.ToString("0"))
                            .Append("</span></dd>");

                    sb.AppendLine("</dl>");
                }
                return new MvcHtmlString(sb.ToString());
            }
            return null;
        }

        #endregion

        #region 判断是否进入订单页面
        public ActionResult CanRedirectToOrderPage()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Json(new { success = false, msg = "IS_NOT_LOGIN" });

            IList<int[]> cart = AppData.CookieCart;
            if (cart == null || cart.Count == 0)
                return Json(new { success = false, msg = "您的购物车中没有商品，请先选购你想要购买的商品！" });

            IList<int> productIds = new List<int>();
            IDictionary<int, int> idQty = new Dictionary<int, int>();
            int prdId;
            foreach (int[] cartInfo in cart)
            {
                if (cartInfo.Length > 4)
                {
                    for (int i = 4; i < cartInfo.Length; i++)
                    {
                        prdId = cartInfo[i];
                        if (!productIds.Contains(prdId))
                        {
                            productIds.Add(prdId);
                            idQty.Add(prdId, cartInfo[1]);
                        }
                        else
                            idQty[prdId] += cartInfo[1];
                    }
                }
                else
                {
                    prdId = cartInfo[0];
                    if (!productIds.Contains(prdId))
                    {
                        productIds.Add(prdId);
                        idQty.Add(prdId, cartInfo[1]);
                    }
                    else
                        idQty[prdId] += cartInfo[1];
                }
            }

            ProductBLL productBLL = new ProductBLL();
            JsonArray products = productBLL.GetProducts(productIds);
            if (products == null)
                return Json(new { success = false, msg = "您想要购买商品不存在或已下线！" });

            JsonObject product;
            int productId;
            int productQty;
            for (int i = 0; i < products.Count; i++)
            {
                product = products[i];
                productId = (int)product["ProductID"];
                productQty = (int)product["Quantity"];
                if (idQty[productId] > productQty)
                    return Json(new { success = false, msg = string.Format("商品【{0}】的库存不足，型号:{1}！", product["Name"], product["Code"]) });
            }
            return Json(new { success = true });
        }
        #endregion

        public ActionResult GetCartFreight()
        {
            IList<int[]> cart = AppData.GetBuy(Request["data"]);
            int regionID = int.Parse(Request["regionID"]);
            var result = new List<object>();
            decimal totalFreight = 0;

            if (cart != null && cart.Count != 0)
            {
                IList<int> productIds = new List<int>();
                int prdId, packageID;
                IDictionary<int, int> idQty = new Dictionary<int, int>();
                ProductBLL productBLL = new ProductBLL();
                PackageBLL packageBLL = new PackageBLL();
                JsonObject product, package;
                foreach (int[] cartInfo in cart)
                {
                    if (cartInfo.Length > 4)
                    {
                        packageID = cartInfo[0];
                        package = packageBLL.GetJsonPackageByIDForCart(packageID);
                        if (package != null)
                        {
                            result.Add("-");
                            for (int i = 4; i < cartInfo.Length; i++)
                            {
                                prdId = cartInfo[i];
                                product = productBLL.GetJsonProductByID(prdId);
                                if (product != null)
                                {
                                    var freights = productBLL.GetProductFreight(prdId, regionID);
                                    decimal freight = 0;
                                    if (idQty.ContainsKey(prdId))
                                    {
                                        freight = freights[1] * cartInfo[1];
                                        idQty[prdId] += cartInfo[1];
                                    }
                                    else
                                    {
                                        freight = freights[0] * 1;
                                        freight += freights[1] * (cartInfo[1] - 1);
                                        idQty.Add(prdId, cartInfo[1]);
                                    }
                                    totalFreight += freight;
                                    result.Add(freight);

                                }
                            }
                        }
                    }
                    else
                    {
                        prdId = cartInfo[0];
                        product = productBLL.GetJsonProductByID(prdId);
                        if (product != null)
                        {
                            var freights = productBLL.GetProductFreight(prdId, regionID);
                            decimal freight = 0;
                            if (idQty.ContainsKey(prdId))
                            {
                                freight = freights[1] * cartInfo[1];
                                idQty[prdId] += cartInfo[1];
                            }
                            else
                            {
                                freight = freights[0] * 1;
                                freight += freights[1] * (cartInfo[1] - 1);
                                idQty.Add(prdId, cartInfo[1]);
                            }
                            totalFreight += freight;
                            result.Add(freight);
                        }
                    }
                }
            }

            return Json(new { success = true, totalFreight = totalFreight, data = result });
        }

        #region 获取购物车中的商品
        public ActionResult GetCartProductList()
        {
            IList<int[]> cart;
            if (!string.IsNullOrEmpty(Request["data"]))
            {
                cart = AppData.GetBuy(Request["data"]);
            }
            else
            {
                cart = AppData.CookieCart;
            }
            JsonArray result;
            if (cart != null && cart.Count != 0)
            {
                IList<int> productIds = new List<int>();
                IDictionary<int, int> idQty = new Dictionary<int, int>();
                int prdId, packageID;
                ProductBLL productBLL = new ProductBLL();
                PackageBLL packageBLL = new PackageBLL();
                JsonObject product, package;
                StringBuilder cookie;
                result = new JsonArray();
                foreach (int[] cartInfo in cart)
                {
                    if (cartInfo.Length > 4)
                    {
                        packageID = cartInfo[0];
                        package = packageBLL.GetJsonPackageByIDForCart(packageID);
                        if (package != null)
                        {
                            package.Add("CartType", 1);
                            package.Add("BuyQty", cartInfo[1]);
                            package.Add("CartID", cartInfo[2]);
                            cookie = new StringBuilder(); ;
                            for (int i = 0; i < cartInfo.Length; i++)
                            {
                                if (i != 0)
                                    cookie.Append("|");
                                cookie.Append(cartInfo[i]);
                            }
                            package.Add("Cookie", cookie.ToString());
                            result.Add(package);

                            for (int i = 4; i < cartInfo.Length; i++)
                            {
                                prdId = cartInfo[i];
                                product = productBLL.GetJsonProductByID(prdId);
                                if (product != null)
                                {
                                    product.Add("CartType", 2);
                                    product.Add("BuyQty", cartInfo[1]);
                                    product.Add("Express", 1);
                                    product.Add("CartID", cartInfo[2]);
                                    result.Add(product);
                                }
                            }
                        }
                    }
                    else
                    {
                        prdId = cartInfo[0];
                        product = productBLL.GetJsonProductByID(prdId);
                        if (product != null)
                        {
                            product.Add("CartType", 0);
                            product.Add("BuyQty", cartInfo[1]);
                            product.Add("Express", cartInfo[2]);
                            cookie = new StringBuilder(); ;
                            for (int i = 0; i < cartInfo.Length; i++)
                            {
                                if (i != 0)
                                    cookie.Append("|");
                                cookie.Append(cartInfo[i]);
                            }
                            product.Add("Cookie", cookie.ToString());
                            result.Add(product);
                        }

                    }

                }
            }
            else
                result = null;

            return Json(new { success = true, data = result, express = new ExpressBLL().GetExpress() });
        }
        #endregion

        #region 获取购物车中商品数量
        public ActionResult GetCartNum()
        {
            IList<int[]> cart = AppData.CookieCart;
            int num = 0;
            if (cart != null && cart.Count != 0)
            {
                foreach (int[] cartInfo in cart)
                {
                    num += cartInfo[1];
                }
            }
            int pm = 0;
            UserObj user = AppData.SessionUser;
            if (user != null)
            {
                pm = new PM().CountNotRead(user.UserID);
            }
            return Json(new { success = true, num = num, pm = pm });
        }
        #endregion

        #region 将商品添加到购物车
        public ActionResult AddToCart()
        {
            Validation vld = new Validation();
            int type = vld.GetInt("type");
            int quantity = vld.GetInt("quantity");

            ProductBLL productBLL = new ProductBLL();
            if (type == 0)
            {
                int productId = vld.GetInt("productId");
                int express = vld.GetInt("express");

                ProductObj productObj = productBLL.GetProduct(productId);
                int color = string.IsNullOrEmpty(Request.Form["color"]) ? 0 : int.Parse(Request.Form["color"]);

                if (productObj == null)
                    return Json(new { success = false, msg = "该商品不存在！" });

                if (productObj.Quantity <= 0)
                    return Json(new { success = false, msg = "抱歉，该商品已经没有库存了！" });

                IList<int[]> cart = AppData.CookieCart;

                if (cart == null)
                    cart = new List<int[]>() { new int[] { productId, quantity, express, color } };
                else
                {
                    bool exists = false;
                    int totalBuyQty = quantity;
                    int oldExpress, oldProductId, oldColor;
                    for (var i = 0; i < cart.Count; i++)
                    {
                        oldProductId = cart[i][0];
                        if (oldProductId == productId)
                        {
                            oldExpress = cart[i][2];
                            oldColor = cart[i][3];
                            if (oldExpress == express && oldColor == color)
                            {
                                cart[i][1] += quantity;
                                exists = true;
                            }
                            totalBuyQty += cart[i][1];
                        }
                    }
                    if (!exists)
                        cart.Add(new int[] { productId, quantity, express, color });

                    quantity = totalBuyQty;
                }

                if (quantity > productObj.Quantity)
                    return Json(new { success = false, msg = string.Format("抱歉，您购买的商品数量已超过现有的库存数量！") });

                AppData.CookieCart = cart;
            }
            else
            {
                int packageID = vld.GetInt("packageID");
                int id = vld.GetInt("id");
                string sPrds = vld.Get("prds", false, "套餐中无商品", @"^\d+\|\d+(,\d+\|\d+)*$", "套餐商品传入参数错误");

                if (vld.HasError)
                    return Json(new { success = false, msg = vld.GetError() });

                string[] aPrds = sPrds.Split(',');
                PackageBLL packageBLL = new PackageBLL();

                IList<int[]> cart = AppData.CookieCart;

                bool contains = false;
                if (cart == null)
                    cart = new List<int[]>() { };
                else
                {
                    for (int i = 0; i < cart.Count; i++)
                    {
                        if (cart[i][0] == packageID && cart[i][2] == id)
                        {
                            cart[i][1] += quantity;
                            contains = true;
                        }
                    }
                }
                if (!contains)
                {
                    int[] res = new int[aPrds.Length + 4];
                    res[0] = packageID;
                    res[1] = quantity;
                    res[2] = cart.Count;
                    res[3] = -1;
                    for (int i = 0; i < aPrds.Length; i++)
                    {
                        string[] idId = aPrds[i].Split('|');
                        int ppcID = int.Parse(idId[0]);
                        int ppID = int.Parse(idId[1]);
                        int productID = packageBLL.GetProductIDByPpID(ppID);
                        ProductObj productObj = productBLL.GetProduct(productID);

                        if (productObj.Quantity <= 0)
                            return Json(new { success = false, msg = "抱歉，该商品已经没有库存了！" });

                        if (quantity > productObj.Quantity)
                            return Json(new { success = false, msg = string.Format("抱歉，您购买的商品“{0}”的数量已超过现有的库存数量！", productObj.Name) });

                        res[i + 4] = ppID;
                    }
                    cart.Add(res);
                }

                AppData.CookieCart = cart;
            }

            return Json(new { success = true });
        }
        #endregion

        #region 从购物车中移除商品
        public ActionResult RemoveFromCart(string ids)
        {
            string[] arrIds = ids.Split(',');
            IList<int[]> cart = AppData.CookieCart;

            if (cart != null)
            {
                int productId, express, color;
                string[] pe;
                for (int i = 0; i < arrIds.Length; i++)
                {
                    if (!string.IsNullOrEmpty(arrIds[i]))
                    {
                        pe = arrIds[i].Split('|');
                        productId = int.Parse(pe[0]);
                        express = int.Parse(pe[1]);
                        color = int.Parse(pe[2]);
                        for (int j = 0; j < cart.Count; j++)
                        {
                            if (cart[j][0] == productId && cart[j][2] == express && cart[j][3] == color)
                            {
                                cart.RemoveAt(j);
                            }
                        }

                    }
                }
                AppData.CookieCart = cart;
            }

            return Json(new { success = true });
        }

        #endregion

        #region [页面] 订单信息填写页面
        public ActionResult Order()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Redirect("/Login.html");

            ViewBag.express = new ExpressBLL().GetExpress();
            return View();
        }
        #endregion

        #region 生成订单
        public ActionResult CreateOrder()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Json(new { success = false, msg = "您还未登录！" });

            IList<int[]> cart;
            if (!string.IsNullOrEmpty(Request["buy"]))
            {
                cart = AppData.GetBuy(Request["buy"]);
            }
            else
            {
                cart = AppData.CookieCart;
            }
            if (cart == null || cart.Count == 0)
                return Json(new { success = false, msg = "您的购物车中没有商品，请先选购你想要购买的商品！" });

            IList<int> productIds = new List<int>();
            IDictionary<int, int> idQty = new Dictionary<int, int>();
            int prdId;
            foreach (int[] cartInfo in cart)
            {
                if (cartInfo.Length > 4)
                {
                    for (int i = 4; i < cartInfo.Length; i++)
                    {
                        prdId = cartInfo[i];
                        if (!productIds.Contains(prdId))
                        {
                            productIds.Add(prdId);
                            idQty.Add(prdId, cartInfo[1]);
                        }
                        else
                            idQty[prdId] += cartInfo[1];
                    }
                }
                else
                {
                    prdId = cartInfo[0];
                    if (!productIds.Contains(prdId))
                    {
                        productIds.Add(prdId);
                        idQty.Add(prdId, cartInfo[1]);
                    }
                    else
                        idQty[prdId] += cartInfo[1];
                }
            }

            ProductBLL productBLL = new ProductBLL();
            JsonArray products = productBLL.GetProducts(productIds);
            if (products == null)
                return Json(new { success = false, msg = "您想要购买商品不存在或已下线！" });

            JsonObject product;
            int productId;
            int productQty, buyQty;

            for (int i = 0; i < products.Count; i++)
            {
                product = products[i];
                productId = (int)product["ProductID"];
                productQty = (int)product["Quantity"];
                buyQty = idQty[productId];
                if (buyQty > productQty)
                    return Json(new { success = false, msg = string.Format("商品【{0}】的库存不足，货号:{1}！", product["Name"], product["Code"]) });
            }

            decimal specialPrice, price, amount = 0;
            OrderProductList orderProductList = new OrderProductList();
            OrderProductObj orderProductObj;
            int express;
            int packageID;
            JsonObject package;
            PackageBLL packageBLL = new PackageBLL();
            foreach (int[] cartInfo in cart)
            {
                productId = cartInfo[0];
                buyQty = cartInfo[1];
                express = cartInfo[2];

                if (cartInfo.Length > 4)
                {
                    packageID = cartInfo[0];
                    package = packageBLL.GetJsonPackageByIDForCart(packageID);
                    if (package != null)
                    {
                        for (int i = 4; i < cartInfo.Length; i++)
                        {
                            productId = cartInfo[i];
                            product = productBLL.GetJsonProductByID(productId);
                            if (product != null)
                            {
                                specialPrice = (decimal)product["SpecialPrice"];
                                price = (decimal)product["Price"];
                                price = specialPrice > 0 && specialPrice < price ? specialPrice : price;
                                amount += price * buyQty;
                                orderProductObj = new OrderProductObj();

                                orderProductObj.PackageID = packageID;
                                orderProductObj.ProductID = productId;
                                orderProductObj.OrignalPrice = price;
                                orderProductObj.Discount = 0;
                                orderProductObj.Quantity = buyQty;
                                orderProductObj.Status = 0;
                                orderProductObj.Express = express;
                                orderProductObj.CreationTime = DateTime.Now;
                                orderProductList.Add(orderProductObj);

                            }
                        }
                    }
                }
                else
                {
                    productId = cartInfo[0];
                    product = productBLL.GetJsonProductByID(productId);
                    if (product != null)
                    {
                        specialPrice = (decimal)product["SpecialPrice"];
                        price = (decimal)product["Price"];
                        price = specialPrice > 0 && specialPrice < price ? specialPrice : price;
                        amount += price * buyQty;
                        orderProductObj = new OrderProductObj();

                        orderProductObj.PackageID = 0;
                        orderProductObj.ProductID = productId;
                        orderProductObj.OrignalPrice = price;
                        orderProductObj.Discount = 0;
                        orderProductObj.Quantity = buyQty;
                        orderProductObj.Status = 0;
                        orderProductObj.Express = express;
                        orderProductObj.CreationTime = DateTime.Now;
                        orderProductList.Add(orderProductObj);
                    }
                }
            }

            OrderObj orderObj = new OrderObj();
            Validation validation = new Validation();

            orderObj.Address = validation.Get("address", false, "详细地址不可为空");
            orderObj.Receiver = validation.Get("receiver", false, "收货人不可为空");
            orderObj.Zip = validation.Get("zip");
            orderObj.CityID = validation.GetInt("cityId", false, "收货人不可为空");
            orderObj.RegionID = validation.GetInt("regionID");
            orderObj.Phone = validation.Get("phone");
            orderObj.Mobile = validation.Get("mobile");
            orderObj.PayStatus = 0;
            orderObj.PayType = validation.GetInt("payType");
            orderObj.Memo = validation.Get("memo");
            orderObj.ExpressID = validation.GetInt("expressID");

            if (validation.HasError)
                return Json(new { success = false, msg = "参数错误！", errors = validation.GetErrors() });

            if (string.IsNullOrEmpty(orderObj.Phone) && string.IsNullOrEmpty(orderObj.Mobile))
                return Json(new { success = false, msg = "收货人信息中的手机或固话必填一项！" });

            decimal totalFreight = 0, freight;
            idQty = new Dictionary<int, int>();
            for (int i = 0; i < orderProductList.Count; i++)
            {
                var freights = productBLL.GetProductFreight(orderProductList[i].ProductID, orderObj.RegionID);
                freight = 0;
                if (idQty.ContainsKey(orderProductList[i].ProductID))
                {
                    freight = freights[1] * orderProductList[i].Quantity;
                    idQty[orderProductList[i].ProductID] += orderProductList[i].Quantity;
                }
                else
                {
                    freight = freights[0] * 1;
                    freight += freights[1] * (orderProductList[i].Quantity - 1);
                    idQty.Add(orderProductList[i].ProductID, orderProductList[i].Quantity);
                }
                totalFreight += freight;
            }

            orderObj.IP = Request.UserHostAddress;
            orderObj.UserID = user.UserID;
            orderObj.OrderDetails = orderProductList;
            orderObj.Amount = amount;
            orderObj.OriginalAmount = amount;
            orderObj.Freight = totalFreight;
            orderObj.Status = 0;
            orderObj.CreationTime = DateTime.Now;

            OrderBLL orderBLL = new OrderBLL();
            int orderId = orderBLL.AddOrder(orderObj);

            if (orderId != 0)
            {
                var oldCart = AppData.CookieCart;
                for (int i = oldCart.Count - 1; i >= 0; i--)
                {
                    var ccart = oldCart[i];
                    for (int j = 0; j < cart.Count; j++)
                    {
                        if (cart[j].Length == ccart.Length)
                        {
                            bool flag = true;
                            for (int a = 0; a < ccart.Length; a++)
                            {
                                if (ccart[a] != cart[j][a])
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                oldCart.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
                AppData.CookieCart = oldCart;
                return Json(new { success = true, id = orderId });
            }
            return Json(new { success = false, msg = "抱歉，添加订单失败！" });

        }
        #endregion

        #region [页面] 订单信息
        public ActionResult OrderInfo(int id)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Redirect("/Login.html");

            ViewBag.success = Request.QueryString["step"] == "success";
            OrderObj orderObj = new OrderBLL().GetOrderByID(id);
            if (orderObj == null || user.UserID != orderObj.UserID)
                return Redirect("/Error/2.html");
            ViewBag.orderInfo = orderObj;
            return View();
        }
        #endregion

        #region 获取订单明细
        public ActionResult GetOrderDetails(int orderId)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Json(new { success = false, msg = "您未登录或会话已过期" });

            OrderBLL orderBLL = new OrderBLL();
            var result = orderBLL.GetOrderDetails(orderId, user.UserID);

            return Json(new { success = true, data = result });
        }
        #endregion

        #region 支付宝跳转
        public ActionResult alipayto(int orderId)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Redirect("/Login.html");

            OrderObj orderObj = new OrderBLL().GetOrderByID(orderId);
            if (orderObj == null || user.UserID != orderObj.UserID)
                return Redirect("/Error/2.html");

            ///////////////////////以下参数是需要设置的相关配置参数，设置后不会更改的///////////////////////////
            AlipayConfig con = new AlipayConfig();
            string partner = con.Partner;
            string key = con.Key;
            string seller_email = con.Seller_email;
            string input_charset = con.Input_charset;
            string notify_url = con.Notify_url;
            string return_url = con.Return_url;
            string show_url = con.Show_url;
            string sign_type = con.Sign_type;

            ////////////////////////////////////////////////////////////////////////////////////////////////////

            ///////////////////////以下参数是需要通过下单时的订单数据传入进来获得////////////////////////////////
            //必填参数
            string out_trade_no = orderObj.OrderCode;  //请与贵网站订单系统中的唯一订单号匹配
            string subject = "洁利来(Gllo)商品";                      //订单名称，显示在支付宝收银台里的“商品名称”里，显示在支付宝的交易管理的“商品名称”的列表里。
            string body = "洁利来(Gllo)商品";                          //订单描述、订单详细、订单备注，显示在支付宝收银台里的“商品描述”里
            string price = orderObj.Amount.ToString("0.00");                        //订单总金额，显示在支付宝收银台里的“商品单价”里

            string logistics_fee = orderObj.Freight.ToString("0.00");                  				//物流费用，即运费。
            string logistics_type = "EXPRESS";				                //物流类型，三个值可选：EXPRESS（快递）、POST（平邮）、EMS（EMS）
            string logistics_payment = "BUYER_PAY";            			//物流支付方式，两个值可选：SELLER_PAY（卖家承担运费）、BUYER_PAY（买家承担运费）

            string quantity = "1";              							//商品数量，建议默认为1，不改变值，把一次交易看成是一次下订单而非购买一件商品。

            //扩展参数——买家收货信息（推荐作为必填）
            //该功能作用在于买家已经在商户网站的下单流程中填过一次收货信息，而不需要买家在支付宝的付款流程中再次填写收货信息。
            //若要使用该功能，请至少保证receive_name、receive_address有值
            //收货信息格式请严格按照姓名、地址、邮编、电话、手机的格式填写
            string receive_name = orderObj.Receiver;			                    //收货人姓名，如：张三
            string receive_address = orderObj.Address;			                //收货人地址，如：XX省XXX市XXX区XXX路XXX小区XXX栋XXX单元XXX号
            string receive_zip = orderObj.Zip;                  			    //收货人邮编，如：123456
            string receive_phone = orderObj.Phone;                		    //收货人电话号码，如：0571-81234567
            string receive_mobile = orderObj.Mobile;               		    //收货人手机号码，如：13312341234

            //扩展参数——第二组物流方式
            //物流方式是三个为一组成组出现。若要使用，三个参数都需要填上数据；若不使用，三个参数都需要为空
            //有了第一组物流方式，才能有第二组物流方式，且不能与第一个物流方式中的物流类型相同，
            //即logistics_type="EXPRESS"，那么logistics_type_1就必须在剩下的两个值（POST、EMS）中选择
            string logistics_fee_1 = "";                					//物流费用，即运费。
            string logistics_type_1 = "";               					//物流类型，三个值可选：EXPRESS（快递）、POST（平邮）、EMS（EMS）
            string logistics_payment_1 = "";           					    //物流支付方式，两个值可选：SELLER_PAY（卖家承担运费）、BUYER_PAY（买家承担运费）

            //扩展参数——第三组物流方式
            //物流方式是三个为一组成组出现。若要使用，三个参数都需要填上数据；若不使用，三个参数都需要为空
            //有了第一组物流方式和第二组物流方式，才能有第三组物流方式，且不能与第一组物流方式和第二组物流方式中的物流类型相同，
            //即logistics_type="EXPRESS"、logistics_type_1="EMS"，那么logistics_type_2就只能选择"POST"
            string logistics_fee_2 = "";                					//物流费用，即运费。
            string logistics_type_2 = "";               					//物流类型，三个值可选：EXPRESS（快递）、POST（平邮）、EMS（EMS）
            string logistics_payment_2 = "";            					//物流支付方式，两个值可选：SELLER_PAY（卖家承担运费）、BUYER_PAY（买家承担运费）

            //扩展功能参数——其他
            string buyer_email = "";                    					//默认买家支付宝账号
            string discount = "";                       					//折扣，是具体的金额，而不是百分比。若要使用打折，请使用负数，并保证小数点最多两位数

            /////////////////////////////////////////////////////////////////////////////////////////////////////

            //构造请求函数，无需修改
            AlipayService aliService = new AlipayService(
                partner,
                seller_email,
                return_url,
                notify_url,
                show_url,
                out_trade_no,
                subject,
                body,
                price,
                logistics_fee,
                logistics_type,
                logistics_payment,
                quantity,
                receive_name,
                receive_address,
                receive_zip,
                receive_phone,
                receive_mobile,
                logistics_fee_1,
                logistics_type_1,
                logistics_payment_1,
                logistics_fee_2,
                logistics_type_2,
                logistics_payment_2,
                buyer_email,
                discount,
                key,
                input_charset,
                sign_type);
            string sHtmlText = aliService.Build_Form();

            ViewBag.form = new MvcHtmlString(sHtmlText);

            return View();
        }
        #endregion

        #region AlipayReturnUrl
        public ActionResult AlipayReturnUrl()
        {
            SortedDictionary<string, string> sArrary = GetRequestGet();
            ///////////////////////以下参数是需要设置的相关配置参数，设置后不会更改的//////////////////////
            AlipayConfig con = new AlipayConfig();
            string partner = con.Partner;
            string key = con.Key;
            string input_charset = con.Input_charset;
            string sign_type = con.Sign_type;
            string transport = con.Transport;
            //////////////////////////////////////////////////////////////////////////////////////////////

            if (sArrary.Count > 0)//判断是否有带返回参数
            {
                AlipayNotify aliNotify = new AlipayNotify(sArrary, Request.QueryString["notify_id"], partner, key, input_charset, sign_type, transport);
                string responseTxt = aliNotify.ResponseTxt; //获取远程服务器ATN结果，验证是否是支付宝服务器发来的请求
                string sign = Request.QueryString["sign"];  //获取支付宝反馈回来的sign结果
                string mysign = aliNotify.Mysign;           //获取通知返回后计算后（验证）的签名结果

                //写日志记录（若要调试，请取消下面两行注释）
                //string sWord = "responseTxt=" + responseTxt + "\n return_url_log:sign=" + Request.QueryString["sign"] + "&mysign=" + mysign + "\n return回来的参数：" + aliNotify.PreSignStr;
                //AlipayFunction.log_result(Server.MapPath("log/" + DateTime.Now.ToString().Replace(":", "")) + ".txt",sWord);

                //判断responsetTxt是否为ture，生成的签名结果mysign与获得的签名结果sign是否一致
                //responsetTxt的结果不是true，与服务器设置问题、合作身份者ID、notify_id一分钟失效有关
                //mysign与sign不等，与安全校验码、请求时的参数格式（如：带自定义参数等）、编码格式有关
                if (responseTxt == "true" && sign == mysign)//验证成功
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //请在这里加上商户的业务逻辑程序代码

                    //——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
                    //获取支付宝的通知返回参数，可参考技术文档中页面跳转同步通知参数列表
                    string trade_no = Request.QueryString["trade_no"];              //支付宝交易号
                    string order_no = Request.QueryString["out_trade_no"];	        //获取订单号
                    string total_fee = Request.QueryString["price"];	            //获取总金额
                    string subject = Request.QueryString["subject"];                //商品名称、订单名称
                    string body = Request.QueryString["body"];                      //商品描述、订单备注、描述
                    string buyer_email = Request.QueryString["buyer_email"];        //买家支付宝账号
                    string receive_name = Request.QueryString["receive_name"];      //收货人姓名
                    string receive_address = Request.QueryString["receive_address"];//收货人地址
                    string receive_zip = Request.QueryString["receive_zip"];        //收货人邮编
                    string receive_phone = Request.QueryString["receive_phone"];    //收货人电话
                    string receive_mobile = Request.QueryString["receive_mobile"];  //收货人手机
                    string trade_status = Request.QueryString["trade_status"];      //交易状态

                    if (Request.QueryString["trade_status"] == "WAIT_SELLER_SEND_GOODS")
                    {
                        //判断该笔订单是否在商户网站中已经做过处理（可参考“集成教程”中“3.4返回数据处理”）
                        //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                        //如果有做过处理，不执行商户的业务程序

                        OrderBLL orderBLL = new OrderBLL();
                        int orderId = orderBLL.GetOrderIDByCode(order_no);
                        decimal payedAmount = decimal.Parse(total_fee);
                        orderBLL.PayOrder(orderId, payedAmount, trade_no, buyer_email);

                        return Redirect("/Order/" + orderId + ".html");
                    }
                    else
                    {
                        return Content("trade_status=" + Request.QueryString["trade_status"]);
                    }
                    //——请根据您的业务逻辑来编写程序（以上代码仅作参考）——

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                else//验证失败
                {
                    return Content("验证失败");
                }
            }
            else
            {
                return Content("无返回参数");
            }
        }

        /// <summary>
        /// 获取支付宝GET过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public SortedDictionary<string, string> GetRequestGet()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.QueryString;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.QueryString[requestItem[i]]);
            }

            return sArray;
        }
        #endregion

        #region AlipayNotifyUrl
        public ActionResult AlipayNotifyUrl()
        {
            SortedDictionary<string, string> sArrary = GetRequestPost();
            ///////////////////////以下参数是需要设置的相关配置参数，设置后不会更改的//////////////////////
            AlipayConfig con = new AlipayConfig();
            string partner = con.Partner;
            string key = con.Key;
            string input_charset = con.Input_charset;
            string sign_type = con.Sign_type;
            string transport = con.Transport;
            //////////////////////////////////////////////////////////////////////////////////////////////

            if (sArrary.Count > 0)//判断是否有带返回参数
            {
                AlipayNotify aliNotify = new AlipayNotify(sArrary, Request.Form["notify_id"], partner, key, input_charset, sign_type, transport);
                string responseTxt = aliNotify.ResponseTxt; //获取远程服务器ATN结果，验证是否是支付宝服务器发来的请求
                string sign = Request.Form["sign"];         //获取支付宝反馈回来的sign结果
                string mysign = aliNotify.Mysign;           //获取通知返回后计算后（验证）的签名结果

                //写日志记录（若要调试，请取消下面两行注释）
                //string sWord = "responseTxt=" + responseTxt + "\n notify_url_log:sign=" + Request.Form["sign"] + "&mysign=" + mysign + "\n notify回来的参数：" + aliNotify.PreSignStr;
                //AlipayFunction.log_result(Server.MapPath("log/" + DateTime.Now.ToString().Replace(":", "")) + ".txt", sWord);

                //判断responsetTxt是否为ture，生成的签名结果mysign与获得的签名结果sign是否一致
                //responsetTxt的结果不是true，与服务器设置问题、合作身份者ID、notify_id一分钟失效有关
                //mysign与sign不等，与安全校验码、请求时的参数格式（如：带自定义参数等）、编码格式有关
                if (responseTxt == "true" && sign == mysign)//验证成功
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //请在这里加上商户的业务逻辑程序代码

                    //——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
                    //获取支付宝的通知返回参数，可参考技术文档中服务器异步通知参数列表
                    string trade_no = Request.Form["trade_no"];         //支付宝交易号
                    string order_no = Request.Form["out_trade_no"];     //获取订单号
                    string total_fee = Request.Form["price"];           //获取总金额
                    string subject = Request.Form["subject"];           //商品名称、订单名称
                    string body = Request.Form["body"];                 //商品描述、订单备注、描述
                    string buyer_email = Request.Form["buyer_email"];   //买家支付宝账号
                    string trade_status = Request.Form["trade_status"]; //交易状态

                    if (Request.Form["trade_status"] == "WAIT_BUYER_PAY")
                    {//该判断表示买家已在支付宝交易管理中产生了交易记录，但没有付款

                        //判断该笔订单是否在商户网站中已经做过处理（可参考“集成教程”中“3.4返回数据处理”）
                        //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                        //如果有做过处理，不执行商户的业务程序

                        return Content("success");  //请不要修改或删除
                    }
                    else if (Request.Form["trade_status"] == "WAIT_SELLER_SEND_GOODS")
                    {//该判断示买家已在支付宝交易管理中产生了交易记录且付款成功，但卖家没有发货

                        //判断该笔订单是否在商户网站中已经做过处理（可参考“集成教程”中“3.4返回数据处理”）
                        //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                        //如果有做过处理，不执行商户的业务程序
                        OrderBLL orderBLL = new OrderBLL();
                        int orderId = orderBLL.GetOrderIDByCode(order_no);
                        decimal payedAmount = decimal.Parse(total_fee);
                        orderBLL.PayOrder(orderId, payedAmount, trade_no, buyer_email);

                        return Content("success");  //请不要修改或删除
                    }
                    else if (Request.Form["trade_status"] == "WAIT_BUYER_CONFIRM_GOODS")
                    {//该判断表示卖家已经发了货，但买家还没有做确认收货的操作

                        //判断该笔订单是否在商户网站中已经做过处理（可参考“集成教程”中“3.4返回数据处理”）
                        //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                        //如果有做过处理，不执行商户的业务程序

                        OrderBLL orderBLL = new OrderBLL();
                        int orderId = orderBLL.GetOrderIDByCode(order_no);
                        orderBLL.WaitConfirmGoods(orderId);

                        return Content("success");  //请不要修改或删除
                    }
                    else if (Request.Form["trade_status"] == "TRADE_FINISHED")
                    {//该判断表示买家已经确认收货，这笔交易完成

                        //判断该笔订单是否在商户网站中已经做过处理（可参考“集成教程”中“3.4返回数据处理”）
                        //如果没有做过处理，根据订单号（out_trade_no）在商户网站的订单系统中查到该笔订单的详细，并执行商户的业务程序
                        //如果有做过处理，不执行商户的业务程序

                        OrderBLL orderBLL = new OrderBLL();
                        int orderId = orderBLL.GetOrderIDByCode(order_no);
                        orderBLL.ConfirmGoods(orderId);

                        return Content("success");  //请不要修改或删除
                    }
                    else
                    {
                        return Content("success");  //其他状态判断。普通即时到帐中，其他状态不用判断，直接打印success。
                    }
                    //——请根据您的业务逻辑来编写程序（以上代码仅作参考）——

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                else//验证失败
                {
                    return Content("fail");
                }
            }
            else
            {
                return Content("无通知参数");
            }
        }

        public SortedDictionary<string, string> GetRequestPost()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;

            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
            }

            return sArray;
        }
        #endregion

        #region 查询订单
        public ActionResult GetOrders()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Json(new { success = false, msg = "您还未登录" });
            }

            Validation vld = new Validation();
            string code = vld.Get("code");
            DateTime dtFrom = vld.GetDateTime("dtFrom");
            DateTime dtTo = vld.GetDateTime("dtTo");
            int payStatus = vld.GetInt("payStatus", defaultValue: -1);
            int page = vld.GetInt("page", defaultValue: 1);
            int pageSize = vld.GetInt("pageSize", defaultValue: 10);

            OrderBLL orderBLL = new OrderBLL();
            int total;
            var result = orderBLL.GetOrders(user.UserID, code, dtFrom, dtTo, payStatus, page, pageSize, out total);

            return Json(new { success = true, data = result, total = total });
        }
        #endregion

        #region [页面] Survey
        public ActionResult Survey(int id)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Redirect("/Login.html");

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(id);
            if (orderObj == null || user.UserID != orderObj.UserID)
                return Redirect("/Error/2.html");

            if (Request.HttpMethod == "POST")
            {
                SurveyObj surveyObj = orderBLL.GetSurveyByID(id);
                if (surveyObj == null)
                {
                    surveyObj = new SurveyObj();
                }
                surveyObj.OrderID = id;

                Validation vld = new Validation();
                surveyObj.CustomerName = vld.Get("name", false, "请填写姓名!");
                surveyObj.Address = vld.Get("address", false, "请填写量房地址!");
                surveyObj.Email = vld.Get("email", false, "请填写邮箱!");
                surveyObj.Mobile = vld.Get("mobile", false, "请填写手机!");
                surveyObj.RegionID = vld.GetInt("regionID", false, "请填写省市区!");
                surveyObj.Gender = vld.GetBool("gender");
                surveyObj.Type = vld.GetInt("type", false, "请选择装修类型!");
                surveyObj.Options = vld.Get("options");
                string otherOptions = vld.Get("otherOptions");
                if (!string.IsNullOrEmpty(otherOptions))
                {
                    if (!string.IsNullOrEmpty(surveyObj.Options))
                    {
                        surveyObj.Options += ", ";
                    }
                    surveyObj.Options += otherOptions;
                }
                surveyObj.Days = vld.GetInt("days", false, "请填写姓名!");
                surveyObj.SquareType = vld.GetInt("square");
                surveyObj.HopeTime = vld.Get("time");
                if (string.IsNullOrEmpty(surveyObj.HopeTime))
                {
                    surveyObj.HopeTime = vld.Get("otherTime");
                }
                surveyObj.HopeHour = vld.GetInt("hour");
                surveyObj.Memo = vld.Get("memo");
                surveyObj.SurveyLike = vld.GetInt("like");

                if (vld.HasError)
                {
                    StringBuilder sbErrors = new StringBuilder();
                    var errors = vld.GetErrors();
                    foreach (var err in errors)
                    {
                        sbErrors.Append(err.Value)
                            .Append("<br>");
                    }
                    ViewBag.errors = sbErrors.ToString();
                    return View();
                }

                var pic = Request.Files["pic"];
                if (pic != null && pic.ContentLength != 0)
                {
                    string ext = Path.GetExtension(pic.FileName);
                    if (!Regex.IsMatch(ext, @"^\.(gif|jpg|jpeg|png)$", RegexOptions.IgnoreCase))
                    {
                        ViewBag.errors = "上传的图片格式不合要求,请上传gif,png,jpg格式的图片";
                        return View();
                    }

                    if (pic.ContentLength > 3000000)
                    {
                        ViewBag.errors = "上传图片的大小不能超过3M!";
                        return View();
                    }

                    string dirPath = Config.MediaPath + @"\Survey";
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);

                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff") + ext;
                    string savePath = Path.Combine(dirPath, newFileName);

                    pic.SaveAs(savePath);
                    surveyObj.Pic = "/Survey/" + newFileName;
                }

                orderBLL.SetSurvey(surveyObj);
                ViewBag.success = true;
            }

            return View();
        }
        #endregion

        #region GetSurveyByID
        public ActionResult GetSurveyByID(int orderID)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Json(new { success = false, msg = "会话已过期，请重新登录！" });

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(orderID);
            if (orderObj == null || user.UserID != orderObj.UserID)
                return Json(new { success = false, msg = "该订单不存在！" });

            var res = orderBLL.GetSurveyByID(orderID);
            return Json(new { success = true, data = res });
        }
        #endregion

        #region ReturnExchange
        public ActionResult ReturnExchange(int id)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Redirect("/Login.html");

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(id);
            if (orderObj == null || user.UserID != orderObj.UserID)
                return Redirect("/Error/2.html");

            ViewBag.order = orderObj;
            var details = orderBLL.GetOrderDetails(id, user.UserID);
            if (details.Count == 1)
            {
                ViewBag.code = details[0]["Code"];
                ViewBag.qty = details[0]["Quantity"];
            }
            else
            {
                ViewBag.code = "";
                ViewBag.qty = "";
            }
            return View();
        }
        #endregion

        public ActionResult CheckReturn()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Json(new { success = false, msg = "会话已过期，请重新登录！" });

            Validation vld = new Validation();
            int orderID = vld.GetInt("orderID");

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(orderID);
            if (orderObj == null || user.UserID != orderObj.UserID)
                return Json(new { success = false, msg = "该订单不存在！" });

            ProductBLL productBLL = new ProductBLL();
            ExchangeBLL exchangeBLL = new ExchangeBLL();
            ExchangeInfo exchangeInfo = new ExchangeInfo();

            string returnProductCode = vld.Get("returnProductCode");
            exchangeInfo.ReturnQty = vld.GetInt("returnProductQty");
            exchangeInfo.ReturnProductID = productBLL.GetProductIDByCode(returnProductCode);
            if (exchangeInfo.ReturnProductID == 0)
            {
                return Json(new { success = false, msg = "该商品不存在！" });
            }
            var orderDetails = orderBLL.GetOrderDetails(orderID, orderObj.UserID);
            if (orderDetails == null)
            {
                return Json(new { success = false, msg = "订单中无可退货的商品！" });
            }
            var a = from od in orderDetails
                    where (string)od["Code"] == returnProductCode
                    select (int)od["Quantity"];
            if (a.Count<int>() == 0)
            {
                return Json(new { success = false, msg = "订单中没有此商品！" });
            }
            int returnQty = exchangeBLL.GetReturnQty(exchangeInfo.ReturnProductID, orderID);
            if (exchangeInfo.ReturnQty + returnQty > a.Sum())
            {
                return Json(new { success = false, msg = "退货数量超过订单中购买的商品数量！" });
            }
            var product = productBLL.GetProduct(exchangeInfo.ReturnProductID);

            return Json(new { success = true, amount = product.Price * exchangeInfo.ReturnQty, qty = a.Sum() });
        }

        public ActionResult CheckExchange()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Json(new { success = false, msg = "会话已过期，请重新登录！" });

            Validation vld = new Validation();
            int orderID = vld.GetInt("orderID");

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(orderID);
            if (orderObj == null || user.UserID != orderObj.UserID)
                return Json(new { success = false, msg = "该订单不存在！" });

            ProductBLL productBLL = new ProductBLL();
            ExchangeBLL exchangeBLL = new ExchangeBLL();
            ExchangeInfo exchangeInfo = new ExchangeInfo();

            string exchageProductCode = vld.Get("exchageProductCode");
            exchangeInfo.ExchangeProductID = productBLL.GetProductIDByCode(exchageProductCode);
            exchangeInfo.ExchangeQty = vld.GetInt("exchageProductQty");

            if (exchangeInfo.ExchangeProductID == 0)
            {
                return Json(new { success = false, msg = "换货商品不存在！" });
            }
            var product = productBLL.GetProduct(exchangeInfo.ExchangeProductID);
            //if (exchangeInfo.ExchangeQty > product.Quantity)
            //{
            //    return Json(new { success = false, msg = "换货商品库存不足！" });
            //}

            return Json(new { success = true, amount = product.Price * exchangeInfo.ExchangeQty });
        }

        #region AddReturnExchange
        public ActionResult AddReturnExchange()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Json(new { success = false, msg = "会话已过期，请重新登录！" });

            Validation vld = new Validation();
            int orderID = vld.GetInt("orderID");

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(orderID);
            if (orderObj == null || user.UserID != orderObj.UserID)
                return Json(new { success = false, msg = "该订单不存在！" });

            ProductBLL productBLL = new ProductBLL();
            ExchangeBLL exchangeBLL = new ExchangeBLL();
            ExchangeInfo exchangeInfo = new ExchangeInfo();
            exchangeInfo.OrderID = orderID;
            int type = vld.GetInt("type");

            string returnProductCode = vld.Get("returnProductCode");
            exchangeInfo.ReturnQty = vld.GetInt("returnProductQty");
            exchangeInfo.ReturnProductID = productBLL.GetProductIDByCode(returnProductCode);
            if (exchangeInfo.ReturnProductID == 0)
            {
                return Json(new { success = false, msg = "退货商品不存在！" });
            }
            var orderDetails = orderBLL.GetOrderDetails(orderID, orderObj.UserID);
            if (orderDetails == null)
            {
                return Json(new { success = false, msg = "订单中无可退货的商品！" });
            }
            var a = from od in orderDetails
                    where (string)od["Code"] == returnProductCode
                    select (int)od["Quantity"];
            if (a.Count<int>() == 0)
            {
                return Json(new { success = false, msg = "订单中没有此商品！" });
            }
            int returnQty = exchangeBLL.GetReturnQty(exchangeInfo.ReturnProductID, orderID);
            if (exchangeInfo.ReturnQty + returnQty > a.Sum())
            {
                return Json(new { success = false, msg = "退货数量超过订单中购买的商品数量！" });
            }
            var excs = exchangeBLL.GetExchange(user.UserID, orderID);
            if (excs != null)
            {
                var b = from e in excs
                        where e.Item1.ReturnProductID == exchangeInfo.ReturnProductID
                        select e.Item1.ReturnQty;
                if (exchangeInfo.ReturnQty > b.Sum())
                {
                    return Json(new { success = false, msg = "请勿重复退货！" });
                }
            }
            if (type == 0)
            {
                string exchageProductCode = vld.Get("exchageProductCode");
                exchangeInfo.ExchangeProductID = productBLL.GetProductIDByCode(exchageProductCode);
                exchangeInfo.ExchangeQty = vld.GetInt("exchageProductQty");

                if (exchangeInfo.ExchangeProductID == 0)
                {
                    return Json(new { success = false, msg = "换货商品不存在！" });
                }
                var product = productBLL.GetProduct(exchangeInfo.ReturnProductID);
                //if (exchangeInfo.ExchangeQty > product.Quantity)
                //{
                //    return Json(new { success = false, msg = "换货商品库存不足！" });
                //}
            }
            else
            {
                exchangeInfo.ExchangeProductID = 0;
                exchangeInfo.ExchangeQty = 0;
            }

            exchangeInfo.Mobile = vld.Get("mobile");
            exchangeInfo.RegionID = vld.GetInt("regionID");
            exchangeInfo.Address = vld.Get("address");
            exchangeInfo.AddTime = DateTime.Now;
            exchangeInfo.Zip = vld.Get("zip");
            exchangeInfo.Reason = vld.Get("reason");
            exchangeInfo.Phone = vld.Get("phone");

            exchangeBLL.AddExchange(exchangeInfo);

            exchangeBLL.SubmitChanges();

            return Json(new { success = true });
        }
        #endregion

        #region ModifyReturnExchange
        public ActionResult ModifyReturnExchange(int exchangeID)
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
                return Json(new { success = false, msg = "会话已过期，请重新登录！" });

            ProductBLL productBLL = new ProductBLL();
            ExchangeBLL exchangeBLL = new ExchangeBLL();
            ExchangeInfo exchangeInfo = exchangeBLL.GetExchangeByID(exchangeID);
            if (exchangeInfo == null)
            {
                return Json(new { success = false, msg = "该退货申请单不存在！" });
            }

            Validation vld = new Validation();
            int orderID = vld.GetInt("orderID");

            OrderBLL orderBLL = new OrderBLL();
            OrderObj orderObj = orderBLL.GetOrderByID(orderID);
            if (orderObj == null || user.UserID != orderObj.UserID)
                return Json(new { success = false, msg = "该订单不存在！" });

            exchangeInfo.ExchangeID = exchangeID;
            exchangeInfo.OrderID = orderID;
            int type = vld.GetInt("type");

            string returnProductCode = vld.Get("returnProductCode");
            exchangeInfo.ReturnQty = vld.GetInt("returnProductQty");
            exchangeInfo.ReturnProductID = productBLL.GetProductIDByCode(returnProductCode);
            if (exchangeInfo.ReturnProductID == 0)
            {
                return Json(new { success = false, msg = "退货商品不存在！" });
            }
            var orderDetails = orderBLL.GetOrderDetails(orderID, orderObj.UserID);
            if (orderDetails == null)
            {
                return Json(new { success = false, msg = "订单中无可退货的商品！" });
            }
            var a = from od in orderDetails
                    where (string)od["Code"] == returnProductCode
                    select (int)od["Quantity"];
            if (a.Count<int>() == 0)
            {
                return Json(new { success = false, msg = "订单中没有此商品！" });
            }
            if (exchangeInfo.ReturnQty > a.Sum())
            {
                return Json(new { success = false, msg = "退货数量超过订单中购买的商品数量！" });
            }

            if (type == 0)
            {
                string exchageProductCode = vld.Get("exchageProductCode");
                exchangeInfo.ExchangeProductID = productBLL.GetProductIDByCode(exchageProductCode);
                exchangeInfo.ExchangeQty = vld.GetInt("exchageProductQty");

                if (exchangeInfo.ExchangeProductID == 0)
                {
                    return Json(new { success = false, msg = "换货商品不存在！" });
                }
                var product = productBLL.GetProduct(exchangeInfo.ReturnProductID);
                //if (exchangeInfo.ExchangeQty > product.Quantity)
                //{
                //    return Json(new { success = false, msg = "换货商品库存不足！" });
                //}
            }
            else
            {
                exchangeInfo.ExchangeProductID = 0;
                exchangeInfo.ExchangeQty = 0;
            }

            exchangeInfo.Mobile = vld.Get("mobile");
            exchangeInfo.RegionID = vld.GetInt("regionID");
            exchangeInfo.Address = vld.Get("address");
            exchangeInfo.Zip = vld.Get("zip");
            exchangeInfo.Reason = vld.Get("reason");
            exchangeInfo.Phone = vld.Get("phone");

            exchangeBLL.UpdateExchange(exchangeInfo);

            exchangeBLL.SubmitChanges();

            return Json(new { success = true });
        }
        #endregion
    }
}
