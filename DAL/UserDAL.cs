using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gllo.Models;
using System.Data.SqlClient;
using System.Data;

namespace Gllo.DAL
{
    public class UserDAL
    {
        private SqlHelper helper;

        #region 用户登录
        public bool Login(UserObj user)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@UserName", 20, user.UserName);
                helper.AddStringParameter("@Password", 128, user.Password);
                SqlParameter returnValueParam = helper.AddReturnValueParameter(SqlDbType.Int);
                SqlDataReader dr;
                using (dr = helper.ExecuteReader("select UserID,SafeCode,UserRole from Users where UserName=@UserName and Password=@Password", CommandType.Text))
                {
                    if (dr.Read())
                    {
                        user.UserID = dr.GetInt32(0);
                        user.SafeCode = dr[1] == DBNull.Value ? null : (string)dr[1];
                        user.UserRole = (UserRoles)dr.GetInt16(2);
                    }
                    else
                        return false;
                }
                helper.ClearParameters();
                helper.AddIntParameter("@UserID", user.UserID);
                helper.AddDateTimeParameter("@LastLoginTime", DateTime.Now);

                helper.GetBooleanValue("update Users set LastLoginTime=@LastLoginTime where UserID=@UserID", CommandType.Text);
                return true;
            }
        }
        #endregion

        #region 判断用户名是否存在
        public bool IsRegist(string userName)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@UserName", 20, userName);

                return helper.GetBooleanValue("if exists (select 1 from Users where UserName=@UserName) select 1 else select 0", CommandType.Text);
            }
        }
        #endregion

        #region 用户注册
        public void Register(UserObj userObj)
        {
            using (helper = new SqlHelper())
            {
                SqlParameter userIdParam = helper.AddOutputParameter("@UserID");
                helper.AddStringParameter("@UserName", 20, userObj.UserName);
                helper.AddIntParameter("@UserRole", (int)userObj.UserRole);
                helper.AddStringParameter("@Password", 32, userObj.Password);
                helper.AddDateTimeParameter("@RegDate", DateTime.Now);
                helper.AddDateTimeParameter("@LastLoginTime", DateTime.Now);
                helper.AddStringParameter("@NickName", 20, userObj.NickName);
                helper.AddBoolenParameter("@Gender", userObj.Gender);
                helper.AddStringParameter("@Email", 100, userObj.Email);
                helper.AddDateTimeParameter("@BirthDay", userObj.BirthDay);

                string sql = "insert into Users (UserName,UserRole,Password,RegDate,LastLoginTime,NickName,Gender,Email,BirthDay) values (@UserName,@UserRole,@Password,@RegDate,@LastLoginTime,@NickName,@Gender,@Email,@BirthDay) select @UserID=@@IDENTITY";
                helper.ExecuteNonQuery(sql, CommandType.Text);
                userObj.UserID = (int)userIdParam.Value;
            }
        }
        #endregion

        #region 修改用户信息
        public void ModifyUser(UserObj userObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userObj.UserID);
                helper.AddStringParameter("@Password", 32, userObj.Password);
                helper.AddStringParameter("@NickName", 20, userObj.NickName);
                helper.AddBoolenParameter("@Gender", userObj.Gender);
                helper.AddDateTimeParameter("@BirthDay", userObj.BirthDay);

                string sql = "update Users set Password=@Password,NickName=@NickName,Gender=@Gender,BirthDay=@BirthDay where UserID=@UserID";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region 查询用户
        public JsonArray GetUsers(UserRoles userRole, string keywords, DateTime regDateFrom, DateTime regDateTo, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                string where = "UserRole=@UserRole";

                helper.AddIntParameter("@UserRole", (int)userRole);
                if (!string.IsNullOrEmpty(keywords))
                {
                    helper.AddStringParameter("@Keywords", 50, keywords);
                    where += " and (UserName like '%'+@Keywords+'%'or NickName like '%'+@Keywords+'%' or Email like '%'+@Keywords+'%') ";
                }

                if (regDateFrom != DateTime.MinValue && regDateTo != DateTime.MinValue)
                {
                    helper.AddDateTimeParameter("@RegDateFrom", regDateFrom.Date);
                    helper.AddDateTimeParameter("@RegDateTo", regDateTo.Date.AddDays(1).AddMilliseconds(-3));

                    where += " and RegDate between @RegDateFrom and @RegDateTo";
                }

                JsonArray result = helper.GetJsonArray("UserID",
                    "UserID,UserName,RegDate,LastLoginTime,NickName,Gender,Email,BirthDay,Points",
                    "Users",
                    where,
                    page,
                    pageSize,
                    out total,
                    "RegDate",
                    false);

                if (result != null)
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        helper.ClearParameters();
                        helper.AddIntParameter("@UserID", (int)result[i]["UserID"]);
                        var address = helper.GetJsonArray("select AddressID,Address,Zip,Receiver,Mobile,Phone,UserAddress.CityID,CityName,UserAddress.RegionID,RegionName,Province.ProvinceID,ProvinceName from UserAddress inner join City on UserAddress.CityID=City.CityID inner join Province on Province.ProvinceID=City.ProvinceID left join Region on Region.RegionID=UserAddress.RegionID where UserID=@UserID", CommandType.Text);
                        result[i].Add("Address", address);
                    }
                }
                return result;
            }
        }
        #endregion

        public UserObj GetUserByID(int userId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userId);
                using (SqlDataReader dr = helper.ExecuteReader("select UserID,UserName,RegDate,LastLoginTime,NickName,Gender,Email,BirthDay,Password,Points from Users where UserID=@UserID", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        UserObj user = new UserObj();
                        user.UserID = (int)dr[0];
                        user.UserName = dr[1] == DBNull.Value ? null : (string)dr[1];
                        user.RegDate = dr[2] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[2];
                        user.LastLoginTime = dr[3] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[3];
                        user.NickName = dr[4] == DBNull.Value ? null : (string)dr[4];
                        user.Gender = dr[5] == DBNull.Value ? false : (bool)dr[5];
                        user.Email = dr[6] == DBNull.Value ? null : (string)dr[6];
                        user.BirthDay = dr[7] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[7];
                        user.Password = dr[8] == DBNull.Value ? null : (string)dr[8];
                        user.Points = dr[9] == DBNull.Value ? 0 : (int)dr[9];

                        return user;
                    }

                    return null;
                }
            }
        }

        public UserObj GetUserByUserName(string userName)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@UserName", 50, userName);
                using (SqlDataReader dr = helper.ExecuteReader("select UserID,UserName,RegDate,LastLoginTime,NickName,Gender,Email,BirthDay,Password,Points from Users where UserName=@UserName", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        UserObj user = new UserObj();
                        user.UserID = (int)dr[0];
                        user.UserName = dr[1] == DBNull.Value ? null : (string)dr[1];
                        user.RegDate = dr[2] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[2];
                        user.LastLoginTime = dr[3] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[3];
                        user.NickName = dr[4] == DBNull.Value ? null : (string)dr[4];
                        user.Gender = dr[5] == DBNull.Value ? false : (bool)dr[5];
                        user.Email = dr[6] == DBNull.Value ? null : (string)dr[6];
                        user.BirthDay = dr[7] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[7];
                        user.Password = dr[8] == DBNull.Value ? null : (string)dr[8];
                        user.Points = dr[9] == DBNull.Value ? 0 : (int)dr[9];

                        return user;
                    }

                    return null;
                }
            }
        }

        public void AddPoints(int userID, int points)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userID);
                points += helper.GetIntValue("select Points from Users where UserID=@UserID", CommandType.Text);
                helper.AddIntParameter("@Points", points);
                helper.ExecuteNonQuery("update Users set Points=@Points where UserID=@UserID");
            }
        }

        #region 添加配送地址
        public void AddAddress(AddressObj addressObj)
        {
            using (helper = new SqlHelper())
            {
                SqlParameter addressIdParam = helper.AddOutputParameter("@AddressID");
                helper.AddIntParameter("@UserID", addressObj.UserID);
                helper.AddStringParameter("@Address", 200, addressObj.Address);
                helper.AddStringParameter("@Zip", 20, addressObj.Zip);
                helper.AddStringParameter("@Receiver", 20, addressObj.Receiver);
                helper.AddStringParameter("@Mobile", 20, addressObj.Mobile);
                helper.AddStringParameter("@Phone", 50, addressObj.Phone);
                helper.AddIntParameter("@CityID", addressObj.CityID);
                helper.AddIntParameter("@RegionID", addressObj.RegionID);

                string sql = "insert into UserAddress (UserID,Address,Zip,Receiver,Mobile,Phone,CityID,RegionID) values (@UserID,@Address,@Zip,@Receiver,@Mobile,@Phone,@CityID,@RegionID)";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region 删除配送地址
        public void DeleteAddress(int userId, int addressId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userId);
                helper.AddIntParameter("@AddressID", addressId);
                helper.ExecuteNonQuery("delete from UserAddress where AddressID=@AddressID and UserID=@UserID", CommandType.Text);
            }
        }
        #endregion

        #region 修改配送地址
        public void ModifyAddress(AddressObj addressObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@AddressID", addressObj.AddressID);
                helper.AddIntParameter("@UserID", addressObj.UserID);
                helper.AddStringParameter("@Address", 200, addressObj.Address);
                helper.AddStringParameter("@Zip", 20, addressObj.Zip);
                helper.AddStringParameter("@Receiver", 20, addressObj.Receiver);
                helper.AddStringParameter("@Mobile", 20, addressObj.Mobile);
                helper.AddStringParameter("@Phone", 50, addressObj.Phone);
                helper.AddIntParameter("@CityID", addressObj.CityID);
                helper.AddIntParameter("@RegionID", addressObj.RegionID);

                string sql = "update UserAddress set Address=@Address,Zip=@Zip,Receiver=@Receiver,Mobile=@Mobile,Phone=@Phone,CityID=@CityID,RegionID=@RegionID where AddressID=@AddressID";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region 获取配送地址
        public JsonArray GetUserAddress(int userId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userId);
                return helper.GetJsonArray("select AddressID,Address,Zip,Receiver,Mobile,Phone,UserAddress.CityID,CityName,UserAddress.RegionID,RegionName,Province.ProvinceID,ProvinceName from UserAddress inner join City on UserAddress.CityID=City.CityID inner join Province on Province.ProvinceID=City.ProvinceID left join Region on Region.RegionID=UserAddress.RegionID where UserID=@UserID", CommandType.Text);
            }
        }
        #endregion

        #region 根据编号获取配送地址
        public AddressObj GetAddress(int addressId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@AddressID", addressId);
                using (SqlDataReader dr = helper.ExecuteReader("select Address,Zip,Receiver,Mobile,Phone,UserAddress.CityID,UserAddress.RegionID,UserID,Province.ProvinceID,CityName,RegionName,ProvinceName from UserAddress inner join City on UserAddress.CityID=City.CityID inner join Province on Province.ProvinceID=City.ProvinceID left join Region on Region.RegionID=UserAddress.RegionID where AddressID=@AddressID", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        AddressObj addressObj = new AddressObj();
                        addressObj.AddressID = addressId;
                        addressObj.Address = dr[0] == DBNull.Value ? null : (string)dr[0];
                        addressObj.Zip = dr[1] == DBNull.Value ? null : (string)dr[1];
                        addressObj.Receiver = dr[2] == DBNull.Value ? null : (string)dr[2];
                        addressObj.Mobile = dr[3] == DBNull.Value ? null : (string)dr[3];
                        addressObj.Phone = dr[4] == DBNull.Value ? null : (string)dr[4];
                        addressObj.CityID = dr[5] == DBNull.Value ? 0 : (int)dr[5];
                        addressObj.RegionID = dr[6] == DBNull.Value ? 0 : (int)dr[6];
                        addressObj.UserID = dr[7] == DBNull.Value ? 0 : (int)dr[7];

                        return addressObj;
                    }
                    return null;
                }
            }
        }
        #endregion


        #region 加入收藏
        public void AddFavorite(int userId, int productId, int type)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userId);
                helper.AddIntParameter("@ProductID", productId);
                helper.AddIntParameter("@Type", type);

                helper.ExecuteNonQuery("if not exists (select 1 from Favorites where ProductID=@ProductID and UserID=@UserID and Type=@Type) insert into Favorites (ProductID,UserID,Type,AddTime) values (@ProductID,@UserID,@Type,GetDate())", CommandType.Text);
            }
        }
        #endregion

        #region 获取收藏的商品
        public JsonArray GetFav(int userId, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userId);

                return helper.GetJsonArray("FavoriteID",
                    "FavoriteID,a.ProductID,Name,Code,Material,Price,SpecialPrice,CategoryName,PictureDesc,PictureUrl,SellNum,Type",
                    "(select FavoriteID,a.ProductID,Name,Code,Material,Price,SpecialPrice,CategoryName,PictureDesc,Url as PictureUrl,SellNum,UserID,0 as Type from Favorites a inner join Products b on a.ProductID=b.ProductID inner join ProductCates c on b.CategoryID=c.CategoryID left join (select PictureDesc,Url,d.ProductID as PID from ProductPictures b inner join (select ProductID,min(PictureID) as PictureID from ProductPictures where Type=0 group by ProductID) d on b.PictureID=d.PictureID) e on PID=a.ProductID where a.Type=0 or a.Type is null union select FavoriteID,a.ProductID,PackageName as Name,Code,Material,0,0,CategoryName,PictureDesc,Url as PictureUrl,SaleNum as SellNum,UserID,1 as Type from Favorites a inner join Package b on a.ProductID=b.PackageID left join (select PictureDesc,Url,a.PackageID as PID from PackagePictures b inner join (select PackageID,min(PictureID) as PictureID from PackagePictures group by PackageID) a on a.PictureID=b.PictureID) c on PID=b.PackageID left join PackageCates d on b.CategoryID=d.CategoryID where a.Type=1) a",
                    "UserID=@UserID",
                    page,
                    pageSize,
                    out total,
                    "FavoriteID",
                    false);
            }
        }
        #endregion

        #region 删除收藏的商品
        public void DelFav(int userId, int id)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userId);
                helper.AddIntParameter("@FavoriteID", id);

                helper.ExecuteNonQuery("delete from Favorites where UserID=@UserID and FavoriteID=@FavoriteID", CommandType.Text);
            }
        }
        #endregion


        public void DeleteUser(int id)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", id);
                helper.ExecuteNonQuery("delete from Users where UserID=@UserID", CommandType.Text);
            }
        }
    }
}
