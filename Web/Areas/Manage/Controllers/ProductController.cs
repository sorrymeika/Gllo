using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Gllo.Sys;
using Gllo.Models;
using Gllo.BLL;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace Gllo.Web.Manage.Controllers
{
    public class ProductController : Controller
    {
        #region [页面] 商品类别管理
        public ActionResult ProductCates()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 3))
                return Redirect("/Manage/Error/2.html");

            return View();
        }
        #endregion

        #region 新增商品分类
        public ActionResult AddProductCate()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 301))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            ProductCateObj productCateObj = new ProductCateObj();
            productCateObj.CategoryName = validation.Get("categoryName", false, "新闻分类名称不可为空！");
            productCateObj.ParentID = validation.GetInt("categoryID");
            productCateObj.Sort = validation.GetInt("sort");
            if (validation.HasError)
            {
                return Json(new { success = false, msg = "ValidationFailed", errors = validation.GetErrors() });
            }

            ProductBLL productBLL = new ProductBLL();
            productBLL.AddProductCate(productCateObj);
            return Json(new { success = true });
        }
        #endregion

        #region 修改商品分类
        public ActionResult ModifyProductCate()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 302))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            int categoryID = validation.GetInt("categoryID");
            ProductBLL productBLL = new ProductBLL();
            ProductCateObj productCateObj = productBLL.GetProductCateByCateID(categoryID);
            if (productCateObj != null)
            {
                productCateObj.CategoryName = validation.Get("categoryName", false, "新闻分类名称不可为空！");
                productCateObj.Sort = validation.GetInt("sort");
                if (validation.HasError)
                {
                    return Json(new { success = false, msg = "ValidationFailed", errors = validation.GetErrors() });
                }

                productBLL.ModifyProductCate(productCateObj);
                return Json(new { success = true });
            }
            else
                return Json(new { success = false, msg = "该类别不存在！" });

        }
        #endregion

        #region 删除商品分类
        public ActionResult DeleteProductCate()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 303))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            int categoryID = validation.GetInt("categoryID");
            if (validation.HasError || categoryID == 0)
                return Json(new { success = false, msg = "参数错误！" });
            ProductBLL productBLL = new ProductBLL();
            bool hasChild = productBLL.IsProductCateHasChildren(categoryID);
            if (hasChild)
                return Json(new { success = false, msg = "该类别包含子类或商品，无法删除！" });

            productBLL.DeleteProductCate(categoryID);
            return Json(new { success = true });
        }
        #endregion

        #region 获取所有商品分类
        public ActionResult GetProductCates()
        {
            ProductBLL productBLL = new ProductBLL();
            JsonArray result = productBLL.GetProductCates();

            return Json(new { success = true, data = result });
        }
        #endregion

        #region [页面] 商品管理
        public ActionResult Products()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 4))
                return Redirect("/Manage/Error/2.html");

            return View();
        }
        #endregion

        #region [页面] 商品添加
        public ActionResult ProductAdd()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 401))
                return Redirect("/Manage/Error/2.html");

            AppData.SessionPicViews = null;
            return View();
        }
        #endregion

        #region [页面] 商品修改
        public ActionResult ProductModify(int id)
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 402))
                return Redirect("/Manage/Error/2.html");

            ViewBag.productID = id;
            ViewBag.mediaServer = Config.MediaServer;
            return View();
        }
        #endregion

        #region [页面] 商品查看
        public ActionResult ViewProduct(int id)
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 4))
                return Redirect("/Manage/Error/2.html");

            ViewBag.productID = id;
            ViewBag.mediaServer = Config.MediaServer;
            return View();
        }
        #endregion

        #region 添加商品
        public ActionResult AddProduct()
        {
            ViewBag.success = false;
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 401))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();

            string sCategories = validation.Get("categoryIds", false, "请选择商品类别", @"^\d+(\,\d+)*$", "请选择商品类别");
            ProductObj productObj = new ProductObj();
            productObj.CategoryID = validation.GetInt("categoryID");
            productObj.Name = validation.Get("name", false, "请填写商品名称");
            productObj.Type = validation.Get("type");
            productObj.Serial = validation.Get("serial");
            productObj.Model = validation.Get("model");
            productObj.Code = validation.Get("code");
            productObj.Material = validation.Get("material");
            productObj.Weight = validation.Get("weight");
            productObj.Characteristic = validation.Get("characteristic");
            productObj.Designer = validation.Get("designer");
            productObj.Price = validation.GetDecimal("price");
            productObj.SpecialPrice = validation.GetDecimal("specialPrice");
            productObj.IsRecommend = validation.GetBool("isRecommend");
            productObj.IsOnSale = validation.GetBool("isOnSale");
            productObj.IsNew = validation.GetBool("isNew");
            productObj.CanPurchasedSeparately = validation.GetBool("canPurchasedSeparately");
            productObj.Description = HttpUtility.UrlDecode(validation.Get("description", false, "请填写商品描述"), Encoding.UTF8);
            productObj.Freight = validation.GetDecimal("freight");
            productObj.Freight1 = validation.GetDecimal("freight1");
            productObj.Inventory = validation.GetInt("inventory");
            productObj.Quantity = productObj.Inventory;
            productObj.Tags = validation.Get("tags");
            productObj.Points = validation.GetInt("points");
            productObj.Sort = DateTime.Now;

            productObj.FreightModels = new List<int>();
            string freightModels = validation.Get("freightModels");
            var fms = freightModels.Split(',');
            for (int i = 0; i < fms.Length; i++)
            {
                if (!string.IsNullOrEmpty(fms[i]))
                {
                    productObj.FreightModels.Add(int.Parse(fms[i]));
                }
            }

            string strPics = validation.Get("pics", false, "请上传商品图片", @"\d+(,\d+)*", "参数错误");

            if (validation.HasError || productObj.CategoryID == 0)
                return Json(new { success = false, msg = "参数错误", errors = validation.GetErrors() });

            string[] cates = sCategories.Split(',');
            productObj.Categories = new List<int>();
            for (var i = 0; i < cates.Length; i++)
            {
                productObj.Categories.Add(int.Parse(cates[i]));
            }

            string[] pics = strPics.Split(',');
            ProductPictureObj productPictureObj;
            productObj.ProductPictures = new List<ProductPictureObj>();
            for (int i = 0; i < pics.Length; i++)
            {
                productPictureObj = new ProductPictureObj();
                productPictureObj.PictureID = int.Parse(pics[i]);
                productObj.ProductPictures.Add(productPictureObj);
            }

            ProductBLL productBLL = new ProductBLL();
            if (productBLL.IsProductExists(productObj.Name))
                return Json(new { success = false, msg = "该商品名称已存在!" });

            productBLL.AddProduct(productObj);

            if (productObj.ProductID != 0)
                return Json(new { success = true, id = productObj.ProductID });
            else
                return Json(new { success = false, msg = "抱歉，添加失败!" });
        }
        #endregion

        public ActionResult GetFreightModels()
        {
            return Json(new { success = true, data = new FreightModel().GetModels() });
        }

        #region 商品上、下架
        public ActionResult ModiftProductStatus(int id)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 404))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            ProductBLL productBLL = new ProductBLL();

            ProductObj productObj = productBLL.GetProduct(id);
            int status = productObj.Status == 1 ? 0 : 1;
            productBLL.ModifyProduct(id, status);

            return Json(new { success = true });
        }
        #endregion

        #region 删除商品
        public ActionResult DeleteProduct(int id)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 403))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            ProductBLL productBLL = new ProductBLL();

            ProductObj productObj = productBLL.GetProduct(id);

            DeletePics(productObj.ProductPictures);
            DeletePics(productObj.Colors);

            productBLL.DeleteProduct(id);

            return Json(new { success = true });
        }

        private void DeletePics(IList<ProductPictureObj> pics)
        {
            if (pics != null)
            {
                ProductPictureObj productPictureObj;
                for (int i = 0; i < pics.Count; i++)
                {
                    productPictureObj = pics[i];
                    string oldPath = Config.MediaPath + productPictureObj.SavePath;
                    try
                    {
                        System.IO.File.Delete(oldPath);
                    }
                    catch
                    {
                    }
                }
            }
        }
        #endregion

        #region 修改商品
        public ActionResult ModifyProduct()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 402))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation validation = new Validation();
            ProductBLL productBLL = new ProductBLL();

            int productId = validation.GetInt("id");
            string sCategories = validation.Get("categoryIds", false, "请选择商品类别", @"^\d+(\,\d+)*$", "请选择商品类别");
            ProductObj productObj = productBLL.GetProduct(productId);
            if (productObj == null)
                return Json(new { success = false, msg = "您要修改的商品不存在!" });

            productObj.Name = validation.Get("name", false, "请填写商品名称");
            productObj.Type = validation.Get("type");
            productObj.Serial = validation.Get("serial");
            productObj.Model = validation.Get("model");
            productObj.Code = validation.Get("code");
            productObj.Material = validation.Get("material");
            productObj.Weight = validation.Get("weight");
            productObj.Characteristic = validation.Get("characteristic");
            productObj.Designer = validation.Get("designer");
            productObj.Price = validation.GetDecimal("price");
            productObj.SpecialPrice = validation.GetDecimal("specialPrice");
            productObj.IsNew = validation.GetBool("isNew");
            productObj.IsRecommend = validation.GetBool("IsRecommend");
            productObj.IsOnSale = validation.GetBool("IsOnSale");
            productObj.CanPurchasedSeparately = validation.GetBool("canPurchasedSeparately");
            productObj.Description = HttpUtility.UrlDecode(validation.Get("description", false, "请填写商品描述"), Encoding.UTF8);
            productObj.Freight = validation.GetDecimal("freight");
            productObj.Freight1 = validation.GetDecimal("freight1");
            int quantity = validation.GetInt("quantity");
            productObj.Points = validation.GetInt("points");
            productObj.Quantity = quantity;
            productObj.Inventory = productObj.Inventory + quantity - productObj.Quantity;
            string strPics = validation.Get("pics", false, "请上传商品图片", @"\d+(,\d+)*", "参数错误");

            productObj.FreightModels = new List<int>();
            string freightModels = validation.Get("freightModels");
            var fms = freightModels.Split(',');
            for (int i = 0; i < fms.Length; i++)
            {
                if (!string.IsNullOrEmpty(fms[i]))
                {
                    productObj.FreightModels.Add(int.Parse(fms[i]));
                }
            }

            if (string.IsNullOrEmpty(strPics))
                return Json(new { success = false, msg = "请上传商品图片!" });

            if (validation.HasError)
                return Json(new { success = false, msg = "参数错误!" });

            string[] cates = sCategories.Split(',');
            productObj.Categories = new List<int>();
            for (var i = 0; i < cates.Length; i++)
            {
                productObj.Categories.Add(int.Parse(cates[i]));
            }

            string[] pics = strPics.Split(',');
            ProductPictureObj productPictureObj;
            productObj.ProductPictures = new List<ProductPictureObj>();
            for (int i = 0; i < pics.Length; i++)
            {
                productPictureObj = new ProductPictureObj();
                productPictureObj.PictureID = int.Parse(pics[i]);
                productObj.ProductPictures.Add(productPictureObj);
            }

            productBLL.ModifyProduct(productObj);

            return Json(new { success = true });
        }
        #endregion

        #region 添加产品图片
        public ActionResult InsertProductPic()
        {
            ViewBag.success = false;
            if (!AppData.IsManagerLogin)
            {
                ViewBag.msg = "抱歉，您未登录后台或会话已过期!";
                return View();
            }
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 4))
            {
                ViewBag.msg = "抱歉，您没有执行该操作的权限!";
                return View();
            }

            Validation vld = new Validation();
            int picID = vld.GetInt("id");
            string desc = vld.Get("desc");
            int type = vld.GetInt("type");

            HttpPostedFileBase pic = Request.Files.Count == 0 ? null : Request.Files[0];
            ProductBLL productBLL = new ProductBLL();
            if (pic != null && !string.IsNullOrEmpty(pic.FileName))
            {
                string ext = Path.GetExtension(pic.FileName);
                if (!Regex.IsMatch(ext, @"^\.(gif|jpg|jpeg|png)$", RegexOptions.IgnoreCase))
                {
                    ViewBag.msg = "上传的图片格式不合要求,请上传gif,png,jpg格式的图片";
                    return View();
                }

                string dirPath = Config.MediaPath + @"\Product";
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff") + ext;
                string savePath = Path.Combine(dirPath, newFileName);

                ProductPictureObj productPictureObj;
                if (picID == 0)
                {
                    productPictureObj = new ProductPictureObj();
                    productPictureObj.ProductID = string.IsNullOrEmpty(Request.Form["productID"]) ? 0 : int.Parse(Request.Form["productID"]);
                    productPictureObj.SavePath = @"\Product\" + newFileName;
                    productPictureObj.Url = "/Product/" + newFileName;
                    productPictureObj.PictureDesc = desc;
                    productPictureObj.Type = type;

                    productBLL.AddPicture(productPictureObj);
                }
                else
                {
                    productPictureObj = productBLL.GetProductPicture(picID);
                    System.IO.File.Delete(Config.MediaPath + productPictureObj.SavePath);
                    productPictureObj.SavePath = @"\Product\" + newFileName;
                    productPictureObj.Url = "/Product/" + newFileName;
                    productPictureObj.PictureDesc = desc;
                    productBLL.ModifyPicture(productPictureObj);
                }
                ViewBag.picId = productPictureObj.PictureID;

                pic.SaveAs(savePath);

                ViewBag.success = true;
                return View();
            }
            else if (picID == 0)
            {
                ViewBag.msg = type == 0 ? "请上传产品图片!" : "请上传颜色图片!";
                return View();
            }
            else
            {
                ViewBag.picId = picID;
                var productPictureObj = productBLL.GetProductPicture(picID);
                productPictureObj.PictureDesc = desc;
                productBLL.ModifyPicture(productPictureObj);

                ViewBag.success = true;
                return View();
            }

        }
        #endregion

        #region 删除产品图片
        public ActionResult DeleteProductPic(int pictureID)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 4))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            ProductBLL productBLL = new ProductBLL();
            ProductPictureObj productPictureObj = productBLL.GetProductPicture(pictureID);
            if (productPictureObj == null)
                return Json(new { success = false, msg = "图片不存在" });

            System.IO.File.Delete(Config.MediaPath + productPictureObj.SavePath);
            productBLL.DeleteProductPicture(pictureID);
            return Json(new { success = true });
        }
        #endregion

        #region 根据商品编号获取商品信息
        public ActionResult GetProductByID(int id)
        {
            ProductBLL productBLL = new ProductBLL();
            ProductObj result = productBLL.GetProduct(id);
            if (result != null)
                return Json(new { success = true, data = result });
            else
                return Json(new { success = false, msg = "该商品不存在" });

        }
        #endregion

        #region 查询商品
        public ActionResult SearchProducts()
        {
            Validation validation = new Validation();
            int categoryID = validation.GetInt("categoryID");
            int page = validation.GetInt("page", defaultValue: 1);
            int pageSize = validation.GetInt("pageSize", defaultValue: 10);
            int isNew = validation.GetInt("isNew");
            int isOnSale = validation.GetInt("isOnSale");
            int isRecommend = validation.GetInt("isRecommend");
            int canPurchasedSeparately = validation.GetInt("canPurchasedSeparately");
            string keywords = validation.Get("keywords");
            decimal priceFrom = validation.GetDecimal("priceFrom");
            decimal priceTo = validation.GetDecimal("priceTo");
            int status = validation.GetInt("status");
            int sort = validation.GetInt("sort");
            bool isAsc = validation.GetBool("isAsc");

            string sortField;
            switch (sort)
            {
                case 0:
                    sortField = "Products.ProductID";
                    break;
                case 1:
                    sortField = "Products.Quantity";
                    break;
                case 2:
                    sortField = "Products.Name";
                    break;
                case 3:
                    sortField = "Products.Price";
                    break;
                default:
                    sortField = "Products.ProductID";
                    break;
            }

            ProductBLL productBLL = new ProductBLL();
            int total;
            JsonArray result = productBLL.GetProducts(categoryID, keywords, priceFrom, priceTo, isNew, isOnSale, isRecommend, canPurchasedSeparately, page, pageSize, out total, status, sortField, isAsc);
            return Json(new { success = true, total = total, data = result });
        }
        #endregion

        public ActionResult AddModel()
        {
            if (!AppData.IsManagerLogin)
            {
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            }
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 4))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            string modelName = vld.Get("modelName", false);
            string content = vld.Get("content", false);
            content = HttpUtility.UrlDecode(content, Encoding.UTF8);

            if (vld.HasError)
            {
                return Json(new { success = false, msg = "请查看是否有未填的项" });
            }

            ProductBLL productBLL = new ProductBLL();
            productBLL.AddModel(modelName, content);
            return Json(new { success = true });
        }

        public ActionResult SaveModel()
        {
            if (!AppData.IsManagerLogin)
            {
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            }
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 4))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            int modelId = vld.GetInt("modelId");
            string modelName = vld.Get("modelName", false);
            string content = vld.Get("content", false);
            content = HttpUtility.UrlDecode(content, Encoding.UTF8);

            if (vld.HasError)
            {
                return Json(new { success = false, msg = "请查看是否有未填的项" });
            }

            ProductBLL productBLL = new ProductBLL();
            productBLL.SaveModel(modelId, modelName, content);
            return Json(new { success = true });
        }

        public ActionResult DeleteModel()
        {
            if (!AppData.IsManagerLogin)
            {
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            }
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 4))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            int modelId = vld.GetInt("modelId");

            ProductBLL productBLL = new ProductBLL();
            productBLL.DeleteModel(modelId);
            return Json(new { success = true });
        }


        public ActionResult GetModels()
        {
            if (!AppData.IsManagerLogin)
            {
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            }
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 4))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            ProductBLL productBLL = new ProductBLL();
            return Json(new { success = true, data = productBLL.GetModels() });
        }

        #region [页面] 商品评论
        public ActionResult Comments()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 5))
                return Redirect("/Manage/Error/2.html");

            return View();
        }
        #endregion

        #region 获取评论
        public ActionResult GetComments()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 5))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            string userName = vld.Get("userName");
            string product = vld.Get("product");
            DateTime dtFrom = vld.GetDateTime("dtFrom");
            DateTime dtTo = vld.GetDateTime("dtTo");
            int page = vld.GetInt("page", defaultValue: 1);
            int pageSize = vld.GetInt("pageSize", defaultValue: 1);

            ProductBLL productBLL = new ProductBLL();
            int total;
            var res = productBLL.GetComments(userName, product, dtFrom, dtTo, page, pageSize, out total);
            return Json(new { success = true, data = res, total = total });
        }
        #endregion

        #region 删除评论
        public ActionResult DeleteComment(int id)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 502))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            ProductBLL productBLL = new ProductBLL();
            productBLL.DeleteComment(id);
            return Json(new { success = true });
        }
        #endregion

        #region 回复
        public ActionResult ReComment()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 501))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                Validation vld = new Validation();
                int id = vld.GetInt("id");
                string re = vld.Get("re");

                ProductBLL productBLL = new ProductBLL();
                productBLL.ReComment(id, re);
                return Json(new { success = true });
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 501))
                    return Redirect("/Manage/Error/2.html");

                Validation vld = new Validation(false);
                int id = vld.GetInt("id");

                ProductBLL productBLL = new ProductBLL();
                ViewBag.re = productBLL.GetRe(id);
                return View();
            }
        }
        #endregion

        #region [页面] 商品留言
        public ActionResult Messages()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 6))
                return Redirect("/Manage/Error/2.html");

            return View();
        }
        #endregion

        #region 获取留言
        public ActionResult GetMessages()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 6))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();
            string product = vld.Get("product");
            DateTime dtFrom = vld.GetDateTime("dtFrom");
            DateTime dtTo = vld.GetDateTime("dtTo");
            int page = vld.GetInt("page", defaultValue: 1);
            int pageSize = vld.GetInt("pageSize", defaultValue: 1);

            ProductBLL productBLL = new ProductBLL();
            int total;
            var res = productBLL.GetMessages(product, dtFrom, dtTo, page, pageSize, out total);
            return Json(new { success = true, data = res, total = total });
        }
        #endregion

        #region 删除留言
        public ActionResult DeleteMessage(int id)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 602))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            ProductBLL productBLL = new ProductBLL();
            productBLL.DeleteMessage(id);
            return Json(new { success = true });
        }
        #endregion

        #region 回复留言
        public ActionResult ReMessage()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 601))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                Validation vld = new Validation();
                int id = vld.GetInt("id");
                string re = vld.Get("re");

                ProductBLL productBLL = new ProductBLL();
                productBLL.ReMessage(id, re);
                return Json(new { success = true });
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 601))
                    return Redirect("/Manage/Error/2.html");

                Validation vld = new Validation(false);
                int id = vld.GetInt("id");

                ProductBLL productBLL = new ProductBLL();
                ViewBag.re = productBLL.GetMessageRe(id);
                return View();
            }
        }
        #endregion

    }
}
