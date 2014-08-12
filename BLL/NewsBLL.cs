using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;

namespace Gllo.BLL
{
    public class NewsBLL
    {
        private readonly Gllo.DAL.NewsDAL dal = new DAL.NewsDAL();

        #region 新闻分类是否存在
        public bool IsNewsCateExists(int parentId, string categoryName)
        {
            return dal.IsNewsCateExists(parentId, categoryName);
        }
        #endregion

        #region 添加新闻分类
        public void AddNewsCate(NewsCateObj newsCateObj)
        {
            dal.AddNewsCate(newsCateObj);
        }
        #endregion

        #region 删除新闻分类
        public void DeleteNewsCate(int categoryID)
        {
            dal.DeleteNewsCate(categoryID);
        }
        #endregion

        #region 修改新闻分类
        public void ModifyNewsCate(NewsCateObj newsCateObj)
        {
            dal.ModifyNewsCate(newsCateObj);
        }
        #endregion

        #region 根据新闻分类编号获取分类信息
        public NewsCateObj GetNewsCateByCateID(int categoryID)
        {

            return dal.GetNewsCateByCateID(categoryID);
        }
        #endregion

        #region 获取全部新闻分类
        public JsonArray GetNewsCates(int patentID)
        {
            return dal.GetNewsCates(patentID);
        }
        #endregion

        #region 获取全部新闻分类
        public JsonArray GetNewsCates()
        {
            return dal.GetNewsCates();
        }
        #endregion

        #region 验证新闻分类中是否包含子类或新闻
        public bool IsNewsCateHasChildren(int categoryID)
        {
            return dal.IsNewsCateHasChildren(categoryID);
        }
        #endregion

        #region 添加新闻
        public void AddNews(NewsObj newsObj)
        {
            dal.AddNews(newsObj);
        }
        #endregion

        #region 修改新闻
        public void ModifyNews(NewsObj newsObj)
        {
            dal.ModifyNews(newsObj);
        }
        #endregion

        #region 删除新闻
        public void DeleteNews(int newsID)
        {
            dal.DeleteNews(newsID);
        }
        #endregion

        #region 新闻排序
        public void SortNews(int newsID, int beforeID)
        {
            dal.SortNews(newsID, beforeID);
        }
        #endregion

        #region 根据新闻编号获取新闻信息
        public NewsObj GetNews(int newsID)
        {
            return dal.GetNews(newsID);
        }
        #endregion

        public NewsObj GetPrevNews(int newsID, int categoryId)
        {
            return dal.GetPrevNews(newsID, categoryId);
        }

        public NewsObj GetNextNews(int newsID, int categoryId)
        {
            return dal.GetNextNews(newsID, categoryId);
        }

        #region 搜索新闻
        public JsonArray GetNews(int categoryId, string keywords, DateTime dtFrom, DateTime dtTo, int page, int pageSize, out int total)
        {
            return dal.GetNews(categoryId, keywords, dtFrom, dtTo, page, pageSize, out  total);
        }
        #endregion
    }
}
