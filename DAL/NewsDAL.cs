using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

using Gllo.Models;
using System.Data.SqlClient;

namespace Gllo.DAL
{
    public class NewsDAL
    {
        SqlHelper helper;

        #region 新闻分类是否存在
        public bool IsNewsCateExists(int parentId, string categoryName)
        {
            using (helper = new SqlHelper())
            {

                helper.AddIntParameter("@ParentId", parentId);
                helper.AddStringParameter("@CategoryName", 50, categoryName);

                return helper.GetBooleanValue("if exists (select 1 from NewsCates where CategoryName=@CategoryName) select 1 else select 0", CommandType.Text);
            }
        }
        #endregion

        #region 添加新闻分类
        public void AddNewsCate(NewsCateObj newsCateObj)
        {
            using (helper = new SqlHelper())
            {
                SqlParameter idParam = helper.AddOutputParameter("@CategoryID");
                helper.AddStringParameter("@CategoryName", 50, newsCateObj.CategoryName);
                helper.AddIntParameter("@ParentID", newsCateObj.ParentID);
                helper.AddDateTimeParameter("@Sort", newsCateObj.Sort);

                helper.ExecuteNonQuery("insert into NewsCates (CategoryName,ParentID,Sort) values (@CategoryName,@ParentID,@Sort) select @CategoryID=@@IDENTITY", CommandType.Text);

                newsCateObj.CategoryID = (int)idParam.Value;
            }
        }
        #endregion

        #region 删除新闻分类
        public void DeleteNewsCate(int categoryID)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@CategoryID", categoryID);
                helper.ExecuteNonQuery("delete from NewsCates where CategoryID=@CategoryID", CommandType.Text);
                helper.ExecuteNonQuery("delete from News where CategoryID in (select CategoryID from NewsCates where ParentId=@CategoryID)", CommandType.Text);
                helper.ExecuteNonQuery("delete from NewsCates where ParentId=@CategoryID", CommandType.Text);
                helper.ExecuteNonQuery("delete from News where CategoryID=@CategoryID", CommandType.Text);

