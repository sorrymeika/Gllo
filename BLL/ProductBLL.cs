using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;

namespace Gllo.BLL
{
    public class ProductBLL
    {
        private readonly Gllo.DAL.ProductDAL dal = new DAL.ProductDAL();

        #region 添加商品分类
        public void AddProductCate(ProductCateObj productCateObj)
        {
            dal.AddProductCate(productCateObj);
        }
        #endregion

        #region 删除商品分类
        public void DeleteProductCate(int categoryID)
        {
            dal.DeleteProductCate(categoryID);
        }
        #endregion

        #region 修改商品分类
        public void ModifyProductCate(ProductCateObj productCateObj)
        {
            dal.ModifyProductCate(productCateObj);
        }
        #endregion

        #region 根据商品分类编号获取分类信息
        public ProductCateObj GetProductCateByCateID(int categoryID)
        {
            return dal.GetProductCateByCateID(categoryID);

        }
        #endregion

        #region 获取全部商品分类
        public JsonArray GetProductCates()
        {
            return dal.GetProductCates();
        }
        #endregion

        #region 获取全部父类
        public JsonArray GetParentCates()
        {
            return dal.GetProductCatesByParentID(0);
        }
        #endregion

        #region 根据父类编号获取子类
        public JsonArray GetProductCatesByParentID(int parentID)
        {
            return dal.GetProductCatesByParentID(parentID);
        }
        #endregion

        #region 验证商品分类中是否包含子类或商品
        public bool IsProductCateHasChildren(int categoryID)
        {
            return dal.IsProductCateHasChildren(categoryID);
        }
        #endregion

        #region 判断商品是否存在
        public bool IsProductExists(string productName)
        {
            return dal.IsProductExists(productName);
        }
        #endregion

        #region 添加商品
        public void AddProduct(ProductObj productObj)
        {
            dal.AddProduct(productObj);
        }
        #endregion

        #region 修改商品
        public void ModifyProduct(ProductObj productObj)
        {
            dal.ModifyProduct(productObj);
        }
        #endregion

        #region 修改商品状态
        public void ModifyProduct(int productId, int status)
        {
            dal.ModifyProduct(productId, status);
        }
        #endregion

        #region 删除商品
        public void DeleteProduct(int productID)
        {
            dal.DeleteProduct(productID);
        }
        #endregion

        #region 根据商品编号获取商品信息
        public ProductObj GetProduct(int productID)
        {
            return dal.GetProduct(productID);
        }
        #endregion

        #region 搜索商品分类筛选
        public JsonArray GetProductCates(string keywords)
        {
            return dal.GetProductCates(keywords);
        }
        #endregion

        #region 获取相关标签
        public JsonArray GetTags(string keywords)
        {
            return dal.GetTags(keywords);
        }
        #endregion

        #region 搜索商品
        public JsonArray GetProducts(int categoryId, string keywords, decimal priceFrom, decimal priceTo, int isNew, int isOnSale, int isRecommend, int canPurchasedSeparately, int page, int pageSize, out int total, int status = 1, string sort = "Products.Sort", bool isAsc = false)
        {
            return dal.GetProducts(categoryId, keywords, priceFrom, priceTo, isNew, isOnSale, isRecommend, canPurchasedSeparately, status, page, pageSize, out total, sort, isAsc);
        }
        #endregion

        #region 根据商品编号列表获取商品
        public JsonArray GetProducts(IList<int> ids)
        {
            return dal.GetProducts(ids);
        }
        #endregion

        #region GetJsonProductByID
        public JsonObject GetJsonProductByID(int id)
        {
            return dal.GetJsonProductByID(id);
        }
        #endregion

        #region 新增商品图片
        public void AddPicture(ProductPictureObj productPictureObj)
        {
            dal.AddPicture(productPictureObj);
        }
        #endregion

        #region 修改商品图片
        public void ModifyPicture(ProductPictureObj productPictureObj)
        {
            dal.ModifyPicture(productPictureObj);
        }
        #endregion

        #region 根据图片编号获取商品图片
        public ProductPictureObj GetProductPicture(int pictureID)
        {
            return dal.GetProductPicture(pictureID);
        }
        #endregion

        #region 删除产品图片
        public int DeleteProductPicture(int pictureID)
        {
            return dal.DeleteProductPicture(pictureID);
        }
        #endregion

        public void AddModel(string modelName, string content)
        {
            dal.AddModel(modelName, content);
        }

        public void SaveModel(int modelId, string modelName, string content)
        {
            dal.SaveModel(modelId, modelName, content);

        }

        public JsonArray GetModels()
        {
            return dal.GetModels();
        }

        public void DeleteModel(int modelId)
        {
            dal.DeleteModel(modelId);
        }

        #region 添加评论
        public void AddComment(int userID, int productID, string content, int score)
        {
            dal.AddComment(userID, productID, content, score);
        }
        #endregion

        #region 回复评论
        public void ReComment(int commentID, string re)
        {
            dal.ReComment(commentID, re);
        }
        #endregion

        #region 删除评论
        public void DeleteComment(int commentID)
        {
            dal.DeleteComment(commentID);
        }

        public void DeleteComment(int userId, int commentID)
        {
            dal.DeleteComment(userId, commentID);
        }
        #endregion

        #region 获取评论
        public JsonArray GetComments(string userName, string product, DateTime dtFrom, DateTime dtTo, int page, int pageSize, out int total)
        {
            return dal.GetComments(userName, product, dtFrom, dtTo, page, pageSize, out  total);
        }

        public JsonArray GetComments(int productId, int page, int pageSize, out int total)
        {
            return dal.GetComments(productId, page, pageSize, out  total);
        }

        public string GetRe(int commentID)
        {
            return dal.GetRe(commentID);
        }
        #endregion

        public JsonArray GetCommentsByUserID(int userId, int page, int pageSize, out int total)
        {
            return dal.GetCommentsByUserID(userId, page, pageSize, out  total);
        }

        #region 判断用户是否购买过某商品
        public bool IsBuy(int userID, int productID)
        {
            return dal.IsBuy(userID, productID);
        }
        #endregion

        #region AddMessage
        public void AddMessage(string userName, int productID, string content, bool isAnonymity)
        {
            dal.AddMessage(userName, productID, content, isAnonymity);
        }
        #endregion

        #region ReMessage
        public void ReMessage(int messageID, string re)
        {
            dal.ReMessage(messageID, re);
        }
        #endregion

        #region DeleteMessage
        public void DeleteMessage(int messageID)
        {
            dal.DeleteMessage(messageID);
        }
        #endregion

        #region GetMessages
        public JsonArray GetMessages(int productID, int page, int pageSize, out int total)
        {
            return dal.GetMessages(productID, page, pageSize, out total);
        }
        #endregion

        #region GetMessages
        public JsonArray GetMessages(string product, DateTime dtFrom, DateTime dtTo, int page, int pageSize, out int total)
        {
            return dal.GetMessages(product, dtFrom, dtTo, page, pageSize, out total);
        }
        #endregion

        #region 获取评论
        public string GetMessageRe(int messageID)
        {
            return dal.GetMessageRe(messageID);
        }
        #endregion

        public int GetProductIDByCode(string code)
        {
            return dal.GetProductIDByCode(code);
        }

        public decimal[] GetProductFreight(int productID, int regionID)
        {
            var res = dal.GetProductFreight(productID, regionID);
            if (res == null)
            {
                return new decimal[] { 0, 0 };
            }
            return res;
        }

        public bool IsComment(int userID, int productId)
        {
            return dal.IsComment(userID, productId);
        }
    }
}
