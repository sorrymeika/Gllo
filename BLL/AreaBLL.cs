using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;

namespace Gllo.BLL
{
    public class AreaBLL
    {

        private readonly Gllo.DAL.AreaDAL dal = new DAL.AreaDAL();

        #region 添加省
        public void AddProvince(ProvinceObj provinceObj)
        {
            dal.AddProvince(provinceObj);
        }
        #endregion

        #region 修改省
        public void ModifyProvince(ProvinceObj provinceObj)
        {
            dal.ModifyProvince(provinceObj);
        }
        #endregion

        #region 删除省
        public void DeleteProvince(int provinceId)
        {
            dal.DeleteProvince(provinceId);
        }
        #endregion

        #region 根据省编号获取省
        public ProvinceObj GetProvince(int provinceId)
        {
            return dal.GetProvince(provinceId);
        }
        #endregion

        #region 获取全部省
        public JsonArray GetProvince()
        {
            return dal.GetProvince();
        }
        #endregion

        #region 添加市
        public void AddCity(CityObj cityObj)
        {
            dal.AddCity(cityObj);
        }
        #endregion

        #region 修改市
        public void ModifyCity(CityObj cityObj)
        {
            dal.ModifyCity(cityObj);
        }
        #endregion

        #region 删除市
        public void DeleteCity(int cityId)
        {
            dal.DeleteCity(cityId);
        }
        #endregion

        #region 根据市编号获取市
        public CityObj GetCity(int cityId)
        {
            return dal.GetCity(cityId);
        }
        #endregion

        #region 获取全部市
        public JsonArray GetCityByProvId(int provinceId)
        {
            return dal.GetCityByProvId(provinceId);
        }
        #endregion

        #region 添加区
        public void AddRegion(RegionObj regionObj)
        {
             dal.AddRegion(regionObj);
        }
        #endregion

        #region 修改区
        public void ModifyRegion(RegionObj regionObj)
        {
            dal.ModifyRegion(regionObj);
        }
        #endregion

        #region 删除区
        public void DeleteRegion(int regionId)
        {
            dal.DeleteRegion(regionId);
        }
        #endregion

        #region 根据区编号获取区
        public RegionObj GetRegion(int regionId)
        {
            return dal.GetRegion(regionId);
        }
        #endregion

        #region 获取全部区
        public JsonArray GetRegionByCityId(int cityId)
        {
            return dal.GetRegionByCityId(cityId);
        }
        #endregion

        #region 根据区编号获取区
        public JsonObject GetAreaByRetionID(int regionId)
        {
            return dal.GetAreaByRetionID(regionId);
        }
        #endregion

        public JsonArray GetCities()
        {

            return dal.GetCities();
        }

        public JsonArray GetRegions()
        {
            return dal.GetRegions();
        }
    }
}