                helper.CommitTran();
            }
        }
        #endregion

        #region 修改新闻分类
        public void ModifyNewsCate(NewsCateObj newsCateObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CategoryID", newsCateObj.CategoryID);
                helper.AddStringParameter("@CategoryName", 50, newsCateObj.CategoryName);
                helper.AddIntParameter("@ParentID", newsCateObj.ParentID);
                helper.AddDateTimeParameter("@Sort", newsCateObj.Sort);

                helper.ExecuteNonQuery("update NewsCates set CategoryName=@CategoryName,ParentID=@ParentID,Sort=@Sort where CategoryID=@CategoryID", CommandType.Text);
            }
        }
        #endregion

        #region 根据新闻分类编号获取分类信息
        public NewsCateObj GetNewsCateByCateID(int categoryID)
        {
            using (SqlHelper helper = new SqlHelper())
            {
                helper.AddIntParameter("@CategoryID", categoryID);
                using (SqlDataReader dr = helper.ExecuteReader("select CategoryName,ParentID,Sort from NewsCates where CategoryID=@CategoryID order by Sort", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        NewsCateObj newsCateObj = new NewsCateObj();
                        newsCateObj.CategoryID = categoryID;
                        newsCateObj.CategoryName = (string)dr[0];
                        newsCateObj.ParentID = (int)dr[1];
                        newsCateObj.Sort = (DateTime)dr[2];

                        return newsCateObj;
                    }
                    else
                        return null;
                }
            }
        }
        #endregion

        #region 获取全部新闻分类
        public JsonArray GetNewsCates()
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ParentID", 0);
                return GetNewsCatesByParentID(0);
            }
        }

        public JsonArray GetNewsCates(int parentID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ParentID", parentID);
                return GetNewsCatesByParentID(parentID);
            }
        }

        private JsonArray GetNewsCatesByParentID(int parentID)
        {
            helper.SetParameter("@ParentID", parentID);
            string sql = "select CategoryID,CategoryName,ParentID,Sort from NewsCates where ParentID=@ParentID order by Sort";
            JsonArray newsCateList;
            JsonObject newsCateObj;
            using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
            {
                if (dr.HasRows)
                {
                    newsCateList = new JsonArray();
                    while (dr.Read())
                    {
                        newsCateObj = new JsonObject();
                        newsCateObj.Add("categoryID", (int)dr[0]);
                        newsCateObj.Add("categoryName", (string)dr[1]);
                        newsCateObj.Add("parentID", dr[2] == DBNull.Value ? 0 : (int)dr[2]);
                        newsCateList.Add(newsCateObj);
                    }
                }
                else
                    newsCateList = null;
            }
            if (newsCateList != null)
            {
                JsonArray newsCateChildren;
                for (int i = 0; i < newsCateList.Count; i++)
                {
                    newsCateObj = newsCateList[i];
                    parentID = (int)newsCateObj["categoryID"];
                    newsCateChildren = GetNewsCates(parentID);
                    if (newsCateChildren != null)
                    {
                        newsCateObj.Add("children", newsCateChildren);
                    }
                }
            }
            return newsCateList;
        }
        #endregion

        #region 验证新闻分类中是否包含子类或新闻
        public bool IsNewsCateHasChildren(int categoryID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CategoryID", categoryID);
                bool result = helper.GetBooleanValue("if exists (select 1 from NewsCates where ParentID=@CategoryID) select 1 else select 0", CommandType.Text);
                if (!result)
                    result = helper.GetBooleanValue("if exists (select 1 from News where CategoryID=@CategoryID) select 1 else select 0", CommandType.Text);
                return result;
            }
        }
        #endregion

        #region 添加新闻
        public void AddNews(NewsObj newsObj)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();
                SqlParameter idParam = helper.AddOutputParameter("@NewsID");
                helper.AddStringParameter("@Title", 400, newsObj.Title);
                helper.AddStringParameter("@Writer", 200, newsObj.Writer);
                helper.AddIntParameter("@Clicks", newsObj.Clicks);
                helper.AddDateTimeParameter("@PublishDate", newsObj.PublishDate);
                helper.AddTextParameter("@Content", newsObj.Content);
                helper.AddStringParameter("@Tags", 100, newsObj.Tags);
                helper.AddIntParameter("@CategoryID", newsObj.CategoryID);
                helper.AddDateTimeParameter("@Sort", newsObj.Sort);

                string sql = "insert into News (Title,Writer,Clicks,PublishDate,Content,Tags,CategoryID,Sort) values (@Title,@Writer,@Clicks,@PublishDate,@Content,@Tags,@CategoryID,@Sort) select @NewsID=@@IDENTITY";
                helper.ExecuteNonQuery(sql, CommandType.Text);
                newsObj.NewsID = (int)idParam.Value;

                if (newsObj.Pages != null)
                {
                    helper.ClearParameters();
                    helper.AddIntParameter("@NewsID", newsObj.NewsID);
                    helper.AddIntParameter("@Page", 0);
                    helper.AddTextParameter("@Content", null);
                    sql = "insert into NewsPages (NewsID,Page,Content) values (@NewsID,@Page,@Content)";

                    for (int i = 0; i < newsObj.Pages.Count; i++)
                    {
                        helper.SetParameter("@Page", newsObj.Pages[i].Page);
                        helper.SetParameter("@Content", newsObj.Pages[i].Content);

                        helper.ExecuteNonQuery(sql, CommandType.Text);
                    }

                }

                helper.CommitTran();
            }
        }
        #endregion

        #region 修改新闻
        public void ModifyNews(NewsObj newsObj)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@NewsID", newsObj.NewsID);
                helper.AddStringParameter("@Title", 400, newsObj.Title);
                helper.AddStringParameter("@Writer", 200, newsObj.Writer);
                helper.AddIntParameter("@Clicks", newsObj.Clicks);
                helper.AddDateTimeParameter("@PublishDate", newsObj.PublishDate);
                helper.AddTextParameter("@Content", newsObj.Content);
                helper.AddStringParameter("@Tags", 100, newsObj.Tags);
                helper.AddIntParameter("@CategoryID", newsObj.CategoryID);
                helper.AddDateTimeParameter("@Sort", newsObj.Sort);

                string sql = "update News set Title=@Title,Writer=@Writer,Clicks=@Clicks,PublishDate=@PublishDate,Content=@Content,Tags=@Tags,CategoryID=@CategoryID,Sort=@Sort where NewsID=@NewsID";
                helper.ExecuteNonQuery(sql, CommandType.Text);

                helper.CommitTran();
            }
        }
        #endregion

        #region 删除新闻
        public void DeleteNews(int newsID)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@NewsID", newsID);

                string sql = "delete from News where NewsID=@NewsID";
                helper.ExecuteNonQuery(sql, CommandType.Text);

                helper.CommitTran();
            }
        }
        #endregion

        #region 新闻排序
        public void SortNews(int newsID, int beforeID)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@BeforeID", beforeID);

                string sql = "select Sort from News where NewsID=@BeforeID";
                DateTime sort = helper.GetDateTimeValue(sql, CommandType.Text);

                sql = "update News set Sort=@Sort where NewsID=@NewsID";
                helper.AddDateTimeParameter("@Sort", sort.AddMilliseconds(3.33));
                helper.AddIntParameter("@NewsID", newsID);
                helper.ExecuteNonQuery(sql, CommandType.Text);

                helper.CommitTran();
            }
        }
        #endregion

        #region 根据新闻编号获取新闻信息
        public NewsObj GetNews(int newsID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@NewsID", newsID);

                string sql = "select Title,Writer,Clicks,PublishDate,Content,Tags,CategoryID,Sort from News where NewsID=@NewsID";
                using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        NewsObj newsObj = new NewsObj();

                        newsObj.NewsID = newsID;
                        newsObj.Title = (string)dr[0];
                        newsObj.Writer = dr[1] == DBNull.Value ? null : (string)dr[1];
                        newsObj.Clicks = dr[2] == DBNull.Value ? 0 : (int)dr[2];
                        newsObj.PublishDate = dr[3] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[3];
                        newsObj.Content = dr[4] == DBNull.Value ? null : (string)dr[4];
                        newsObj.Tags = dr[5] == DBNull.Value ? null : (string)dr[5];
                        newsObj.CategoryID = dr[6] == DBNull.Value ? 0 : (int)dr[6];
                        newsObj.Sort = dr[7] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[7];

                        return newsObj;
                    }
                    return null;
                }
            }
        }
        #endregion

        public NewsObj GetPrevNews(int newsID, int categoryId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@NewsID", newsID);
                helper.AddIntParameter("@CategoryID", categoryId);

                string sql = "select top 1 Title,Writer,Clicks,PublishDate,Content,Tags,CategoryID,Sort,NewsID from News where NewsID<@NewsID and CategoryID=@CategoryID";
                using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        NewsObj newsObj = new NewsObj();

                        newsObj.Title = (string)dr[0];
                        newsObj.Writer = dr[1] == DBNull.Value ? null : (string)dr[1];
                        newsObj.Clicks = dr[2] == DBNull.Value ? 0 : (int)dr[2];
                        newsObj.PublishDate = dr[3] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[3];
                        newsObj.Content = dr[4] == DBNull.Value ? null : (string)dr[4];
                        newsObj.Tags = dr[5] == DBNull.Value ? null : (string)dr[5];
                        newsObj.CategoryID = dr[6] == DBNull.Value ? 0 : (int)dr[6];
                        newsObj.Sort = dr[7] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[7];
                        newsObj.NewsID = (int)dr[8];

                        return newsObj;
                    }
                    return null;
                }
            }
        }

        public NewsObj GetNextNews(int newsID, int categoryId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@NewsID", newsID);
                helper.AddIntParameter("@CategoryID", categoryId);

                string sql = "select top 1 Title,Writer,Clicks,PublishDate,Content,Tags,CategoryID,Sort,NewsID from News where NewsID>@NewsID and CategoryID=@CategoryID";
                using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        NewsObj newsObj = new NewsObj();

                        newsObj.Title = (string)dr[0];
                        newsObj.Writer = dr[1] == DBNull.Value ? null : (string)dr[1];
                        newsObj.Clicks = dr[2] == DBNull.Value ? 0 : (int)dr[2];
                        newsObj.PublishDate = dr[3] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[3];
                        newsObj.Content = dr[4] == DBNull.Value ? null : (string)dr[4];
                        newsObj.Tags = dr[5] == DBNull.Value ? null : (string)dr[5];
                        newsObj.CategoryID = dr[6] == DBNull.Value ? 0 : (int)dr[6];
                        newsObj.Sort = dr[7] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[7];
                        newsObj.NewsID = (int)dr[8];

                        return newsObj;
                    }
                    return null;
                }
            }
        }

        #region 搜索新闻
        public JsonArray GetNews(int categoryId, string keywords, DateTime dtFrom, DateTime dtTo, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                StringBuilder where = new StringBuilder("1=1");
                if (categoryId != 0)
                {
                    helper.AddIntParameter("@ParentID", 0);
                    StringBuilder cateIds = new StringBuilder();
                    GetCategories(categoryId, cateIds);
                    where.Append(" and News.CategoryID in (")
                        .Append(cateIds.ToString())
                        .Append(categoryId.ToString())
                        .Append(")");

                    helper.ClearParameters();
                }
                if (!string.IsNullOrEmpty(keywords))
                {
                    helper.AddStringParameter("@Keywords", 50, keywords);
                    where.Append(" and (Title like '%'+@Keywords+'%' or Tags like '%'+@Keywords+'%')");
                }

                if (dtFrom != DateTime.MinValue && dtTo != DateTime.MinValue)
                {
                    helper.AddDateTimeParameter("@DtFrom", dtFrom.Date);
                    helper.AddDateTimeParameter("@DtTo", dtTo.Date.AddDays(1).AddMilliseconds(-1));
                    where.Append(" and PublishDate between @DtFrom and @DtTo");
                }

                return helper.GetJsonArray(
                    new string[] { "NewsID" },
                    "NewsID,News.CategoryID,Title,Writer,Clicks,PublishDate,Content,Tags,CategoryName",
                    "News inner join NewsCates on News.CategoryID=NewsCates.CategoryID",
                    where.ToString(), page, pageSize, out  total,
                    new Dictionary<string, bool>{
                        {"News.Sort", false}
                    }
                );
            }
        }

        private List<int> GetCategories(int parentID, StringBuilder sb)
        {
            helper.SetParameter("@ParentID", parentID);
            string sql = "select CategoryID from NewsCates where ParentID=@ParentID";
            List<int> cateIds;
            using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
            {
                if (dr.HasRows)
                {
                    cateIds = new List<int>();
                    while (dr.Read())
                    {
                        parentID = (int)dr[0];
                        cateIds.Add(parentID);
                        sb.Append(parentID.ToString())
                            .Append(",");
                    }
                }
                else
                    cateIds = null;
            }
            if (cateIds != null)
            {
                for (int i = 0; i < cateIds.Count; i++)
                {
                    parentID = cateIds[i];
                    GetCategories(parentID, sb);
                }
            }
            return cateIds;
        }
        #endregion

    }
}
