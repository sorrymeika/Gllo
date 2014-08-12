using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;

namespace Gllo.BLL
{
    public class PrivilegeBLL
    {
        private readonly Gllo.DAL.PrivilegeDAL dal = new DAL.PrivilegeDAL();

        #region AddPrivilegeGroup
        public void AddPrivilegeGroup(string groupName)
        {
            dal.AddPrivilegeGroup(groupName);
        }
        #endregion

        #region ModifyPrivilegeGroup
        public void ModifyPrivilegeGroup(PrivilegeGroupObj groupObj)
        {
            dal.ModifyPrivilegeGroup(groupObj);
        }
        #endregion

        #region GetPrivilegeGroups
        public JsonArray GetPrivilegeGroups()
        {
            return dal.GetPrivilegeGroups();
        }
        #endregion

        #region GetGroupByID
        public PrivilegeGroupObj GetGroupByID(int groupID)
        {
            return dal.GetGroupByID(groupID);
        }
        #endregion

        #region DeletePrivilegeGroup
        public void DeletePrivilegeGroup(int groupID)
        {
            dal.DeletePrivilegeGroup(groupID);
        }
        #endregion

        #region GetGroupPrivileges
        public IList<int> GetGroupPrivileges(int groupID)
        {
            return dal.GetGroupPrivileges(groupID);
        }
        #endregion

        #region SetPrivileges
        public void SetPrivileges(int groupID, IList<int> privilegeIds)
        {
            dal.SetPrivileges(groupID, privilegeIds);
        }
        #endregion

        #region GetPrivileges
        public JsonArray GetPrivileges()
        {
            return dal.GetPrivileges();
        }
        #endregion

        #region GetUsers
        public JsonArray GetUsers(string userName, int groupID, int page, int pageSize, out int total)
        {
            return dal.GetUsers(userName, groupID, page, pageSize, out  total);
        }
        #endregion

        #region AddUser
        public void AddUser(string userName, string password, int groupID)
        {
            dal.AddUser(userName, password, groupID);
        }
        #endregion

        #region ModifyUser
        public void ModifyUser(int userID, string userName, int groupID)
        {
            dal.ModifyUser(userID, userName, groupID);
        }
        #endregion

        #region GetUserByID
        public JsonObject GetUserByID(int userID)
        {
            return dal.GetUserByID(userID);
        }
        #endregion

        #region DeleteUser
        public void DeleteUser(int userID)
        {
            dal.DeleteUser(userID);
        }
        #endregion

        #region HasNotPrivilege
        public static bool HasNotPrivilege(int userID, int pid)
        {
            return false;
            //return new DAL.PrivilegeDAL().HasNotPrivilege(userID, pid);
        }
        #endregion

        public JsonArray GetUserPrivileges(int userID)
        {
            return dal.GetUserPrivileges(userID);
        }
    }
}
