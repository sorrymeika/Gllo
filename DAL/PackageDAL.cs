using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;
using System.Data;
using System.Data.SqlClient;

namespace Gllo.DAL
{
    public class PackageDAL
    {
        SqlHelper helper;

        #region AddPackageCate
        public void AddPackageCate(PackageCateObj packageCateObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@CategoryName", 50, packageCateObj.CategoryName);
                helper.AddIntParameter("@ParentID", packageCateObj.ParentID);
                helper.AddIntParameter("@Sort", packageCateObj.Sort);

                packageCateObj.CategoryID = helper.GetIntValue("select Max(CategoryID) from PackageCates", CommandType.Text) + 1;
                helper.AddIntParameter("@CategoryID", packageCateObj.CategoryID);

                helper.ExecuteNonQuery("insert into PackageCates (CategoryID,CategoryName,ParentID,Sort) values (@CategoryID,@CategoryName,@ParentID,@Sort)", CommandType.Text);
            }
        }
        #endregion

        #region DeletePackageCate
        public void DeletePackageCate(int categoryID)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@CategoryID", categoryID);
                helper.ExecuteNonQuery("delete from LPackageCate where CategoryID=@CategoryID or CategoryID in (select CategoryID from PackageCates where ParentID=@CategoryID)", CommandType.Text);

                helper.ExecuteNonQuery("delete from LPhotoCate where CategoryID=@CategoryID or CategoryID in (select CategoryID from PackageCates where ParentID=@CategoryID)", CommandType.Text);

                helper.ExecuteNonQuery("delete from PackageCates where CategoryID=@CategoryID or ParentID=@CategoryID", CommandType.Text);

