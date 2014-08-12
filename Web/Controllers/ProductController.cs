using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Gllo.BLL;
using Gllo.Models;
using System.Text;
using Gllo.Sys;

namespace Gllo.Web.Controllers
{
    public class ProductController : Controller
    {
        #region 首页
        public ActionResult Index()
        {
            int total;

            MediaBLL mediaBLL = new MediaBLL();
            IDictionary<int, MediaObj> flash = new Dictionary<int, MediaObj>();
            flash.Add(15, mediaBLL.GetMedia(15));
            flash.Add(16, mediaBLL.GetMedia(16));
            flash.Add(17, mediaBLL.GetMedia(17));
            flash.Add(18, mediaBLL.GetMedia(18));
            flash.Add(19, mediaBLL.GetMedia(19));
            flash.Add(20, mediaBLL.GetMedia(20));
            flash.Add(21, mediaBLL.GetMedia(21));
            flash.Add(22, mediaBLL.GetMedia(22));
            flash.Add(23, mediaBLL.GetMedia(23));
            flash.Add(24, mediaBLL.GetMedia(24));
            flash.Add(25, mediaBLL.GetMedia(25));
            flash.Add(26, mediaBLL.GetMedia(26));
            flash.Add(27, mediaBLL.GetMedia(27));
            flash.Add(28, mediaBLL.GetMedia(28));
            flash.Add(29, mediaBLL.GetMedia(29));
            flash.Add(30, mediaBLL.GetMedia(30));
            flash.Add(31, mediaBLL.GetMedia(31));
            ViewBag.flash = flash;

            ProductBLL productBLL = new ProductBLL();

            JsonArray cates = productBLL.GetProductCatesByParentID(0);
            ViewBag.cates = cates;
            ViewBag.cate1 = WriteProducts(productBLL.GetProducts((int)cates[0]["categoryID"], null, 0, 0, -1, -1, -1, -1, 1, 3, out total));
            ViewBag.cate2 = WriteProducts(productBLL.GetProducts((int)cates[1]["categoryID"], null, 0, 0, -1, -1, -1, -1, 1, 2, out total));
            ViewBag.cate3 = WriteProducts(productBLL.GetProducts((int)cates[2]["categoryID"], null, 0, 0, -1, -1, -1, -1, 1, 6, out total));
            ViewBag.cate4 = WriteProducts(productBLL.GetProducts((int)cates[3]["categoryID"], null, 0, 0, -1, -1, -1, -1, 1, 6, out total));
            ViewBag.cate5 = WriteProducts(productBLL.GetProducts((int)cates[4]["categoryID"], null, 0, 0, -1, -1, -1, -1, 1, 6, out total));
            ViewBag.cate6 = WriteProducts(productBLL.GetProducts((int)cates[5]["categoryID"], null, 0, 0, -1, -1, -1, -1, 1, 6, out total));
            ViewBag.cate7 = WriteProducts(productBLL.GetProducts((int)cates[6]["categoryID"], null, 0, 0, -1, -1, -1, -1, 1, 6, out total));

            ViewBag.onSaleProducts = WriteProducts(productBLL.GetProducts(0, null, 0, 0, -1, 1, -1, -1, 1, 4, out total));
            ViewBag.newProducts = WriteProducts(productBLL.GetProducts(0, null, 0, 0, 1, -1, -1, -1, 1, 3, out total));
            return View();
        }

        private MvcHtmlString WriteProducts(Gllo.Models.JsonArray products, int len = 0)
        {
            if (products != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                decimal specialPrice, price;
                if (len != 0)
                {
                    sb.Append("<div>");
                }
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

                    if (len != 0 && (i + 1) % len == 0 && i != products.Count - 1)
                    {
                        sb.Append("</div><div>");
                    }
                }
                if (len != 0)
                    sb.Append("</div>");

                return new MvcHtmlString(sb.ToString());
            }
            return null;
        }
        #endregion

