using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;
using System.Data;
using System.Data.SqlClient;

namespace Gllo.DAL
{
    public class PhotoDAL
    {
        SqlHelper helper;

        #region IsPhotoExists
        public bool IsPhotoExists(string photoName)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@Name", 500, photoName);

                return helper.GetBooleanValue("if exists (select 1 from Photos where Name=@Name) select 1 else select 0", CommandType.Text);
            }
        }
        #endregion

        #region AddPhoto
        public void AddPhoto(PhotoObj photoObj)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                var id = helper.AddOutputParameter("@PhotoID");

                helper.AddStringParameter("@Pic", 500, photoObj.Pic);
                helper.AddStringParameter("@Name", 500, photoObj.Name);
                helper.AddTextParameter("@Info", photoObj.Info);

                helper.ExecuteNonQuery("insert into Photos (Pic,Name,Info) values (@Pic,@Name,@Info) select @PhotoID=@@IDENTITY", CommandType.Text);
                photoObj.PhotoID = (int)id.Value;

                for (int i = 0; i < photoObj.Categories.Count; i++)
                {
                    helper.ClearParameters();
                    helper.AddIntParameter("@PhotoID", photoObj.PhotoID);
                    helper.AddIntParameter("@CategoryID", photoObj.Categories[i]);
                    helper.ExecuteNonQuery("insert into LPhotoCate (PhotoID,CategoryID) values (@PhotoID,@CategoryID)", CommandType.Text);
                }

                helper.CommitTran();
            }
        }
        #endregion

        #region ModifyPhoto
        public void ModifyPhoto(PhotoObj photoObj)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@PhotoID", photoObj.PhotoID);
                helper.AddStringParameter("@Pic", 500, photoObj.Pic);
                helper.AddStringParameter("@Name", 500, photoObj.Name);
                helper.AddTextParameter("@Info", photoObj.Info);

                helper.ExecuteNonQuery("update Photos set Pic=@Pic,Name=@Name,Info=@Info where PhotoID=@PhotoID", CommandType.Text);

                helper.ExecuteNonQuery("delete from LPhotoCate where PhotoID=@PhotoID", CommandType.Text);

                for (int i = 0; i < photoObj.Categories.Count; i++)
                {
                    helper.ClearParameters();
                    helper.AddIntParameter("@PhotoID", photoObj.PhotoID);
                    helper.AddIntParameter("@CategoryID", photoObj.Categories[i]);
                    helper.ExecuteNonQuery("insert into LPhotoCate (PhotoID,CategoryID) values (@PhotoID,@CategoryID)", CommandType.Text);
                }

                helper.CommitTran();
            }
        }
        #endregion

        #region DeletePhoto
        public void DeletePhoto(int photoID)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@PhotoID", photoID);

                helper.ExecuteNonQuery("delete from Photos where PhotoID=@PhotoID", CommandType.Text);
                helper.ExecuteNonQuery("delete from LPhotoCate where PhotoID=@PhotoID", CommandType.Text);
                helper.ExecuteNonQuery("delete from PhotoPoints where PhotoID=@PhotoID", CommandType.Text);

                helper.CommitTran();
            }
        }
        #endregion

        #region GetPhotoByID
        public PhotoObj GetPhotoByID(int photoID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@PhotoID", photoID);
                PhotoObj photoObj;
                SqlDataReader dr;
                using (dr = helper.ExecuteReader("select Pic,Name,Info from Photos where PhotoID=@PhotoID", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        photoObj = new PhotoObj();
                        photoObj.PhotoID = photoID;
                        photoObj.Pic = dr[0] == DBNull.Value ? null : (string)dr[0];
                        photoObj.Name = dr[1] == DBNull.Value ? null : (string)dr[1];
                        photoObj.Info = dr[2] == DBNull.Value ? null : (string)dr[2];
                    }
                    else
                        photoObj = null;
                }
                if (photoObj != null)
                {
                    using (dr = helper.ExecuteReader("select CategoryID from LPhotoCate where PhotoID=@PhotoID", CommandType.Text))
                    {
                        if (dr.HasRows)
                        {
                            photoObj.Categories = new List<int>();
                            while (dr.Read())
                            {
                                photoObj.Categories.Add((int)dr[0]);
                            }
                        }
                    }
                }
                return photoObj;
            }
        }
        #endregion

        #region GetPhotos
        public JsonArray GetPhotos(IList<int> categoryIDs, string keywords, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                StringBuilder where = new StringBuilder("1=1");
                StringBuilder table = new StringBuilder("Photos");
                if (categoryIDs != null && categoryIDs.Count != 0)
                {
                    helper.AddIntParameter("@ParentID", 0);
                    table.Append(" inner join (select PhotoID as LPID from LPhotoCate where 1=1");
                    int categoryId;
                    for (int i = 0; i < categoryIDs.Count; i++)
                    {
                        categoryId = categoryIDs[i];
                        StringBuilder cateIds = new StringBuilder();
                        GetCategories(categoryId, cateIds);
                        table.Append(" and CategoryID in (")
                            .Append(cateIds.ToString())
                            .Append(categoryId.ToString())
                            .Append(")");
                    }
                    table.Append(" group by PhotoID) a on LPID=Photos.PhotoID");
                    helper.ClearParameters();
                }

                if (!string.IsNullOrEmpty(keywords))
                {
                    helper.AddStringParameter("@Keywords", 200, keywords);
                    where.Append(" and (Name like '%'+@Keywords+'%')");
                }

                return helper.GetJsonArray("PhotoID",
                    "PhotoID,Pic,Name,Info",
                    table.ToString(),
                    where.ToString(),
                    page,
                    pageSize,
                    out total,
                    "PhotoID",
                    false);
            }
        }

        public JsonArray GetPhotos(IList<int> categoryIDs)
        {
            using (helper = new SqlHelper())
            {
                StringBuilder sql = new StringBuilder();
                StringBuilder where = new StringBuilder(" where 1=1");
                StringBuilder table = new StringBuilder("Photos");
                if (categoryIDs != null && categoryIDs.Count != 0)
                {
                    helper.AddIntParameter("@ParentID", 0);
                    table.Append(" inner join (select PhotoID as LPID from LPhotoCate where 1=1");
                    int categoryId;
                    for (int i = 0; i < categoryIDs.Count; i++)
                    {
                        categoryId = categoryIDs[i];
                        StringBuilder cateIds = new StringBuilder();
                        GetCategories(categoryId, cateIds);
                        table.Append(" and CategoryID in (")
                            .Append(cateIds.ToString())
                            .Append(categoryId.ToString())
                            .Append(")");
                    }
                    table.Append(" group by PhotoID) a on LPID=Photos.PhotoID");
                    helper.ClearParameters();
                }

                sql.Append("select PhotoID,Pic,Name,Info from ")
                    .Append(table)
                    .Append(where);

                JsonArray res = helper.GetJsonArray(sql.ToString(), CommandType.Text);
                if (res != null)
                {
                    JsonObject dict;
                    helper.ClearParameters();
                    helper.AddIntParameter("@PhotoID", 0);
                    for (int i = 0; i < res.Count; i++)
                    {
                        dict = res[i];
                        helper.SetParameter("@PhotoID", dict["PhotoID"]);

                        dict.Add("Points", helper.GetJsonArray("select PointID,PhotoID,X,Y,Pic,Name,Cap from PhotoPoints where PhotoID=@PhotoID", CommandType.Text));
                    }
                }

                return res;
            }
        }

        private List<int> GetCategories(int parentID, StringBuilder sb)
        {
            helper.SetParameter("@ParentID", parentID);
            string sql = "select CategoryID from PackageCates where ParentID=@ParentID";
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

        #region AddPhotoPoint
        public void AddPhotoPoint(PhotoPointObj photoPointObj)
        {
            using (helper = new SqlHelper())
            {
                var id = helper.AddOutputParameter("@PointID");

                helper.AddIntParameter("@PhotoID", photoPointObj.PhotoID);
                helper.AddIntParameter("@X", photoPointObj.X);
                helper.AddIntParameter("@Y", photoPointObj.Y);
                helper.AddStringParameter("@Pic", 500, photoPointObj.Pic);
                helper.AddStringParameter("@Name", 500, photoPointObj.Name);
                helper.AddTextParameter("@Cap", photoPointObj.Cap);

                helper.ExecuteNonQuery("insert into PhotoPoints (PhotoID,X,Y,Pic,Name,Cap) values (@PhotoID,@X,@Y,@Pic,@Name,@Cap) select @PointID=@@IDENTITY", CommandType.Text);

                photoPointObj.PointID = (int)id.Value;
            }
        }
        #endregion

        #region ModifyPhotoPoint
        public void ModifyPhotoPoint(PhotoPointObj photoPointObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@PointID", photoPointObj.PointID);
                helper.AddIntParameter("@PhotoID", photoPointObj.PhotoID);
                helper.AddIntParameter("@X", photoPointObj.X);
                helper.AddIntParameter("@Y", photoPointObj.Y);
                helper.AddStringParameter("@Pic", 500, photoPointObj.Pic);
                helper.AddStringParameter("@Name", 500, photoPointObj.Name);
                helper.AddTextParameter("@Cap", photoPointObj.Cap);

                helper.ExecuteNonQuery("update PhotoPoints set PhotoID=@PhotoID,X=@X,Y=@Y,Pic=@Pic,Name=@Name,Cap=@Cap where PointID=@PointID", CommandType.Text);
            }
        }
        #endregion

        #region DeletePhotoPoint
        public void DeletePhotoPoint(int pointID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@PointID", pointID);

                helper.ExecuteNonQuery("delete from PhotoPoints where PointID=@PointID", CommandType.Text);
            }
        }
        #endregion

        #region GetPhotoPoints
        public JsonArray GetPhotoPoints(int photoID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@PhotoID", photoID);
                return helper.GetJsonArray("select PointID,PhotoID,X,Y,Pic,Name,Cap from PhotoPoints where PhotoID=@PhotoID", CommandType.Text);
            }
        }
        #endregion

        #region GetPhotoPointByID
        public PhotoPointObj GetPhotoPointByID(int pointID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@PointID", pointID);
                using (SqlDataReader dr = helper.ExecuteReader("select PhotoID,X,Y,Pic,Name,Cap from PhotoPoints where PointID=@PointID", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        PhotoPointObj photoPointObj = new PhotoPointObj();
                        photoPointObj.PointID = pointID;
                        photoPointObj.PhotoID = dr[0] == DBNull.Value ? 0 : (int)dr[0];
                        photoPointObj.X = dr[1] == DBNull.Value ? 0 : (int)dr[1];
                        photoPointObj.Y = dr[2] == DBNull.Value ? 0 : (int)dr[2];
                        photoPointObj.Pic = dr[3] == DBNull.Value ? null : (string)dr[3];
                        photoPointObj.Name = dr[4] == DBNull.Value ? null : (string)dr[4];
                        photoPointObj.Cap = dr[5] == DBNull.Value ? null : (string)dr[5];

                        return photoPointObj;
                    }
                    return null;
                }
            }
        }
        #endregion


    }
}
