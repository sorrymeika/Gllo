using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;
using System.Data;
using System.Data.SqlClient;

namespace Gllo.DAL
{
    public class ExpressDAL
    {
        SqlHelper helper;

        #region 添加快递方式
        public void AddExpress(ExpressObj expressObj)
        {
            using (helper = new SqlHelper())
            {
                expressObj.ExpressID = helper.GetIntValue("select max(ExpressID) from Express", CommandType.Text) + 1;

                helper.AddIntParameter("@ExpressID", expressObj.ExpressID);
                helper.AddStringParameter("@ExpressName", 50, expressObj.ExpressName);
                helper.AddStringParameter("@ExpressUrl", 300, expressObj.ExpressUrl);

                helper.ExecuteNonQuery("insert into Express (ExpressID,ExpressName,ExpressUrl,DeletedFlag) values (@ExpressID,@ExpressName,@ExpressUrl,0)", CommandType.Text);
            }
        }
        #endregion

        #region 修改快递方式
        public void ModifyExpress(ExpressObj expressObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ExpressID", expressObj.ExpressID);
                helper.AddStringParameter("@ExpressName", 50, expressObj.ExpressName);
                helper.AddStringParameter("@ExpressUrl", 50, expressObj.ExpressUrl);

                helper.ExecuteNonQuery("update Express set ExpressName=@ExpressName,ExpressUrl=@ExpressUrl where ExpressID=@ExpressID", CommandType.Text);
            }
        }
        #endregion

        #region 删除快递方式
        public void DeleteExpress(int expressId, bool realDelete)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ExpressID", expressId);
                if (realDelete)
                    helper.ExecuteNonQuery("delete from Express where ExpressID=@ExpressID", CommandType.Text);
                else
                    helper.ExecuteNonQuery("update Express set DeletedFlag=0 where ExpressID=@ExpressID", CommandType.Text);

            }
        }
        #endregion

        #region 获取快递方式
        public IList<ExpressObj> GetExpress()
        {
            using (helper = new SqlHelper())
            {
                using (SqlDataReader dr = helper.ExecuteReader("select ExpressID,ExpressName,ExpressUrl from Express where DeletedFlag=0", CommandType.Text))
                {
                    if (dr.HasRows)
                    {
                        IList<ExpressObj> expressList = new List<ExpressObj>();
                        ExpressObj expressObj;
                        while (dr.Read())
                        {
                            expressObj = new ExpressObj();
                            expressObj.ExpressID = (int)dr[0];
                            expressObj.ExpressName = (string)dr[1];
                            expressObj.ExpressUrl = dr[2] == DBNull.Value ? "#" : (string)dr[2];
                            expressList.Add(expressObj);
                        }
                        return expressList;
                    }
                    else
                        return null;
                }
            }
        }
        #endregion

        #region 根据编号获取快递方式
        public ExpressObj GetExpress(int expressId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ExpressID", expressId);
                using (SqlDataReader dr = helper.ExecuteReader("select ExpressID,ExpressName,ExpressUrl from Express where DeletedFlag=0 and ExpressID=@ExpressID", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        ExpressObj expressObj = new ExpressObj();
                        expressObj.ExpressID = (int)dr[0];
                        expressObj.ExpressName = (string)dr[1];
                        expressObj.ExpressUrl = dr[2] == DBNull.Value ? "#" : (string)dr[2];
                        return expressObj;
                    }
                    else
                        return null;
                }
            }
        }
        #endregion

    }
}