        #region 商品
        public ActionResult Product(int id)
        {
            ProductBLL productBLL = new ProductBLL();
            ProductObj productObj = productBLL.GetProduct(id);
            Dictionary<int, string> cates = new Dictionary<int, string>();
            ProductCateObj productCateObj = productBLL.GetProductCateByCateID(productObj.CategoryID);
            IList<ProductCateObj> productCateList = new List<ProductCateObj>();

            ViewBag.allCates = productBLL.GetProductCates();

            int total;
            productBLL.GetComments(id, 1, 1, out total);

            ViewBag.commentCount = total;
            productBLL.GetMessages(id, 1, 1, out total);
            ViewBag.messageCount = total;

            while (productCateObj != null)
            {
                productCateList.Add(productCateObj);
                productCateObj = productBLL.GetProductCateByCateID(productCateObj.ParentID);
            }
            StringBuilder sb = new StringBuilder();
            for (int i = productCateList.Count - 1; i >= 0; i--)
            {
                sb.Append("&gt;")
                    .Append("<a href='/list/")
                    .Append(productCateList[i].CategoryID)
                    .Append(".html'>")
                    .Append(productCateList[i].CategoryName)
                    .Append("</a>");
            }
            ViewBag.current = new MvcHtmlString(sb.ToString());
            ViewBag.product = productObj;

            //int totalRecommends;
            //ViewBag.recommends = productBLL.GetProducts(0, null, 0, 0, -1, -1, 1, -1, 1, 10, out  totalRecommends);

            ExpressBLL expressBLL = new ExpressBLL();
            ViewBag.express = expressBLL.GetExpress();

            HttpCookie cookie = Request.Cookies["phistory"];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                cookie = new HttpCookie("phistory", id.ToString());
                cookie.Expires = DateTime.Now.AddDays(30);
                ViewBag.recommends = productBLL.GetProducts(new List<int> { id });
            }
            else
            {
                string[] values = cookie.Value.Split(',');
                bool flag = false;
                IList<int> hst = new List<int>();
                for (int i = 0; i < values.Length; i++)
                {
                    hst.Add(int.Parse(values[i]));
                    if (int.Parse(values[i]) == id)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    hst.Insert(0, id);
                    cookie.Value = "";
                    for (int i = 0; i < hst.Count && i < 10; i++)
                    {
                        if (i != 0)
                        {
                            cookie.Value += ",";
                        }
                        cookie.Value += hst[i].ToString();
                    }
                }
                ViewBag.recommends = productBLL.GetProducts(hst);

                cookie.Expires = DateTime.Now.AddDays(30);
            }
            Response.SetCookie(cookie);

