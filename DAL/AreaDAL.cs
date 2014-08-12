using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;
using System.Data;
using System.Data.SqlClient;

namespace Gllo.DAL
{
    public class AreaDAL
    {
        private SqlHelper helper;

        #region 添加省
        public void AddProvince(ProvinceObj provinceObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddOutputParameter("@ProvinceID");
                helper.AddStringParameter("@ProvinceName", 100, provinceObj.ProvinceName);
                string sql = "insert into Province (ProvinceName) values (@ProvinceName) select @ProvinceID=@IDENTITY";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region 修改省
        public void ModifyProvince(ProvinceObj provinceObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ProvinceID", provinceObj.ProvinceID);
                helper.AddStringParameter("@ProvinceName", 100, provinceObj.ProvinceName);
                string sql = "update Province set ProvinceName=@ProvinceName where ProvinceID=@ProvinceID";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region 删除省
        public void DeleteProvince(int provinceId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ProvinceID", provinceId);
                string sql = "delete from Province where ProvinceID=@ProvinceID";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region 根据省编号获取省
        public ProvinceObj GetProvince(int provinceId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ProvinceID", provinceId);
                string sql = "select ProvinceID,ProvinceName from Province where ProvinceID=@ProvinceID";
                using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        ProvinceObj provinceObj = new ProvinceObj();
                        provinceObj.ProvinceID = (int)dr[0];
                        provinceObj.ProvinceName = (string)dr[1];
                        return provinceObj;
                    }
                    return null;
                }
            }
        }
        #endregion

        #region 添加市
        public void AddCity(CityObj cityObj)
        {
            using (helper = new SqlHelper())
            {
                SqlParameter idParam = helper.AddOutputParameter("@CityID");
                helper.AddStringParameter("@CityName", 100, cityObj.CityName);
                helper.AddIntParameter("@ProvinceID", cityObj.ProvinceID);

                helper.ExecuteNonQuery("insert into City (CityName,ProvinceID) values (@CityName,@ProvinceID) select @CityID=@IDENTITY", CommandType.Text);

                cityObj.CityID = (int)idParam.Value;
            }
        }
        #endregion

        #region 修改市
        public void ModifyCity(CityObj cityObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CityID", cityObj.CityID);
                helper.AddIntParameter("@ProvinceID", cityObj.ProvinceID);
                helper.AddStringParameter("@CityName", 100, cityObj.CityName);
                string sql = "update City set CityName=@CityName,ProvinceID=@ProvinceID where CityID=@CityID";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region 删除市
        public void DeleteCity(int cityId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CityID", cityId);
                string sql = "delete from City where CityID=@CityID";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region 根据市编号获取市
        public CityObj GetCity(int cityId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CityID", cityId);
                string sql = "select ProvinceID,CityName from City where CityID=@CityID";
                using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        CityObj cityObj = new CityObj();
                        cityObj.CityID = cityId;
                        cityObj.ProvinceID = (int)dr[0];
                        cityObj.CityName = (string)dr[1];
                        return cityObj;
                    }
                    return null;
                }
            }
        }
        #endregion

        #region 获取全部市
        public JsonArray GetCityByProvId(int provinceId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ProvinceID", provinceId);
                return helper.GetJsonArray("select CityID,CityName,ProvinceID from City where ProvinceID=@ProvinceID", CommandType.Text);
            }
        }
        #endregion

        #region 添加区
        public void AddRegion(RegionObj regionObj)
        {
            using (helper = new SqlHelper())
            {
                SqlParameter idParam = helper.AddOutputParameter("@RegionID");
                helper.AddStringParameter("@RegionName", 100, regionObj.RegionName);
                helper.AddIntParameter("@CityID", regionObj.CityID);

                helper.ExecuteNonQuery("insert into Region (RegionName,CityID) values (@RegionName,@CityID) select @RegionID=@IDENTITY", CommandType.Text);

                regionObj.RegionID = (int)idParam.Value;
            }
        }
        #endregion

        #region 修改区
        public void ModifyRegion(RegionObj regionObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CityID", regionObj.CityID);
                helper.AddIntParameter("@RegionID", regionObj.RegionID);
                helper.AddStringParameter("@RegionName", 100, regionObj.RegionName);
                string sql = "update Region set RegionName=@RegionName,CityID=@CityID where RegionID=@RegionID";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region 删除区
        public void DeleteRegion(int regionId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@RegionID", regionId);
                helper.ExecuteNonQuery("delete from Region where RegionID=@RegionID", CommandType.Text);
            }
        }
        #endregion

        #region 根据区编号获取区
        public RegionObj GetRegion(int regionId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@RegionID", regionId);
                string sql = "select CityID,RegionName from Region where RegionID=@RegionID";
                using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        RegionObj regionObj = new RegionObj();
                        regionObj.RegionID = regionId;
                        regionObj.CityID = (int)dr[0];
                        regionObj.RegionName = (string)dr[1];
                        return regionObj;
                    }
                    return null;
                }
            }
        }
        #endregion

        #region 获取全部区
        public JsonArray GetRegionByCityId(int cityId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CityID", cityId);
                return helper.GetJsonArray("select RegionID,RegionName,CityID from Region where CityID=@CityID", CommandType.Text);
            }
        }
        #endregion

        #region 根据区编号获取区
        public JsonObject GetAreaByRetionID(int regionId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@RegionID", regionId);
                string sql = "select a.CityID,a.RegionName,CityName,b.ProvinceID,c.ProvinceName from Region a inner join City b on a.CityID=b.CityID inner join Province c on c.ProvinceID=b.ProvinceID where RegionID=@RegionID";
                using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        JsonObject result = new JsonObject();
                        result.Add("RegionID", regionId);
                        result.Add("CityID", (int)dr[0]);
                        result.Add("RegionName", (string)dr[1]);
                        result.Add("CityName", (string)dr[2]);
                        result.Add("ProvinceID", (int)dr[3]);
                        result.Add("ProvinceName", (string)dr[4]);
                        return result;
                    }
                    return null;
                }
            }
        }
        #endregion

        public JsonArray GetProvince()
        {
            using (helper = new SqlHelper())
            {
                return helper.GetJsonArray("select ProvinceID,ProvinceName from Province", CommandType.Text);
            }
        }

        public JsonArray GetCities()
        {
            using (helper = new SqlHelper())
            {
                return helper.GetJsonArray("select CityID,CityName,ProvinceID from City", CommandType.Text);
            }
        }

        public JsonArray GetRegions()
        {
            using (helper = new SqlHelper())
            {
                return helper.GetJsonArray("select RegionID,RegionName,CityID from Region", CommandType.Text);
            }
        }

        
    }
}
