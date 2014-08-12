using System;
using System.Collections.Generic;
using System.Text;
using Gllo.DAL;
using Gllo.Models;

namespace Gllo.BLL
{
    public class PackageBLL
    {
        private PackageDAL dal = new PackageDAL();

        #region AddPackageCate
        public void AddPackageCate(PackageCateObj packageCateObj)
        {
            dal.AddPackageCate(packageCateObj);
        }
        #endregion

        #region DeletePackageCate
        public void DeletePackageCate(int categoryID)
        {
            dal.DeletePackageCate(categoryID);

        }
        #endregion

        #region ModifyPackageCate
        public void ModifyPackageCate(PackageCateObj packageCateObj)
        {
            dal.ModifyPackageCate(packageCateObj);
        }
        #endregion

        #region GetPackageCate
        public PackageCateObj GetPackageCate(int categoryID)
        {
            return dal.GetPackageCate(categoryID);
        }
        #endregion

        #region GetPackageCates
        public JsonArray GetPackageCates()
        {
            return dal.GetPackageCates();
        }
        #endregion

        #region GetPackageCatesByParentID
        public JsonArray GetPackageCatesByParentID(int parentId)
        {
            return dal.GetPackageCatesByParentID(parentId);
        }
        #endregion

        #region IsPackageExists
        public bool IsPackageExists(string name)
        {
            return dal.IsPackageExists(name);
        }
        #endregion

        #region AddPackage
        public void AddPackage(PackageObj packageObj)
        {
            dal.AddPackage(packageObj);
        }
        #endregion

        #region ModifyPackage
        public void ModifyPackage(PackageObj packageObj)
        {
            dal.ModifyPackage(packageObj);
        }
        #endregion

        #region DeletePackage
        public void DeletePackage(int packageID)
        {
            dal.DeletePackage(packageID);
        }
        #endregion

        #region GetPackageByID
        public PackageObj GetPackageByID(int packageID)
        {
            return dal.GetPackageByID(packageID);
        }
        #endregion

        #region GetPackages
        public JsonArray GetPackages(IList<int> categoryIDs, string keywords, int buildingID, int regionID, int cityID, int provinceID, int page, int pageSize, out int total)
        {
            return dal.GetPackages(categoryIDs, keywords, buildingID, regionID, cityID, provinceID, page, pageSize, out  total);
        }
        #endregion

        #region AddPicture
        public void AddPicture(PackagePictureObj packagePictureObj)
        {
            dal.AddPicture(packagePictureObj);
        }
        #endregion

        #region DeletePicture
        public int DeletePicture(int pictureID)
        {
            return dal.DeletePicture(pictureID);
        }
        #endregion

        #region ModifyPicture
        public void ModifyPicture(PackagePictureObj packagePictureObj)
        {
            dal.ModifyPicture(packagePictureObj);
        }
        #endregion

        #region GetPictureByID
        public PackagePictureObj GetPictureByID(int pictureID)
        {
            return dal.GetPictureByID(pictureID);
        }
        #endregion

        #region AddBuilding
        public void AddBuilding(PackageBuildingObj packageBuildingObj)
        {
            dal.AddBuilding(packageBuildingObj);
        }
        #endregion

        #region ModifyBuilding
        public void ModifyBuilding(PackageBuildingObj packageBuildingObj)
        {
            dal.ModifyBuilding(packageBuildingObj);
        }
        #endregion

        #region DeleteBuilding
        public void DeleteBuilding(int buildingID)
        {
            dal.DeleteBuilding(buildingID);
        }
        #endregion

        #region GetBuildings
        public JsonArray GetBuildings(string keywords, int regionID, int cityID, int provinceID, int page, int pageSize, out int total)
        {
            return dal.GetBuildings(keywords, regionID, cityID, provinceID, page, pageSize, out  total);

        }
        #endregion

        #region GetBuildings
        public JsonArray GetBuildings(int regionID)
        {
            return dal.GetBuildings(regionID);
        }
        #endregion

        #region GetBuildingByID
        public PackageBuildingObj GetBuildingByID(int buildingID)
        {
            return dal.GetBuildingByID(buildingID);
        }
        #endregion

        #region GetJsonBuildingByID
        public JsonObject GetJsonBuildingByID(int buildingID)
        {
            return dal.GetJsonBuildingByID(buildingID);
        }
        #endregion

        #region AddPackageProdCate
        public void AddPackageProdCate(PackageProdCatesObj packageProdCatesObj)
        {
            dal.AddPackageProdCate(packageProdCatesObj);
        }
        #endregion

        #region ModifyPackageProdCate
        public void ModifyPackageProdCate(PackageProdCatesObj packageProdCatesObj)
        {
            dal.ModifyPackageProdCate(packageProdCatesObj);
        }
        #endregion

        #region GetPackageDetails
        public JsonArray GetPackageDetails(int packageID)
        {
            return dal.GetPackageDetails(packageID);

        }
        #endregion

        #region GetPackageDetailByID
        public PackageProdCatesObj GetPackageDetailByID(int ppcID)
        {
            return dal.GetPackageDetailByID(ppcID);
        }
        #endregion

        #region GetPackageDetailByID
        public JsonObject GetJsonPackageDetailByID(int ppcID)
        {
            return dal.GetJsonPackageDetailByID(ppcID);
        }
        #endregion

        #region DeletePackageDetails
        public void DeletePackageDetails(int ppcID)
        {
            dal.DeletePackageDetails(ppcID);
        }
        #endregion

        #region GetProductIDByPpID
        public int GetProductIDByPpID(int ppID)
        {
            return dal.GetProductIDByPpID(ppID);
        }
        #endregion

        #region GetJsonPackageByIDForCart
        public JsonObject GetJsonPackageByIDForCart(int packageID)
        {
            return dal.GetJsonPackageByIDForCart(packageID);
        }
        #endregion
    }
}