            return View();
        }
        #endregion

        #region 商品类别
        public ActionResult Cates(string id)
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

            MediaBLL mediaBLL = new MediaBLL();
            IDictionary<int, MediaObj> flash = new Dictionary<int, MediaObj>();
            flash.Add(32, mediaBLL.GetMedia(32));
            flash.Add(33, mediaBLL.GetMedia(33));
            flash.Add(34, mediaBLL.GetMedia(34));
            flash.Add(35, mediaBLL.GetMedia(35));
            ViewBag.flash = flash;

            ProductBLL productBLL = new ProductBLL();
            IList<ProductCateObj> productCateList = new List<ProductCateObj>();
            ProductCateObj productCateObj = productBLL.GetProductCateByCateID(cateID);
            ViewBag.cates = productCateObj;
            while (productCateObj != null)
            {
                productCateObj = productBLL.GetProductCateByCateID(productCateObj.ParentID);
                if (productCateObj == null)
                    break;
                else
                    productCateList.Add(productCateObj);
            }
            StringBuilder sb = new StringBuilder();
            for (int i = productCateList.Count - 1; i >= 0; i--)
            {
                sb.Append("&gt;")
                    .Append("<a href='/list/")
                    .Append(productCateList[i].CategoryID)
                    .Append(".html'>")
                    .Append(productCateList[i].CategoryName)
                    .Append("</a>");
            }

            ViewBag.current = new MvcHtmlString(sb.ToString());
            ViewBag.allCates = productBLL.GetProductCates();

            Validation vld = new Validation(false);
            int sort = vld.GetInt("sort");
            string price = vld.Get("price");
            decimal priceFrom = 0;
            decimal priceTo = 0;
            if (!string.IsNullOrEmpty(price))
            {
                string[] p = price.Split('-');
                priceFrom = decimal.Parse(p[0]);
                priceTo = decimal.Parse(p[1]);
            }

            string sortField;
            bool isAsc;
            switch (sort)
            {
                case 1:
                    sortField = "Price";
                    isAsc = true;
                    break;
                case 2:
                    sortField = "ProductID";
                    isAsc = false;
                    break;
                case 4:
                    sortField = "SellNum";
                    isAsc = false;
                    break;
                case 5:
                    sortField = "Price";
                    isAsc = false;
                    break;
                default:
                    sortField = "Products.Sort";
                    isAsc = false;
                    break;
            }

            ViewBag.sort = sort;
            ViewBag.price = price;

            int pageSize = 24;
            int total;
            ViewBag.data = productBLL.GetProducts(cateID, null, priceFrom, priceTo, -1, -1, -1, -1, startPage, pageSize, out total, 1, sortField, isAsc);
            ViewBag.pageSize = pageSize;
            ViewBag.total = total;
            ViewBag.totalPages = total % pageSize == 0 ? total / pageSize : (total / pageSize + 1);

            int totalRecommends;
            ViewBag.recommends = productBLL.GetProducts(0, null, 0, 0, -1, -1, 1, -1, 1, 10, out  totalRecommends);

            ViewBag.onSaleProducts = WriteProducts(productBLL.GetProducts(0, null, 0, 0, -1, 1, -1, -1, 1, 5, out total));
            ViewBag.newProducts = WriteProducts(productBLL.GetProducts(0, null, 0, 0, 1, -1, -1, -1, 1, 20, out total), 5);

            return View();
        }
        #endregion

        #region 搜索商品
        public ActionResult Search()
        {
            Validation validation = new Validation(false);
            int page = validation.GetInt("page", defaultValue: 1);
            int categoryID = validation.GetInt("cid");
            int sort = validation.GetInt("sort");
            int pageSize = validation.GetInt("pageSize", defaultValue: 16);
            string keywords = validation.Get("keywords");
            ViewBag.pageSize = pageSize;
            ViewBag.page = page;
            ViewBag.sort = sort;

            string sortField;
            bool isAsc;
            switch (sort)
            {
                case 1:
                    sortField = "Price";
                    isAsc = true;
                    break;
                case 2:
                    sortField = "ProductID";
                    isAsc = false;
                    break;
                case 4:
                    sortField = "SellNum";
                    isAsc = false;
                    break;
                case 5:
                    sortField = "Price";
                    isAsc = false;
                    break;
                default:
                    sortField = "Products.Sort";
                    isAsc = false;
                    break;
            }

            ProductBLL productBLL = new ProductBLL();
            if (string.IsNullOrEmpty(keywords) || keywords == "请输入关键字……")
            {
                ViewBag.keywords = "";
                ViewBag.data = null;
                ViewBag.total = 0;
            }
            else
            {
                ViewBag.keywords = keywords;
                int total;
                ViewBag.categoryID = categoryID;
                if (categoryID == 0)
                {
                    ViewBag.tags = productBLL.GetTags(keywords);

                }
                ViewBag.data = productBLL.GetProducts(categoryID, keywords, 0, 0, -1, -1, -1, -1, page, pageSize, out total, 1, sortField, isAsc);
                ViewBag.total = total;
            }

            if (ViewBag.data == null)
            {
                ViewBag.cates = productBLL.GetProductCates();
            }
            else
            {
                ViewBag.cates = productBLL.GetProductCates(keywords);
            }

            int totalRecommends;
            ViewBag.recommends = productBLL.GetProducts(0, null, 0, 0, -1, -1, 1, -1, 1, 10, out  totalRecommends);
            return View();
        }
        #endregion

        #region 促销专区
        public ActionResult OnSale()
        {
            int startPage = Request.QueryString["page"] == null ? 1 : int.Parse(Request.QueryString["page"]);
            ViewBag.page = startPage;

            ProductBLL productBLL = new ProductBLL();

            ViewBag.allCates = productBLL.GetProductCates();

            int pageSize = 10;
            int total;
            ViewBag.data = productBLL.GetProducts(0, null, 0, 0, -1, 1, -1, -1, startPage, pageSize, out total);
            ViewBag.pageSize = pageSize;
            ViewBag.total = total;
            ViewBag.totalPages = total % pageSize == 0 ? total / pageSize : (total / pageSize + 1);

            int totalRecommends;
            ViewBag.recommends = productBLL.GetProducts(0, null, 0, 0, -1, -1, 1, -1, 1, 10, out  totalRecommends);
            return View();
        }
        #endregion

        #region 添加评论
        public ActionResult AddComment()
        {
            UserObj user = AppData.SessionUser;
            if (user == null)
            {
                return Json(new { success = false, msg = "IS_NOT_LOGIN" });
            }

            Validation vld = new Validation();
            ProductBLL productBLL = new ProductBLL();
            int productId = vld.GetInt("productId");

            if (!productBLL.IsBuy(user.UserID, productId))
            {
                return Json(new { success = false, msg = "IS_NOT_BUY" });
            }

            if (productBLL.IsComment(user.UserID, productId))
            {
                return Json(new { success = false, msg = "IS_COMMENT" });
            }

            string sessionCheckCode = Session["CheckCode"] == null ? null : Session["CheckCode"].ToString();
            Session["CheckCode"] = null;

            if (sessionCheckCode == null)
                return Json(new { success = false, msg = "验证码已经过期，请刷新验证码！" });


            string checkCode = vld.Get("checkCode");
            if (checkCode.ToLower() != sessionCheckCode.ToLower())
                return Json(new { success = false, msg = "验证码错误！" });

            string content = vld.Get("content");
            int score = vld.GetInt("score");

            productBLL.AddComment(user.UserID, productId, content, score);
            return Json(new { success = true });
        }
        #endregion

        #region 获取评论
        public ActionResult GetComments(int id, int page, int pageSize)
        {
            ProductBLL productBLL = new ProductBLL();
            int total;
            var res = productBLL.GetComments(id, page, pageSize, out total);
            return Json(new { success = true, data = res, total = total });
        }
        #endregion

        #region AddMessage
        public ActionResult AddMessage()
        {
            string sessionCheckCode = Session["CheckCode"] == null ? null : Session["CheckCode"].ToString();
            Session["CheckCode"] = null;

            if (sessionCheckCode == null)
                return Json(new { success = false, msg = "验证码已经过期，请刷新验证码！" });


            Validation vld = new Validation();
            string checkCode = vld.Get("checkCode");
            if (checkCode.ToLower() != sessionCheckCode.ToLower())
                return Json(new { success = false, msg = "验证码错误！" });

            int productID = vld.GetInt("productID");
            string content = vld.Get("content");
            bool isAnonymity = vld.GetBool("isAnonymity");

            UserObj user = AppData.SessionUser;
            ProductBLL productBLL = new ProductBLL();
            productBLL.AddMessage(user == null ? Request.UserHostAddress : user.UserName, productID, content, isAnonymity);
            return Json(new { success = true });
        }
        #endregion

        #region GetMessages
        public ActionResult GetMessages(int id, int page, int pageSize)
        {
            ProductBLL productBLL = new ProductBLL();
            int total;
            var res = productBLL.GetMessages(id, page, pageSize, out total);
            return Json(new { success = true, data = res, total = total });
        }
        #endregion

        public ActionResult GetProductFreight(int productID, int regionID)
        {
            ProductBLL productBLL = new ProductBLL();
            var freights = productBLL.GetProductFreight(productID, regionID);

            return Json(new { success = true, freight = freights[0], freight1 = freights[1] });
        }

    }
}
