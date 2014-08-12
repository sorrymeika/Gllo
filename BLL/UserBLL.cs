using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;

namespace Gllo.BLL
{
    public class UserBLL
    {
        private readonly Gllo.DAL.UserDAL dal = new DAL.UserDAL();

        public bool Login(UserObj user)
        {
            return dal.Login(user);
        }

        public bool IsRegist(string userName)
        {
            return dal.IsRegist(userName);
        }

        #region 用户注册
        public void Register(UserObj userObj)
        {
            dal.Register(userObj);
        }
        #endregion

        #region 获取配送地址
        public JsonArray GetUserAddress(int userId)
        {
            return dal.GetUserAddress(userId);
        }
        #endregion

        #region 查询用户
        public JsonArray GetUsers(UserRoles userRole, string keywords, DateTime regDateFrom, DateTime regDateTo, int page, int pageSize, out int total)
        {
            return dal.GetUsers(userRole, keywords, regDateFrom, regDateTo, page, pageSize, out total);
        }
        #endregion

        #region 根据编号获取配送地址
        public AddressObj GetAddress(int addressId)
        {
            return dal.GetAddress(addressId);
        }
        #endregion

        #region 添加配送地址
        public void AddAddress(AddressObj addressObj)
        {
            dal.AddAddress(addressObj);
        }
        #endregion

        #region 删除配送地址
        public void DeleteAddress(int userId, int addressId)
        {
            dal.DeleteAddress(userId, addressId);
        }
        #endregion

        #region 修改配送地址
        public void ModifyAddress(AddressObj addressObj)
        {
            dal.ModifyAddress(addressObj);
        }
        #endregion

        #region 加入收藏
        public void AddFavorite(int userId, int productId, int type)
        {
            dal.AddFavorite(userId, productId, type);
        }
        #endregion

        #region 获取收藏的商品
        public JsonArray GetFav(int userId, int page, int pageSize, out int total)
        {
            return dal.GetFav(userId, page, pageSize, out total);
        }
        #endregion

        public void DelFav(int userId, int id)
        {
            dal.DelFav(userId, id);
        }

        public UserObj GetUserByID(int userId)
        {
            return dal.GetUserByID(userId);
        }

        public void ModifyUser(UserObj userObj)
        {
            dal.ModifyUser(userObj);
        }

        public UserObj GetUserByUserName(string userName)
        {
            return dal.GetUserByUserName(userName);
        }

        public void DeleteUser(int id)
        {
            dal.DeleteUser(id);
        }

        public void AddPoints(int userID, int points)
        {
            dal.AddPoints(userID, points);
        }

    }
}
