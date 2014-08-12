using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;
using System.Data;
using System.Data.SqlClient;

namespace Gllo.DAL
{
    public class FriendLinkDAL
    {
        SqlHelper helper;

        public void AddFriendLink(FriendLinkObj friendLinkObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@FriendName", 200, friendLinkObj.FriendName);
                helper.AddStringParameter("@Url", 200, friendLinkObj.Url);
                helper.AddStringParameter("@Pic", 200, friendLinkObj.Pic);
                helper.AddIntParameter("@Sort", friendLinkObj.Sort);

                helper.ExecuteNonQuery("insert into FriendLinks (FriendName,Url,Pic,Sort) values (@FriendName,@Url,@Pic,@Sort)", CommandType.Text);
            }
        }

        public void ModifyFriendLink(FriendLinkObj friendLinkObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@FriendID", friendLinkObj.FriendID);
                helper.AddStringParameter("@FriendName", 200, friendLinkObj.FriendName);
                helper.AddStringParameter("@Url", 200, friendLinkObj.Url);
                helper.AddStringParameter("@Pic", 200, friendLinkObj.Pic);
                helper.AddIntParameter("@Sort", friendLinkObj.Sort);

                helper.ExecuteNonQuery("update FriendLinks set FriendName=@FriendName,Url=@Url,Pic=@Pic,Sort=@Sort where FriendID=@FriendID", CommandType.Text);
            }
        }

        public void DeleteFriendLink(int friendId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@FriendID", friendId);

                helper.ExecuteNonQuery("delete from FriendLinks where FriendID=@FriendID", CommandType.Text);
            }
        }

        public FriendLinkObj GetFriendLinkByID(int friendId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@FriendID", friendId);

                using (SqlDataReader dr = helper.ExecuteReader("select FriendName,Url,Pic,Sort from FriendLinks where FriendID=@FriendID", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        FriendLinkObj friendLinkObj = new FriendLinkObj();
                        friendLinkObj.FriendID = friendId;
                        friendLinkObj.FriendName = dr[0] == DBNull.Value ? null : (string)dr[0];
                        friendLinkObj.Url = dr[1] == DBNull.Value ? null : (string)dr[1];
                        friendLinkObj.Pic = dr[2] == DBNull.Value ? null : (string)dr[2];
                        friendLinkObj.Sort = dr[3] == DBNull.Value ? 0 : (int)dr[3];

                        return friendLinkObj;
                    }
                    return null;
                }
            }
        }

        public JsonArray GetFriendLinks()
        {
            using (helper = new SqlHelper())
            {
                return helper.GetJsonArray("select FriendID,FriendName,Url,Pic,Sort from FriendLinks", CommandType.Text);
            }
        }
    }
}
