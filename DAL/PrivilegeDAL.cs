using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;
using System.Data;
using System.Data.SqlClient;

namespace Gllo.DAL
{
    public class PrivilegeDAL
    {
        private SqlHelper helper;

        #region AddPrivilegeGroup
        public void AddPrivilegeGroup(string groupName)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@GroupName", 200, groupName);
                helper.ExecuteNonQuery("insert into PrivilegeGroups (GroupName) values (@GroupName)", CommandType.Text);
            }
        }
        #endregion

        #region ModifyPrivilegeGroup
        public void ModifyPrivilegeGroup(PrivilegeGroupObj groupObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@GroupID", groupObj.GroupID);
                helper.AddStringParameter("@GroupName", 200, groupObj.GroupName);

                helper.ExecuteNonQuery("update PrivilegeGroups set GroupName=@GroupName where GroupID=@GroupID", CommandType.Text);
            }
        }
        #endregion

        #region GetPrivilegeGroups
        public JsonArray GetPrivilegeGroups()
        {
            using (helper = new SqlHelper())
            {
                return helper.GetJsonArray("select GroupID,GroupName from PrivilegeGroups", CommandType.Text);
            }
        }
        #endregion

        #region GetGroupByID
        public PrivilegeGroupObj GetGroupByID(int groupID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@GroupID", groupID);
                using (SqlDataReader dr = helper.ExecuteReader("select GroupName from PrivilegeGroups where GroupID=@GroupID", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        PrivilegeGroupObj groupObj = new PrivilegeGroupObj();
                        groupObj.GroupID = groupID;
                        groupObj.GroupName = dr[0] == DBNull.Value ? null : (string)dr[0];

                        return groupObj;
                    }

                    return null;
                }
            }
        }
        #endregion

        #region DeletePrivilegeGroup
        public void DeletePrivilegeGroup(int groupID)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@GroupID", groupID);
                helper.ExecuteNonQuery("delete from PrivilegeGroups where GroupID=@GroupID", CommandType.Text);
                helper.ExecuteNonQuery("delete from Users where GroupID=@GroupID", CommandType.Text);
                helper.ExecuteNonQuery("delete from LPrivilegeGroup where GroupID=@GroupID", CommandType.Text);

                helper.CommitTran();
            }
        }
        #endregion


        #region GetGroupPrivileges
        public IList<int> GetGroupPrivileges(int groupID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@GroupID", groupID);

                using (SqlDataReader dr = helper.ExecuteReader("select PrivilegeID from LPrivilegeGroup where GroupID=@GroupID", CommandType.Text))
                {
                    if (dr.HasRows)
                    {
                        IList<int> privilegeIds = new List<int>();
                        while (dr.Read())
                        {
                            privilegeIds.Add((int)dr[0]);
                        }
                        return privilegeIds;
                    }
                }

                return null;
            }
        }
        #endregion

        #region SetPrivileges
        public void SetPrivileges(int groupID, IList<int> privilegeIds)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@GroupID", groupID);

                helper.BeginTran();

                helper.ExecuteNonQuery("delete from LPrivilegeGroup where GroupID=@GroupID", CommandType.Text);

                if (privilegeIds != null)
                {
                    helper.AddIntParameter("@PrivilegeID", 0);
                    for (int i = 0; i < privilegeIds.Count; i++)
                    {
                        helper.SetParameter("@PrivilegeID", privilegeIds[i]);
                        helper.ExecuteNonQuery("insert into LPrivilegeGroup (GroupID,PrivilegeID) values (@GroupID,@PrivilegeID)", CommandType.Text);
                    }
                }

                helper.CommitTran();
            }
        }
        #endregion

        #region GetPrivileges
        public JsonArray GetPrivileges()
        {
            using (helper = new SqlHelper())
            {
                JsonArray cates = helper.GetJsonArray("select CategoryID,CategoryName from PrivilegeCates", CommandType.Text);

                if (cates != null)
                {
                    int categoryID;
                    helper.AddIntParameter("@CategoryID", 0);
                    foreach (var cate in cates)
                    {
                        categoryID = (int)cate["CategoryID"];
                        helper.SetParameter("@CategoryID", categoryID);

                        var privileges = helper.GetJsonArray("select PrivilegeID,PrivilegeName,Ico,Url,Type,CategoryID from Privileges where CategoryID=@CategoryID", CommandType.Text);
                        cate.Add("Privileges", privileges);
                    }
                }
                return cates;
            }
        }
        #endregion

        #region GetUsers
        public JsonArray GetUsers(string userName, int groupID, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                StringBuilder where = new StringBuilder("UserRole=3");
                if (!string.IsNullOrEmpty(userName))
                {
                    helper.AddStringParameter("@UserName", 50, userName);
                    where.Append(" and UserName like '%'+@UserName+'%'");
                }
                if (groupID != 0)
                {
                    helper.AddIntParameter("@GroupID", groupID);
                    where.Append(" and a.GroupID=@GroupID");
                }

                return helper.GetJsonArray("UserID",
                    "UserID,UserName,a.GroupID,GroupName",
                    "Users a left join PrivilegeGroups b on a.GroupID=b.GroupID",
                    where.ToString(),
                    page,
                    pageSize,
                    out total,
                    "UserID",
                    true);
            }
        }
        #endregion

        #region AddUser
        public void AddUser(string userName, string password, int groupID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@UserName", 50, userName);
                helper.AddStringParameter("@Password", 50, password);
                helper.AddIntParameter("@GroupID", groupID);

                helper.ExecuteNonQuery("insert into Users (UserName,UserRole,Password,RegDate,GroupID) values (@UserName,3,@Password,GetDate(),@GroupID)", CommandType.Text);
            }
        }
        #endregion

        #region ModifyUser
        public void ModifyUser(int userID, string userName, int groupID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userID);
                helper.AddStringParameter("@UserName", 50, userName);
                helper.AddIntParameter("@GroupID", groupID);

                helper.ExecuteNonQuery("update Users set UserName=@UserName,GroupID=@GroupID where UserID=@UserID", CommandType.Text);
            }
        }
        #endregion

        #region GetUserByID
        public JsonObject GetUserByID(int userID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userID);

                var res = helper.GetJsonArray("select UserName,UserRole,Password,RegDate,GroupID from Users where UserID=@UserID", CommandType.Text);
                if (res == null || res.Count == 0)
                    return null;

                return res[0];
            }
        }
        #endregion

        #region DeleteUser
        public void DeleteUser(int userID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userID);
                helper.ExecuteNonQuery("delete from Users where UserID=@UserID", CommandType.Text);
            }
        }
        #endregion

        #region HasNotPrivilege
        public bool HasNotPrivilege(int userID, int pid)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userID);
                helper.AddIntParameter("@PrivilegeID", pid);
                return helper.GetBooleanValue("if exists (select 1 from Users a inner join PrivilegeGroups b on a.GroupID=b.GroupID inner join LPrivilegeGroup c on a.GroupID=c.GroupID where UserID=@UserID and PrivilegeID=@PrivilegeID) select 0 else select 1", CommandType.Text);
            }
        }
        #endregion

        public JsonArray GetUserPrivileges(int userID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userID);

                JsonArray cates = helper.GetJsonArray("select CategoryID,CategoryName from PrivilegeCates", CommandType.Text);

                if (cates != null)
                {
                    int categoryID;
                    helper.AddIntParameter("@CategoryID", 0);
                    foreach (var cate in cates)
                    {
                        categoryID = (int)cate["CategoryID"];
                        helper.SetParameter("@CategoryID", categoryID);

                        var privileges = helper.GetJsonArray("select a.PrivilegeID,PrivilegeName,Ico,Url,CategoryID from Privileges a inner join LPrivilegeGroup b on a.PrivilegeID=b.PrivilegeID inner join PrivilegeGroups c on b.GroupID=c.GroupID inner join Users d on d.GroupID=c.GroupID where CategoryID=@CategoryID and a.Type=0 and d.UserID=@UserID", CommandType.Text);
                        cate.Add("Privileges", privileges);
                    }
                }
                return cates;

            }
        }

    }
}