                helper.CommitTran();
            }
        }
        #endregion

        #region ModifyPackageCate
        public void ModifyPackageCate(PackageCateObj packageCateObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CategoryID", packageCateObj.CategoryID);
                helper.AddStringParameter("@CategoryName", 50, packageCateObj.CategoryName);
                helper.AddIntParameter("@ParentID", packageCateObj.ParentID);
                helper.AddIntParameter("@Sort", packageCateObj.Sort);

                helper.ExecuteNonQuery("update PackageCates set CategoryName=@CategoryName,ParentID=@ParentID,Sort=@Sort where CategoryID=@CategoryID", CommandType.Text);
            }
        }
        #endregion

        #region GetPackageCate
        public PackageCateObj GetPackageCate(int categoryID)
        {
            using (SqlHelper helper = new SqlHelper())
            {
                helper.AddIntParameter("@CategoryID", categoryID);
                using (SqlDataReader dr = helper.ExecuteReader("select CategoryName,ParentID,Sort from PackageCates where CategoryID=@CategoryID", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        PackageCateObj cateObj = new PackageCateObj();
                        cateObj.CategoryID = categoryID;
                        cateObj.CategoryName = (string)dr[0];
                        cateObj.ParentID = (int)dr[1];
                        cateObj.Sort = dr[2] == DBNull.Value ? 0 : (int)dr[2];

                        return cateObj;
                    }
                    else
                        return null;
                }
            }
        }
        #endregion

        #region 获取全部分类
        public JsonArray GetPackageCates()
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ParentID", 0);
                return GetPackageCates(0);
            }
        }

        public JsonArray GetPackageCatesByParentID(int parentID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ParentID", 0);
                return GetPackageCates(parentID, false);
            }
        }

        private JsonArray GetPackageCates(int parentID, bool deep = true)
        {
            helper.SetParameter("@ParentID", parentID);
            string sql = "select CategoryID,CategoryName,ParentID,Sort from PackageCates where ParentID=@ParentID order by Sort desc,CategoryID";
            JsonArray cateList;
            JsonObject cateObj;
            using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
            {
                if (dr.HasRows)
                {
                    cateList = new JsonArray();
                    while (dr.Read())
                    {
                        cateObj = new JsonObject();
                        cateObj.Add("categoryID", (int)dr[0]);
                        cateObj.Add("categoryName", (string)dr[1]);
                        cateObj.Add("parentID", dr[2] == DBNull.Value ? 0 : (int)dr[2]);
                        cateObj.Add("sort", dr[3] == DBNull.Value ? 0 : (int)dr[3]);
                        cateList.Add(cateObj);
                    }
                }
                else
                    cateList = null;
            }
            if (cateList != null && deep)
            {
                JsonArray newsCateChildren;
                for (int i = 0; i < cateList.Count; i++)
                {
                    cateObj = cateList[i];
                    parentID = (int)cateObj["categoryID"];
                    newsCateChildren = GetPackageCates(parentID);
                    if (newsCateChildren != null)
                    {
                        cateObj.Add("children", newsCateChildren);
                    }
                }
            }
            return cateList;
        }
        #endregion


        #region IsPackageExists
        public bool IsPackageExists(string name)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@PackageName", 500, name);

                return helper.GetBooleanValue("if exists (select 1 from Package where PackageName=@PackageName) select 1 else select 0", CommandType.Text);
            }
        }
        #endregion

        #region AddPackage
        public void AddPackage(PackageObj packageObj)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                var id = helper.AddOutputParameter("@PackageID");

                helper.AddIntParameter("@CategoryID", packageObj.CategoryID);
                helper.AddStringParameter("@PackageName", 500, packageObj.PackageName);
                helper.AddStringParameter("@Code", 100, packageObj.Code);
                helper.AddIntParameter("@SaleNum", packageObj.SaleNum);
                helper.AddTextParameter("@Memo", packageObj.Memo);
                helper.AddTextParameter("@Intro", packageObj.Intro);
                helper.AddStringParameter("@Material", 200, packageObj.Material);
                helper.AddIntParameter("@BuildingID", packageObj.BuildingID);

                helper.ExecuteNonQuery("insert into Package (CategoryID,PackageName,Code,SaleNum,Memo,Intro,Material,BuildingID) values (@CategoryID,@PackageName,@Code,@SaleNum,@Memo,@Intro,@Material,@BuildingID) select @PackageID=@@IDENTITY", CommandType.Text);

                packageObj.PackageID = (int)id.Value;

                if (packageObj.Pictures != null)
                {
                    PackagePictureObj productPicture;
                    for (int i = 0; i < packageObj.Pictures.Count; i++)
                    {
                        helper.ClearParameters();
                        productPicture = packageObj.Pictures[i];
                        helper.AddIntParameter("@PictureID", productPicture.PictureID);
                        helper.AddIntParameter("@PackageID", packageObj.PackageID);

                        helper.ExecuteNonQuery("update PackagePictures set PackageID=@PackageID where PictureID=@PictureID", CommandType.Text);
                    }
                }

                for (int i = 0; i < packageObj.Categories.Count; i++)
                {
                    helper.ClearParameters();
                    helper.AddIntParameter("@PackageID", packageObj.PackageID);
                    helper.AddIntParameter("@CategoryID", packageObj.Categories[i]);
                    helper.ExecuteNonQuery("insert into LPackageCate (PackageID,CategoryID) values (@PackageID,@CategoryID)", CommandType.Text);
                }

                helper.CommitTran();
            }
        }
        #endregion

        #region ModifyPackage
        public void ModifyPackage(PackageObj packageObj)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@PackageID", packageObj.PackageID);
                helper.AddIntParameter("@CategoryID", packageObj.CategoryID);
                helper.AddStringParameter("@PackageName", 500, packageObj.PackageName);
                helper.AddStringParameter("@Code", 100, packageObj.Code);
                helper.AddIntParameter("@SaleNum", packageObj.SaleNum);
                helper.AddTextParameter("@Memo", packageObj.Memo);
                helper.AddTextParameter("@Intro", packageObj.Intro);
                helper.AddStringParameter("@Material", 200, packageObj.Material);
                helper.AddIntParameter("@BuildingID", packageObj.BuildingID);

                helper.ExecuteNonQuery("update Package set CategoryID=@CategoryID,PackageName=@PackageName,Code=@Code,SaleNum=@SaleNum,Memo=@Memo,Intro=@Intro,Material=@Material,BuildingID=@BuildingID where PackageID=@PackageID", CommandType.Text);

                if (packageObj.Pictures != null)
                {
                    PackagePictureObj picture;
                    for (int i = 0; i < packageObj.Pictures.Count; i++)
                    {
                        helper.ClearParameters();
                        picture = packageObj.Pictures[i];
                        helper.AddIntParameter("@PictureID", picture.PictureID);
                        helper.AddIntParameter("@PackageID", packageObj.PackageID);

                        helper.ExecuteNonQuery("update PackagePictures set PackageID=@PackageID where PictureID=@PictureID", CommandType.Text);
                    }
                }

                helper.ExecuteNonQuery("delete from LPackageCate where PackageID=@PackageID", CommandType.Text);

                for (int i = 0; i < packageObj.Categories.Count; i++)
                {
                    helper.ClearParameters();
                    helper.AddIntParameter("@PackageID", packageObj.PackageID);
                    helper.AddIntParameter("@CategoryID", packageObj.Categories[i]);
                    helper.ExecuteNonQuery("insert into LPackageCate (PackageID,CategoryID) values (@PackageID,@CategoryID)", CommandType.Text);
                }

                helper.CommitTran();
            }
        }
        #endregion

        #region DeletePackage
        public void DeletePackage(int packageID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@PackageID", packageID);

                helper.ExecuteNonQuery("delete from Package where PackageID=@PackageID", CommandType.Text);

                helper.ExecuteNonQuery("delete from LPackageCate where PackageID=@PackageID", CommandType.Text);

                helper.ExecuteNonQuery("delete from PackagePictures where PackageID=@PackageID", CommandType.Text);

            }
        }
        #endregion

        #region GetPackageByID
        public PackageObj GetPackageByID(int packageID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@PackageID", packageID);
                PackageObj packageObj;
                SqlDataReader dr;
                using (dr = helper.ExecuteReader("select CategoryID,PackageName,Code,SaleNum,Memo,Intro,Material,a.BuildingID,PackageBuilding.RegionID,Region.CityID,City.ProvinceID from Package a left join PackageBuilding on a.BuildingID=PackageBuilding.BuildingID left join Region on PackageBuilding.RegionID=Region.RegionID left join City on City.CityID=Region.CityID left join Province on City.ProvinceID=Province.ProvinceID where PackageID=@PackageID", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        packageObj = new PackageObj();
                        packageObj.PackageID = packageID;
                        packageObj.CategoryID = dr[0] == DBNull.Value ? 0 : (int)dr[0];
                        packageObj.PackageName = dr[1] == DBNull.Value ? null : (string)dr[1];
                        packageObj.Code = dr[2] == DBNull.Value ? null : (string)dr[2];
                        packageObj.SaleNum = dr[3] == DBNull.Value ? 0 : (int)dr[3];
                        packageObj.Memo = dr[4] == DBNull.Value ? null : (string)dr[4];
                        packageObj.Intro = dr[5] == DBNull.Value ? null : (string)dr[5];
                        packageObj.Material = dr[6] == DBNull.Value ? null : (string)dr[6];
                        packageObj.BuildingID = dr[7] == DBNull.Value ? 0 : (int)dr[7];
                        packageObj.RegionID = dr[8] == DBNull.Value ? 0 : (int)dr[8];
                        packageObj.CityID = dr[9] == DBNull.Value ? 0 : (int)dr[9];
                        packageObj.ProvinceID = dr[10] == DBNull.Value ? 0 : (int)dr[10];

                    }
                    else
                        packageObj = null;
                }

                if (packageObj != null)
                {
                    using (dr = helper.ExecuteReader("select PictureID,PackageID,SavePath,Url,PictureDesc from PackagePictures where PackageID=@PackageID", CommandType.Text))
                    {
                        if (dr.HasRows)
                        {
                            PackagePictureObj picture;
                            packageObj.Pictures = new List<PackagePictureObj>();
                            while (dr.Read())
                            {
                                picture = new PackagePictureObj();
                                picture.PictureID = (int)dr[0];
                                picture.PackageID = (int)dr[1];
                                picture.SavePath = dr[2] == DBNull.Value ? null : (string)dr[2];
                                picture.Url = dr[3] == DBNull.Value ? null : (string)dr[3];
                                picture.PictureDesc = dr[4] == DBNull.Value ? null : (string)dr[4];

                                packageObj.Pictures.Add(picture);
                            }
                        }
                    }
                }

                using (dr = helper.ExecuteReader("select CategoryID from LPackageCate where PackageID=@PackageID", CommandType.Text))
                {
                    if (dr.HasRows)
                    {
                        packageObj.Categories = new List<int>();
                        while (dr.Read())
                        {
                            packageObj.Categories.Add((int)dr[0]);
                        }
                    }
                }
                return packageObj;
            }
        }
        #endregion

        #region GetPackages
        public JsonArray GetPackages(IList<int> categoryIDs, string keywords, int buildingID, int regionID, int cityID, int provinceID, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                string columns = "PackageID,PackageName,Code,SaleNum,Memo,Intro,Material,a.BuildingID,BuildingName,PackageBuilding.RegionID,RegionName,Region.CityID,CityName,City.ProvinceID,ProvinceName,PictureDesc,Url";
                StringBuilder where = new StringBuilder("1=1");
                StringBuilder table = new StringBuilder("Package a left join PackageBuilding on a.BuildingID=PackageBuilding.BuildingID left join Region on PackageBuilding.RegionID=Region.RegionID left join City on City.CityID=Region.CityID left join Province on City.ProvinceID=Province.ProvinceID left join (select PictureDesc,Url,a.PackageID as PID from PackagePictures b inner join (select PackageID,min(PictureID) as PictureID from PackagePictures group by PackageID) a on a.PictureID=b.PictureID) c on PID=a.PackageID");
                if (categoryIDs != null && categoryIDs.Count != 0 || (categoryIDs.Count == 1 && categoryIDs[0] == 0))
                {
                    helper.AddIntParameter("@ParentID", 0);
                    table.Append(" inner join (select PackageID as LPID from LPackageCate where 1=1");
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
                    table.Append(" group by PackageID) b on LPID=a.PackageID");
                    helper.ClearParameters();
                }

                if (!string.IsNullOrEmpty(keywords))
                {
                    helper.AddStringParameter("@Keywords", 200, keywords);
                    where.Append(" and (PackageName like '%'+@Keywords+'%' or Code like '%'+@Keywords+'%')");
                }

                if (buildingID != 0)
                {
                    helper.AddIntParameter("@BuildingID", buildingID);
                    where.Append(" and a.BuildingID=@BuildingID");
                }
                else if (regionID != 0)
                {
                    helper.AddIntParameter("@RegionID", regionID);
                    where.Append(" and PackageBuilding.RegionID=@RegionID");
                }
                else if (cityID != 0)
                {
                    helper.AddIntParameter("@CityID", cityID);
                    where.Append(" and Region.CityID=@CityID");
                }
                else if (provinceID != 0)
                {
                    helper.AddIntParameter("@ProvinceID", provinceID);
                    where.Append(" and City.ProvinceID=@ProvinceID");
                }

                return helper.GetJsonArray("PackageID",
                    columns,
                    table.ToString(),
                    where.ToString(),
                    page,
                    pageSize,
                    out total,
                    "PackageID",
                    false);
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

        #region AddPicture
        public void AddPicture(PackagePictureObj packagePictureObj)
        {
            using (helper = new SqlHelper())
            {
                string sql = "insert into PackagePictures (PackageID,SavePath,Url,PictureDesc) values (@PackageID,@SavePath,@Url,@PictureDesc) select @PictureID=@@IDENTITY";
                SqlParameter picIDParam = helper.AddOutputParameter("@PictureID");
                helper.AddIntParameter("@PackageID", packagePictureObj.PackageID);
                helper.AddStringParameter("@SavePath", 200, packagePictureObj.SavePath);
                helper.AddStringParameter("@PictureDesc", 200, packagePictureObj.PictureDesc);
                helper.AddStringParameter("@Url", 200, packagePictureObj.Url);

                helper.ExecuteNonQuery(sql, CommandType.Text);
                packagePictureObj.PictureID = (int)picIDParam.Value;
            }
        }
        #endregion

        #region DeletePicture
        public int DeletePicture(int pictureID)
        {
            using (helper = new SqlHelper())
            {
                string sql = "delete from PackagePictures where PictureID=@PictureID";
                helper.AddIntParameter("@PictureID", pictureID);

                return helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region ModifyPicture
        public void ModifyPicture(PackagePictureObj packagePictureObj)
        {
            using (helper = new SqlHelper())
            {
                string sql = "update PackagePictures set PackageID=@PackageID,PictureDesc=@PictureDesc,SavePath=@SavePath,Url=@Url where PictureID=@PictureID";
                helper.AddIntParameter("@PictureID", packagePictureObj.PictureID);
                helper.AddIntParameter("@PackageID", packagePictureObj.PackageID);
                helper.AddStringParameter("@SavePath", 200, packagePictureObj.SavePath);
                helper.AddStringParameter("@PictureDesc", 200, packagePictureObj.PictureDesc);
                helper.AddStringParameter("@Url", 200, packagePictureObj.Url);

                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region GetPictureByID
        public PackagePictureObj GetPictureByID(int pictureID)
        {
            using (helper = new SqlHelper())
            {

                helper.AddIntParameter("@PictureID", pictureID);

                string sql = "select PictureID,PackageID,SavePath,Url,PictureDesc from PackagePictures where PictureID=@PictureID";
                using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        PackagePictureObj picture = new PackagePictureObj();
                        picture.PictureID = (int)dr[0];
                        picture.PackageID = (int)dr[1];
                        picture.SavePath = dr[2] == DBNull.Value ? null : (string)dr[2];
                        picture.Url = dr[3] == DBNull.Value ? null : (string)dr[3];
                        picture.PictureDesc = dr[4] == DBNull.Value ? null : (string)dr[4];
                        return picture;
                    }
                    return null;
                }
            }
        }
        #endregion


        #region AddBuilding
        public void AddBuilding(PackageBuildingObj packageBuildingObj)
        {
            using (helper = new SqlHelper())
            {
                string sql = "insert into PackageBuilding (BuildingName,RegionID) values (@BuildingName,@RegionID) select @BuildingID=@@IDENTITY";
                SqlParameter picIDParam = helper.AddOutputParameter("@BuildingID");
                helper.AddIntParameter("@RegionID", packageBuildingObj.RegionID);
                helper.AddStringParameter("@BuildingName", 200, packageBuildingObj.BuildingName);

                helper.ExecuteNonQuery(sql, CommandType.Text);
                packageBuildingObj.BuildingID = (int)picIDParam.Value;
            }
        }
        #endregion

        #region ModifyBuilding
        public void ModifyBuilding(PackageBuildingObj packageBuildingObj)
        {
            using (helper = new SqlHelper())
            {
                string sql = "update PackageBuilding set BuildingName=@BuildingName,RegionID=@RegionID where BuildingID=@BuildingID";
                helper.AddIntParameter("@BuildingID", packageBuildingObj.BuildingID);
                helper.AddIntParameter("@RegionID", packageBuildingObj.RegionID);
                helper.AddStringParameter("@BuildingName", 200, packageBuildingObj.BuildingName);

                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region DeleteBuilding
        public void DeleteBuilding(int buildingID)
        {
            using (helper = new SqlHelper())
            {
                string sql = "delete from PackageBuilding where BuildingID=@BuildingID";
                helper.AddIntParameter("@BuildingID", buildingID);

                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region GetBuildings
        public JsonArray GetBuildings(string keywords, int regionID, int cityID, int provinceID, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                StringBuilder where = new StringBuilder("1=1");
                if (!string.IsNullOrEmpty(keywords))
                {
                    helper.AddStringParameter("@Keywords", 200, keywords);
                    where.Append(" and BuildingName like '%'+@Keywords+'%'");
                }

                if (regionID != 0)
                {
                    helper.AddIntParameter("@RegionID", regionID);
                    where.Append(" and PackageBuilding.RegionID=@RegionID");
                }
                else if (cityID != 0)
                {
                    helper.AddIntParameter("@CityID", cityID);
                    where.Append(" and Region.CityID=@CityID");
                }
                else if (provinceID != 0)
                {
                    helper.AddIntParameter("@ProvinceID", provinceID);
                    where.Append(" and City.ProvinceID=@ProvinceID");
                }

                return helper.GetJsonArray("BuildingID",
                    "BuildingID,BuildingName,PackageBuilding.RegionID,RegionName,Region.CityID,CityName,City.ProvinceID,ProvinceName",
                    "PackageBuilding left join Region on PackageBuilding.RegionID=Region.RegionID left join City on City.CityID=Region.CityID left join Province on City.ProvinceID=Province.ProvinceID",
                    where.ToString(),
                    page,
                    pageSize,
                    out total,
                    "BuildingID",
                    false);
            }
        }
        #endregion

        #region GetBuildingByID
        public PackageBuildingObj GetBuildingByID(int buildingID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@BuildingID", buildingID);

                using (SqlDataReader dr = helper.ExecuteReader("select BuildingName,RegionID from PackageBuilding where BuildingID=@BuildingID", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        PackageBuildingObj res = new PackageBuildingObj();

                        res.BuildingID = buildingID;
                        res.BuildingName = dr[0] == DBNull.Value ? null : (string)dr[0];
                        res.RegionID = dr[1] == DBNull.Value ? 0 : (int)dr[1];

                        return res;
                    }

                    return null;
                }
            }
        }
        #endregion

        #region GetJsonBuildingByID
        public JsonObject GetJsonBuildingByID(int buildingID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@BuildingID", buildingID);

                JsonArray res = helper.GetJsonArray("select BuildingID,BuildingName,PackageBuilding.RegionID,RegionName,Region.CityID,CityName,City.ProvinceID,ProvinceName from PackageBuilding left join Region on PackageBuilding.RegionID=Region.RegionID left join City on City.CityID=Region.CityID left join Province on City.ProvinceID=Province.ProvinceID where BuildingID=@BuildingID", CommandType.Text);

                if (res != null && res.Count != 0)
                    return res[0];

                return null;
            }
        }
        #endregion

        #region GetBuildings
        public JsonArray GetBuildings(int regionID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@RegionID", regionID);

                return helper.GetJsonArray("select BuildingID,BuildingName,PackageBuilding.RegionID,RegionName,Region.CityID,CityName,City.ProvinceID,ProvinceName from PackageBuilding left join Region on PackageBuilding.RegionID=Region.RegionID left join City on City.CityID=Region.CityID left join Province on City.ProvinceID=Province.ProvinceID where PackageBuilding.RegionID=@RegionID", CommandType.Text);
            }
        }
        #endregion


        #region AddPackageProdCate
        public void AddPackageProdCate(PackageProdCatesObj packageProdCatesObj)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                SqlParameter idParam = helper.AddOutputParameter("@PpcID");
                helper.AddIntParameter("@PackageID", packageProdCatesObj.PackageID);
                helper.AddStringParameter("@PpcName", 20, packageProdCatesObj.PpcName);

                helper.ExecuteNonQuery("insert into PackageProdCates (PackageID,PpcName) values (@PackageID,@PpcName) select @PpcID=@@IDENTITY", CommandType.Text);
                packageProdCatesObj.PpcID = (int)idParam.Value;

                if (packageProdCatesObj.PackageProducts != null)
                {
                    foreach (var prdId in packageProdCatesObj.PackageProducts)
                    {
                        helper.ClearParameters();
                        helper.AddIntParameter("@PpcID", packageProdCatesObj.PpcID);
                        helper.AddIntParameter("@ProductID", prdId);
                        helper.ExecuteNonQuery("insert into PackageProducts (ProductID,PpcID) values (@ProductID,@PpcID)", CommandType.Text);
                    }
                }

                helper.CommitTran();
            }
        }
        #endregion

        #region ModifyPackageProdCate
        public void ModifyPackageProdCate(PackageProdCatesObj packageProdCatesObj)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@PpcID", packageProdCatesObj.PpcID);
                helper.AddStringParameter("@PpcName", 20, packageProdCatesObj.PpcName);
                helper.ExecuteNonQuery("update PackageProdCates set PpcName=@PpcName where PpcID=@PpcID", CommandType.Text);
                helper.ExecuteNonQuery("delete from PackageProducts where PpcID=@PpcID", CommandType.Text);

                if (packageProdCatesObj.PackageProducts != null)
                {
                    foreach (var productID in packageProdCatesObj.PackageProducts)
                    {
                        helper.ClearParameters();
                        helper.AddIntParameter("@PpcID", packageProdCatesObj.PpcID);
                        helper.AddIntParameter("@ProductID", productID);
                        helper.ExecuteNonQuery("insert into PackageProducts (ProductID,PpcID) values (@ProductID,@PpcID)", CommandType.Text);
                    }
                }

                helper.CommitTran();
            }
        }
        #endregion

        #region GetPackageDetails
        public JsonArray GetPackageDetails(int packageID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@PackageID", packageID);
                JsonArray res = helper.GetJsonArray("select PpcID,PpcName,PackageID from PackageProdCates where PackageID=@PackageID", CommandType.Text);

                if (res != null)
                {
                    JsonObject dict;
                    for (int i = 0; i < res.Count; i++)
                    {
                        dict = res[i];

                        helper.ClearParameters();
                        helper.AddIntParameter("@PpcID", (int)dict["PpcID"]);

                        dict.Add("children", helper.GetJsonArray("select PpID,PpcID,PackageProducts.ProductID,Products.CategoryID,Name,Type,Serial,Model,Code,Material,Weight,Description,Characteristic,Designer,Price,SpecialPrice,IsNew,CanPurchasedSeparately,CategoryName,Freight,Freight1,Quantity,Inventory,Tags,Points,Products.Sort,IsOnSale,IsRecommend,PictureDesc,Url,Status from PackageProducts inner join Products on PackageProducts.ProductID=Products.ProductID inner join ProductCates on Products.CategoryID=ProductCates.CategoryID left join (select PictureDesc,Url,a.ProductID as PID from ProductPictures b inner join (select ProductID,min(PictureID) as PictureID from ProductPictures where Type=0 group by ProductID) a on a.PictureID=b.PictureID) a on PID=Products.ProductID where Status=1 and PpcID=@PpcID", CommandType.Text));
                    }
                }

                return res;
            }
        }
        #endregion

        #region DeletePackageDetails
        public void DeletePackageDetails(int ppcID)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@PpcID", ppcID);
                helper.ExecuteNonQuery("delete from PackageProdCates where PpcID=@PpcID", CommandType.Text);
                helper.ExecuteNonQuery("delete from PackageProducts where PpcID=@PpcID", CommandType.Text);

                helper.CommitTran();
            }
        }
        #endregion

        #region GetPackageDetailByID
        public PackageProdCatesObj GetPackageDetailByID(int ppcID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@PpcID", ppcID);

                SqlDataReader dr;
                PackageProdCatesObj packageProdCatesObj;
                using (dr = helper.ExecuteReader("select PpcID,PpcName,PackageID from PackageProdCates where PpcID=@PpcID", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        packageProdCatesObj = new PackageProdCatesObj();
                        packageProdCatesObj.PpcID = dr[0] == DBNull.Value ? 0 : (int)dr[0];
                        packageProdCatesObj.PpcName = dr[1] == DBNull.Value ? null : (string)dr[1];
                        packageProdCatesObj.PackageID = dr[2] == DBNull.Value ? 0 : (int)dr[2];

                    }
                    else
                        packageProdCatesObj = null;

                }
                if (packageProdCatesObj != null)
                {
                    using (dr = helper.ExecuteReader("select ProductID from PackageProducts where PpcID=@PpcID", CommandType.Text))
                    {
                        if (dr.HasRows)
                        {
                            packageProdCatesObj.PackageProducts = new List<int>();
                            while (dr.Read())
                            {
                                packageProdCatesObj.PackageProducts.Add((int)dr[0]);
                            }
                        }
                    }
                }
                return packageProdCatesObj;

            }
        }
        #endregion

        #region GetPackageDetailByID
        public JsonObject GetJsonPackageDetailByID(int ppcID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@PpcID", ppcID);
                JsonArray res = helper.GetJsonArray("select PpcID,PpcName,PackageID from PackageProdCates where PpcID=@PpcID", CommandType.Text);

                if (res != null && res.Count != 0)
                {
                    JsonObject dict;
                    for (int i = 0; i < res.Count; i++)
                    {
                        dict = res[i];
                        dict.Add("children", helper.GetJsonArray("select PpID,PpcID,PackageProducts.ProductID,Products.CategoryID,Name,Type,Serial,Model,Code,Material,Weight,Description,Characteristic,Designer,Price,SpecialPrice,IsNew,CanPurchasedSeparately,CategoryName,Freight,Freight1,Quantity,Inventory,Tags,Points,Products.Sort,IsOnSale,IsRecommend,PictureDesc,Url,Status from PackageProducts inner join Products on PackageProducts.ProductID=Products.ProductID inner join ProductCates on Products.CategoryID=ProductCates.CategoryID left join (select PictureDesc,Url,a.ProductID as PID from ProductPictures b inner join (select ProductID,min(PictureID) as PictureID from ProductPictures where Type=0 group by ProductID) a on a.PictureID=b.PictureID) a on PID=Products.ProductID where Status=1 and PpcID=@PpcID", CommandType.Text));
                    }

                    return res[0];
                }
                else
                    return null;
            }
        }
        #endregion

        #region GetProductIDByPpID
        public int GetProductIDByPpID(int ppID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@PpID", ppID);
                return helper.GetIntValue("select ProductID from PackageProducts where PpID=@PpID", CommandType.Text);
            }
        }
        #endregion


        #region GetJsonPackageByIDForCart
        public JsonObject GetJsonPackageByIDForCart(int packageID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@PackageID", packageID);
                var res = helper.GetJsonArray("select PackageID,PackageName,Code,SaleNum,Memo,Intro,Material,a.BuildingID,BuildingName,PackageBuilding.RegionID,RegionName,Region.CityID,CityName,City.ProvinceID,ProvinceName,PictureDesc,Url from Package a left join PackageBuilding on a.BuildingID=PackageBuilding.BuildingID left join Region on PackageBuilding.RegionID=Region.RegionID left join City on City.CityID=Region.CityID left join Province on City.ProvinceID=Province.ProvinceID left join (select PictureDesc,Url,a.PackageID as PID from PackagePictures b inner join (select PackageID,min(PictureID) as PictureID from PackagePictures group by PackageID) a on a.PictureID=b.PictureID) c on PID=a.PackageID where PackageID=@PackageID", CommandType.Text);
                return res == null || res.Count == 0 ? null : res[0];
            }
        }
        #endregion
    }
}
