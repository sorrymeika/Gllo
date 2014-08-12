using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data.SqlClient;
using System.Data;

using Gllo.Models;

namespace Gllo.DAL
{
    public class ProductDAL
    {
        SqlHelper helper;

        #region 添加商品分类
        public void AddProductCate(ProductCateObj productCateObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@CategoryName", 50, productCateObj.CategoryName);
                helper.AddIntParameter("@ParentID", productCateObj.ParentID);
                helper.AddIntParameter("@Sort", productCateObj.Sort);

                productCateObj.CategoryID = helper.GetIntValue("select Max(CategoryID) from ProductCates", CommandType.Text) + 1;
                helper.AddIntParameter("@CategoryID", productCateObj.CategoryID);

                helper.ExecuteNonQuery("insert into ProductCates (CategoryID,CategoryName,ParentID,Sort) values (@CategoryID,@CategoryName,@ParentID,@Sort)", CommandType.Text);
            }
        }
        #endregion

        #region 删除商品分类
        public void DeleteProductCate(int categoryID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CategoryID", categoryID);
                helper.ExecuteNonQuery("delete from ProductCates where CategoryID=@CategoryID", CommandType.Text);
            }
        }
        #endregion

        #region 修改商品分类
        public void ModifyProductCate(ProductCateObj productCateObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CategoryID", productCateObj.CategoryID);
                helper.AddStringParameter("@CategoryName", 50, productCateObj.CategoryName);
                helper.AddIntParameter("@ParentID", productCateObj.ParentID);
                helper.AddIntParameter("@Sort", productCateObj.Sort);

                helper.ExecuteNonQuery("update ProductCates set CategoryName=@CategoryName,ParentID=@ParentID,Sort=@Sort where CategoryID=@CategoryID", CommandType.Text);
            }
        }
        #endregion

        #region 根据商品分类编号获取分类信息
        public ProductCateObj GetProductCateByCateID(int categoryID)
        {
            using (SqlHelper helper = new SqlHelper())
            {
                helper.AddIntParameter("@CategoryID", categoryID);
                using (SqlDataReader dr = helper.ExecuteReader("select CategoryName,ParentID,Sort from ProductCates where CategoryID=@CategoryID", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        ProductCateObj productCateObj = new ProductCateObj();
                        productCateObj.CategoryID = categoryID;
                        productCateObj.CategoryName = (string)dr[0];
                        productCateObj.ParentID = (int)dr[1];
                        productCateObj.Sort = dr[2] == DBNull.Value ? 0 : (int)dr[2];

                        return productCateObj;
                    }
                    else
                        return null;
                }
            }
        }
        #endregion

        #region 获取全部商品分类
        public JsonArray GetProductCates()
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ParentID", 0);
                return GetProductCates(0);
            }
        }

        public JsonArray GetProductCatesByParentID(int parentID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ParentID", 0);
                return GetProductCates(parentID, false);
            }
        }

        private JsonArray GetProductCates(int parentID, bool deep = true)
        {
            helper.SetParameter("@ParentID", parentID);
            string sql = "select CategoryID,CategoryName,ParentID,Sort from ProductCates where ParentID=@ParentID order by Sort desc,CategoryID";
            JsonArray productCateList;
            JsonObject productCateObj;
            using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
            {
                if (dr.HasRows)
                {
                    productCateList = new JsonArray();
                    while (dr.Read())
                    {
                        productCateObj = new JsonObject();
                        productCateObj.Add("categoryID", (int)dr[0]);
                        productCateObj.Add("categoryName", (string)dr[1]);
                        productCateObj.Add("parentID", dr[2] == DBNull.Value ? 0 : (int)dr[2]);
                        productCateObj.Add("sort", dr[3] == DBNull.Value ? 0 : (int)dr[3]);
                        productCateList.Add(productCateObj);
                    }
                }
                else
                    productCateList = null;
            }
            if (productCateList != null && deep)
            {
                JsonArray newsCateChildren;
                for (int i = 0; i < productCateList.Count; i++)
                {
                    productCateObj = productCateList[i];
                    parentID = (int)productCateObj["categoryID"];
                    newsCateChildren = GetProductCates(parentID);
                    if (newsCateChildren != null)
                    {
                        productCateObj.Add("children", newsCateChildren);
                    }
                }
            }
            return productCateList;
        }
        #endregion

        #region 验证商品分类中是否包含子类或商品
        public bool IsProductCateHasChildren(int categoryID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CategoryID", categoryID);
                bool result = helper.GetBooleanValue("if exists (select 1 from ProductCates where ParentID=@CategoryID) select 1 else select 0", CommandType.Text);
                if (!result)
                    result = helper.GetBooleanValue("if exists (select 1 from Products where CategoryID=@CategoryID and Status<>2) select 1 else select 0", CommandType.Text);
                return result;
            }
        }
        #endregion

        #region 判断商品是否存在
        public bool IsProductExists(string productName)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@Name", 400, productName);
                return helper.GetBooleanValue("if exists (select 1 from Products where Name=@Name and Status<>2) select 1 else select 0", CommandType.Text);
            }
        }
        #endregion

        #region 添加商品
        public void AddProduct(ProductObj productObj)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                SqlParameter idParam = helper.AddOutputParameter("@ProductID");
                helper.AddIntParameter("@CategoryID", productObj.CategoryID);
                helper.AddStringParameter("@Name", 400, productObj.Name);
                helper.AddStringParameter("@Type", 100, productObj.Type);
                helper.AddStringParameter("@Serial", 200, productObj.Serial);
                helper.AddStringParameter("@Model", 100, productObj.Model);
                helper.AddStringParameter("@Code", 100, productObj.Code);
                helper.AddStringParameter("@Material", 100, productObj.Material);
                helper.AddStringParameter("@Weight", 100, productObj.Weight);
                helper.AddTextParameter("@Description", productObj.Description);
                helper.AddStringParameter("@Characteristic", 100, productObj.Characteristic);
                helper.AddStringParameter("@Designer", 50, productObj.Designer);
                helper.AddDecimalParameter("@Price", productObj.Price);
                helper.AddDecimalParameter("@SpecialPrice", productObj.SpecialPrice);
                helper.AddBoolenParameter("@IsNew", productObj.IsNew);
                helper.AddBoolenParameter("@CanPurchasedSeparately", productObj.CanPurchasedSeparately);
                helper.AddDecimalParameter("@Freight", productObj.Freight);
                helper.AddDecimalParameter("@Freight1", productObj.Freight1);
                helper.AddIntParameter("@Quantity", productObj.Quantity);
                helper.AddIntParameter("@Inventory", productObj.Inventory);
                helper.AddStringParameter("@Tags", 200, productObj.Tags);
                helper.AddIntParameter("@Points", productObj.Points);
                helper.AddDateTimeParameter("@Sort", productObj.Sort);
                helper.AddBoolenParameter("@IsOnSale", productObj.IsOnSale);
                helper.AddBoolenParameter("@IsRecommend", productObj.IsRecommend);

                string sql = "insert into Products (CategoryID,Name,Type,Serial,Model,Code,Material,Weight,Description,Characteristic,Designer,Price,SpecialPrice,IsNew,CanPurchasedSeparately,Freight,Quantity,Inventory,Tags,Points,Sort,IsOnSale,IsRecommend,SellNum,Status,Freight1) values (@CategoryID,@Name,@Type,@Serial,@Model,@Code,@Material,@Weight,@Description,@Characteristic,@Designer,@Price,@SpecialPrice,@IsNew,@CanPurchasedSeparately,@Freight,@Quantity,@Inventory,@Tags,@Points,@Sort,@IsOnSale,@IsRecommend,0,1,@Freight1) select @ProductID=@@IDENTITY";
                helper.ExecuteNonQuery(sql, CommandType.Text);
                productObj.ProductID = (int)idParam.Value;

                if (!string.IsNullOrEmpty(productObj.Tags))
                {
                    string[] tags = productObj.Tags.Split(' ');
                    sql = "if exists (select 1 from ProductTags where Tag=@Tag) update ProductTags set Weight=Weight+1 where Tag=@Tag else insert into ProductTags (Tag,Weight) values (@Tag,1)";
                    for (int i = 0; i < tags.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(tags[i]))
                        {
                            helper.ClearParameters();
                            helper.AddStringParameter("@Tags", 50, tags[i]);
                            helper.ExecuteNonQuery(sql, CommandType.Text);
                        }
                    }
                }

                if (productObj.FreightModels != null)
                {
                    for (int i = 0; i < productObj.FreightModels.Count; i++)
                    {
                        helper.ClearParameters();
                        helper.AddIntParameter("@ModelID", productObj.FreightModels[i]);
                        helper.AddIntParameter("@ProductID", productObj.ProductID);
                        helper.ExecuteNonQuery("insert into ProductFreight (ModelID,ProductID) values (@ModelID,@ProductID)", CommandType.Text);
                    }
                }

                if (productObj.ProductPictures != null)
                {
                    sql = "update ProductPictures set ProductID=@ProductID where PictureID=@PictureID";
                    ProductPictureObj productPicture;
                    for (int i = 0; i < productObj.ProductPictures.Count; i++)
                    {
                        helper.ClearParameters();
                        productPicture = productObj.ProductPictures[i];
                        helper.AddIntParameter("@PictureID", productPicture.PictureID);
                        helper.AddIntParameter("@ProductID", productObj.ProductID);

                        helper.ExecuteNonQuery(sql, CommandType.Text);
                    }
                }

                sql = "insert into LProdCate (ProductID,CategoryID) values (@ProductID,@CategoryID)";
                for (int i = 0; i < productObj.Categories.Count; i++)
                {
                    helper.ClearParameters();
                    helper.AddIntParameter("@ProductID", productObj.ProductID);
                    helper.AddIntParameter("@CategoryID", productObj.Categories[i]);
                    helper.ExecuteNonQuery(sql, CommandType.Text);
                }

                helper.CommitTran();
            }
        }
        #endregion

        #region 修改商品
        public void ModifyProduct(ProductObj productObj)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@ProductID", productObj.ProductID);
                helper.AddIntParameter("@CategoryID", productObj.CategoryID);
                helper.AddStringParameter("@Name", 400, productObj.Name);
                helper.AddStringParameter("@Type", 100, productObj.Type);
                helper.AddStringParameter("@Serial", 200, productObj.Serial);
                helper.AddStringParameter("@Model", 100, productObj.Model);
                helper.AddStringParameter("@Code", 100, productObj.Code);
                helper.AddStringParameter("@Material", 100, productObj.Material);
                helper.AddStringParameter("@Weight", 100, productObj.Weight);
                helper.AddTextParameter("@Description", productObj.Description);
                helper.AddStringParameter("@Characteristic", 100, productObj.Characteristic);
                helper.AddStringParameter("@Designer", 50, productObj.Designer);
                helper.AddDecimalParameter("@Price", productObj.Price);
                helper.AddDecimalParameter("@SpecialPrice", productObj.SpecialPrice);
                helper.AddBoolenParameter("@IsNew", productObj.IsNew);
                helper.AddBoolenParameter("@CanPurchasedSeparately", productObj.CanPurchasedSeparately);
                helper.AddDecimalParameter("@Freight", productObj.Freight);
                helper.AddDecimalParameter("@Freight1", productObj.Freight1);
                helper.AddIntParameter("@Quantity", productObj.Quantity);
                helper.AddIntParameter("@Inventory", productObj.Inventory);
                helper.AddStringParameter("@Tags", 200, productObj.Tags);
                helper.AddIntParameter("@Points", productObj.Points);
                helper.AddDateTimeParameter("@Sort", productObj.Sort);
                helper.AddBoolenParameter("@IsOnSale", productObj.IsOnSale);
                helper.AddBoolenParameter("@IsRecommend", productObj.IsRecommend);

                string sql = "update Products set CategoryID=@CategoryID,Name=@Name,Type=@Type,Serial=@Serial,Model=@Model,Code=@Code,Material=@Material,Weight=@Weight,Description=@Description,Characteristic=@Characteristic,Designer=@Designer,Price=@Price,SpecialPrice=@SpecialPrice,IsNew=@IsNew,CanPurchasedSeparately=@CanPurchasedSeparately,Freight=@Freight,Freight1=@Freight1,Quantity=@Quantity,Inventory=@Inventory,Tags=@Tags,Points=@Points,Sort=@Sort,IsOnSale=@IsOnSale,IsRecommend=@IsRecommend where ProductID=@ProductID";
                helper.ExecuteNonQuery(sql, CommandType.Text);

                if (!string.IsNullOrEmpty(productObj.Tags))
                {
                    string[] tags = productObj.Tags.Split(' ');
                    sql = "if exists (select 1 from ProductTags where Tag=@Tag) update ProductTags set Weight=Weight+1 where Tag=@Tag else insert into ProductTags (Tag,Weight) values (@Tag,1)";
                    for (int i = 0; i < tags.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(tags[i]))
                        {
                            helper.ClearParameters();
                            helper.AddStringParameter("@Tags", 50, tags[i]);
                            helper.ExecuteNonQuery(sql, CommandType.Text);
                        }
                    }
                }

                helper.ClearParameters();
                helper.AddIntParameter("@ProductID", productObj.ProductID);
                helper.ExecuteNonQuery("delete from ProductFreight where ProductID=@ProductID", CommandType.Text);

                if (productObj.FreightModels != null)
                {
                    for (int i = 0; i < productObj.FreightModels.Count; i++)
                    {
                        helper.ClearParameters();
                        helper.AddIntParameter("@ModelID", productObj.FreightModels[i]);
                        helper.AddIntParameter("@ProductID", productObj.ProductID);
                        helper.ExecuteNonQuery("insert into ProductFreight (ModelID,ProductID) values (@ModelID,@ProductID)", CommandType.Text);
                    }
                }

                if (productObj.ProductPictures != null)
                {
                    sql = "update ProductPictures set ProductID=@ProductID where PictureID=@PictureID";
                    ProductPictureObj productPicture;
                    for (int i = 0; i < productObj.ProductPictures.Count; i++)
                    {
                        helper.ClearParameters();
                        productPicture = productObj.ProductPictures[i];
                        helper.AddIntParameter("@PictureID", productPicture.PictureID);
                        helper.AddIntParameter("@ProductID", productObj.ProductID);

                        helper.ExecuteNonQuery(sql, CommandType.Text);
                    }
                }

                sql = "delete from LProdCate where ProductID=@ProductID";
                helper.ExecuteNonQuery(sql, CommandType.Text);

                sql = "insert into LProdCate (ProductID,CategoryID) values (@ProductID,@CategoryID)";
                for (int i = 0; i < productObj.Categories.Count; i++)
                {
                    helper.ClearParameters();
                    helper.AddIntParameter("@ProductID", productObj.ProductID);
                    helper.AddIntParameter("@CategoryID", productObj.Categories[i]);
                    helper.ExecuteNonQuery(sql, CommandType.Text);
                }

                helper.CommitTran();
            }
        }
        #endregion

        #region 删除商品
        public void DeleteProduct(int productID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ProductID", productID);

                helper.BeginTran();

                string sql = "update Products set Status=2 where ProductID=@ProductID";
                helper.ExecuteNonQuery(sql, CommandType.Text);

                sql = "delete from ProductPictures where ProductID=@ProductID";
                helper.ExecuteNonQuery(sql, CommandType.Text);

                sql = "delete from LProdCate where ProductID=@ProductID";
                helper.ExecuteNonQuery(sql, CommandType.Text);

                sql = "delete from LProdCate where ProductID=@ProductID";
                helper.ExecuteNonQuery(sql, CommandType.Text);

                helper.ExecuteNonQuery("delete from ProductFreight where ProductID=@ProductID", CommandType.Text);

                helper.CommitTran();
            }
        }
        #endregion

        #region 修改商品状态
        public void ModifyProduct(int productId, int status)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ProductID", productId);
                helper.AddIntParameter("@Status", status);

                string sql = "update Products set Status=@Status where ProductID=@ProductID";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region 根据商品编号获取商品信息
        public ProductObj GetProduct(int productID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ProductID", productID);

                string sql = "select CategoryID,Name,Tags,Type,Serial,Model,Code,Material,Weight,Description,Characteristic,Designer,Price,SpecialPrice,IsNew,CanPurchasedSeparately,Freight,Quantity,Inventory,Points,Sort,IsOnSale,IsRecommend,Status,SellNum,Freight1 from Products where ProductID=@ProductID";
                ProductObj productObj;
                SqlDataReader dr;
                using (dr = helper.ExecuteReader(sql, CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        productObj = new ProductObj();

                        productObj.ProductID = productID;
                        productObj.CategoryID = dr[0] == DBNull.Value ? 0 : (int)dr[0];
                        productObj.Name = (string)dr[1];
                        productObj.Tags = dr[2] == DBNull.Value ? null : (string)dr[2];
                        productObj.Type = dr[3] == DBNull.Value ? null : (string)dr[3];
                        productObj.Serial = dr[4] == DBNull.Value ? null : (string)dr[4];
                        productObj.Model = dr[5] == DBNull.Value ? null : (string)dr[5];
                        productObj.Code = dr[6] == DBNull.Value ? null : (string)dr[6];
                        productObj.Material = dr[7] == DBNull.Value ? null : (string)dr[7];
                        productObj.Weight = dr[8] == DBNull.Value ? null : (string)dr[8];
                        productObj.Description = dr[9] == DBNull.Value ? null : (string)dr[9];
                        productObj.Characteristic = dr[10] == DBNull.Value ? null : (string)dr[10];
                        productObj.Designer = dr[11] == DBNull.Value ? null : (string)dr[11];
                        productObj.Price = dr[12] == DBNull.Value ? 0 : (decimal)dr[12];
                        productObj.SpecialPrice = dr[13] == DBNull.Value ? 0 : (decimal)dr[13];
                        productObj.IsNew = dr[14] == DBNull.Value ? false : (bool)dr[14];
                        productObj.CanPurchasedSeparately = dr[15] == DBNull.Value ? false : (bool)dr[15];
                        productObj.Freight = dr[16] == DBNull.Value ? 0 : (decimal)dr[16];
                        productObj.Quantity = dr[17] == DBNull.Value ? 0 : (int)dr[17];
                        productObj.Inventory = dr[18] == DBNull.Value ? 0 : (int)dr[18];
                        productObj.Points = dr[19] == DBNull.Value ? 0 : (int)dr[19];
                        productObj.Sort = dr[20] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[20];
                        productObj.IsOnSale = dr[21] == DBNull.Value ? false : (bool)dr[21];
                        productObj.IsRecommend = dr[22] == DBNull.Value ? false : (bool)dr[22];
                        productObj.Status = dr[23] == DBNull.Value ? 0 : (int)dr[23];
                        productObj.SellNum = dr[24] == DBNull.Value ? 0 : (int)dr[24];
                        productObj.Freight1 = dr[25] == DBNull.Value ? 0 : (decimal)dr[25];

                    }
                    else
                        productObj = null;
                }
                if (productObj != null)
                {
                    sql = "select PictureID,ProductID,SavePath,Url,Type,PictureDesc from ProductPictures where ProductID=@ProductID";
                    using (dr = helper.ExecuteReader(sql, CommandType.Text))
                    {
                        if (dr.HasRows)
                        {
                            ProductPictureObj productPicture;
                            productObj.ProductPictures = new List<ProductPictureObj>();
                            productObj.Colors = new List<ProductPictureObj>();
                            while (dr.Read())
                            {
                                productPicture = new ProductPictureObj();
                                productPicture.PictureID = (int)dr[0];
                                productPicture.ProductID = (int)dr[1];
                                productPicture.SavePath = dr[2] == DBNull.Value ? null : (string)dr[2];
                                productPicture.Url = dr[3] == DBNull.Value ? null : (string)dr[3];
                                productPicture.Type = dr[4] == DBNull.Value ? 0 : (int)dr[4];
                                productPicture.PictureDesc = dr[5] == DBNull.Value ? null : (string)dr[5];

                                if (productPicture.Type == 0)
                                    productObj.ProductPictures.Add(productPicture);
                                else
                                    productObj.Colors.Add(productPicture);
                            }
                        }
                    }

                    sql = "select CategoryID from LProdCate where ProductID=@ProductID";
                    using (dr = helper.ExecuteReader(sql, CommandType.Text))
                    {
                        if (dr.HasRows)
                        {
                            productObj.Categories = new List<int>();
                            while (dr.Read())
                            {
                                productObj.Categories.Add((int)dr[0]);
                            }
                        }
                    }

                    sql = "select ProductFreight.ModelID,FreightModel.ModelName from ProductFreight inner join FreightModel on ProductFreight.ModelID=FreightModel.ModelID where ProductID=@ProductID";
                    using (dr = helper.ExecuteReader(sql, CommandType.Text))
                    {
                        if (dr.HasRows)
                        {
                            productObj.FreightModels = new List<int>();
                            productObj.FreightModelNames = new List<string>();
                            while (dr.Read())
                            {
                                productObj.FreightModels.Add((int)dr[0]);
                                productObj.FreightModelNames.Add(dr[1] == DBNull.Value ? null : (string)dr[1]);
                            }
                        }
                    }
                }
                return productObj;
            }
        }
        #endregion

        #region 搜索商品分类筛选
        public JsonArray GetProductCates(string keywords)
        {
            using (helper = new SqlHelper())
            {
                string sql = "select a.CategoryID,CategoryName,Qty from (select CategoryID,count(1) as Qty from Products where (Name like '%'+@Keywords+'%' or Tags like '%'+@Keywords+'%' or Code=@Keywords or Designer like '%'+@Keywords+'%' or Serial like '%'+@Keywords+'%' or Model like '%'+@Keywords+'%') group by CategoryID) a inner join ProductCates b on a.CategoryID=b.CategoryID";
                helper.AddStringParameter("@Keywords", 50, keywords);
                return helper.GetJsonArray(sql, CommandType.Text);
            }
        }
        #endregion

        #region 获取相关标签
        public JsonArray GetTags(string keywords)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@Keywords", 50, keywords);
                string sql = "select top 20 Tag from ProductTags where Tag like '%'+@Keywords+'%'";
                return helper.GetJsonArray(sql, CommandType.Text);
            }
        }
        #endregion

        #region 搜索商品
        public JsonArray GetProducts(int categoryId, string keywords, decimal priceFrom, decimal priceTo, int isNew, int isOnSale, int isRecommend, int canPurchasedSeparately, int status, int page, int pageSize, out int total, string sort, bool isAsc)
        {
            using (helper = new SqlHelper())
            {
                StringBuilder where = new StringBuilder(" Status=@Status");
                StringBuilder table = new StringBuilder("Products");
                if (categoryId != 0)
                {
                    helper.AddIntParameter("@ParentID", 0);
                    StringBuilder cateIds = new StringBuilder();
                    GetCategories(categoryId, cateIds);
                    table.Append(" inner join (select ProductID as LPID from LProdCate where CategoryID in (")
                        .Append(cateIds.ToString())
                        .Append(categoryId.ToString())
                        .Append(") group by ProductID) a on LPID=Products.ProductID");

                    helper.ClearParameters();
                }
                helper.AddIntParameter("@Status", status);

                table.Append(" inner join ProductCates on Products.CategoryID=ProductCates.CategoryID left join (select PictureDesc,Url,a.ProductID as PID from ProductPictures b inner join (select ProductID,min(PictureID) as PictureID from ProductPictures where Type=0 group by ProductID) a on a.PictureID=b.PictureID) c on PID=Products.ProductID");

                if (!string.IsNullOrEmpty(keywords))
                {
                    helper.AddStringParameter("@Keywords", 50, keywords);
                    where.Append(" and (Name like '%'+@Keywords+'%' or Tags like '%'+@Keywords+'%' or Code like '%'+@Keywords+'%' or Designer like '%'+@Keywords+'%' or Serial like '%'+@Keywords+'%' or Model like '%'+@Keywords+'%')");
                }
                if (priceTo != 0)
                {
                    helper.AddDecimalParameter("@PriceFrom", priceFrom);
                    helper.AddDecimalParameter("@PriceTo", priceTo);
                    where.Append(" and (Price between @PriceFrom and @PriceTo)");
                }
                else if (priceFrom != 0)
                {
                    helper.AddDecimalParameter("@PriceFrom", priceFrom);
                    where.Append(" and Price >= @PriceFrom");
                }
                if (isNew != -1)
                {
                    helper.AddBoolenParameter("@IsNew", isNew == 1 ? true : false);
                    where.Append(" and IsNew=@IsNew");
                }
                if (isOnSale != -1)
                {
                    helper.AddBoolenParameter("@IsOnSale", isOnSale == 1 ? true : false);
                    where.Append(" and IsOnSale=@IsOnSale");
                }
                if (isRecommend != -1)
                {
                    helper.AddBoolenParameter("@IsRecommend", isRecommend == 1 ? true : false);
                    where.Append(" and IsRecommend=@IsRecommend");
                }
                if (canPurchasedSeparately != -1)
                {
                    helper.AddBoolenParameter("@CanPurchasedSeparately", canPurchasedSeparately == 1 ? true : false);
                    where.Append(" and CanPurchasedSeparately=@CanPurchasedSeparately");
                }

                JsonArray result = helper.GetJsonArray(
                    new string[] { "ProductID" },
                    "Products.ProductID,Products.CategoryID,Name,Type,Serial,Model,Code,Material,Weight,Description,Characteristic,Designer,Price,SpecialPrice,IsNew,CanPurchasedSeparately,CategoryName,Freight,Freight1,Quantity,Inventory,Tags,Points,Products.Sort,IsOnSale,IsRecommend,PictureDesc,Url as PictureUrl,Status,SellNum",
                    table.ToString(),
                    where.ToString(), page, pageSize, out  total,
                    new Dictionary<string, bool>{
                        {sort, isAsc}
                    }
                );
                return result;
            }
        }

        private List<int> GetCategories(int parentID, StringBuilder sb)
        {
            helper.SetParameter("@ParentID", parentID);
            string sql = "select CategoryID from ProductCates where ParentID=@ParentID";
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

        #region 根据商品编号列表获取商品
        public JsonArray GetProducts(IList<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return null;
            using (helper = new SqlHelper())
            {
                StringBuilder sql = new StringBuilder("select ProductID,Products.CategoryID,Name,Type,Serial,Model,Code,Material,Weight,Description,Characteristic,Designer,Price,SpecialPrice,IsNew,CanPurchasedSeparately,CategoryName,Freight,Freight1,Quantity,Inventory,Tags,Points,Products.Sort,IsOnSale,IsRecommend,PictureDesc,Url,Url as PictureUrl,Status from Products inner join ProductCates on Products.CategoryID=ProductCates.CategoryID left join (select PictureDesc,Url,a.ProductID as PID from ProductPictures b inner join (select ProductID,min(PictureID) as PictureID from ProductPictures where Type=0 group by ProductID) a on a.PictureID=b.PictureID) a on PID=ProductID where Status=1 and ProductID in (");
                for (int i = 0; i < ids.Count; i++)
                {
                    if (i != 0)
                        sql.Append(",");
                    sql.Append(ids[i]);
                }
                sql.Append(")");

                return helper.GetJsonArray(sql.ToString(), CommandType.Text);
            }
        }
        #endregion

        #region GetJsonProductByID
        public JsonObject GetJsonProductByID(int id)
        {
            using (helper = new SqlHelper())
            {
                string sql = "select ProductID,Products.CategoryID,Name,Type,Serial,Model,Code,Material,Weight,Description,Characteristic,Designer,Price,SpecialPrice,IsNew,CanPurchasedSeparately,CategoryName,Freight,Freight1,Quantity,Inventory,Tags,Points,Products.Sort,IsOnSale,IsRecommend,PictureDesc,Url,Status from Products inner join ProductCates on Products.CategoryID=ProductCates.CategoryID left join (select PictureDesc,Url,a.ProductID as PID from ProductPictures b inner join (select ProductID,min(PictureID) as PictureID from ProductPictures where Type=0 group by ProductID) a on a.PictureID=b.PictureID) a on PID=ProductID where Status=1 and ProductID=@ProductID";

                helper.AddIntParameter("@ProductID", id);
                var res = helper.GetJsonArray(sql, CommandType.Text);
                return res == null || res.Count == 0 ? null : res[0];
            }
        }
        #endregion

        public decimal[] GetProductFreight(int productID, int regionID)
        {
            var area = new AreaDAL().GetAreaByRetionID(regionID);
            if (area == null)
                return null;

            int provID = (int)area["ProvinceID"];
            int cityID = (int)area["CityID"];

            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ProductID", productID);

                var freights = helper.GetJsonArray("select ProductFreight.ModelID,AreaID,AreaType,AreaFreight.Freight,AreaFreight.Freight1,FreightModel.Freight as DefaultFreight,FreightModel.Freight1 as DefaultFreight1 from ProductFreight inner join AreaFreight on ProductFreight.ModelID=AreaFreight.ModelID inner join FreightModel on ProductFreight.ModelID=FreightModel.ModelID where ProductID=@ProductID", CommandType.Text);
                if (freights == null)
                {
                    freights = helper.GetJsonArray("select AreaID,AreaType,AreaFreight.Freight,AreaFreight.Freight1,FreightModel.Freight as DefaultFreight,FreightModel.Freight1 as DefaultFreight1 from FreightModel inner join AreaFreight on AreaFreight.ModelID=FreightModel.ModelID where FreightModel.ModelID=1", CommandType.Text);
                    if (freights == null)
                        return null;
                }

                var a = from f in freights
                        where (byte)f["AreaType"] == 3 && f["AreaID"].ToString().Split(',').Contains<string>(regionID.ToString())
                        select f;
                if (a.Count<JsonObject>() != 0)
                {
                    var f1 = a.First<JsonObject>();
                    return new decimal[] { (decimal)f1["Freight"], (decimal)f1["Freight1"] };
                }
                var b = from f in freights
                        where (byte)f["AreaType"] == 2 && f["AreaID"].ToString().Split(',').Contains<string>(cityID.ToString())
                        select f;
                if (b.Count<JsonObject>() != 0)
                {
                    var f2 = b.First<JsonObject>();
                    return new decimal[] { (decimal)f2["Freight"], (decimal)f2["Freight1"] };
                }
                var c = from f in freights
                        where (byte)f["AreaType"] == 1 && f["AreaID"].ToString().Split(',').Contains<string>(provID.ToString())
                        select f;
                if (c.Count<JsonObject>() != 0)
                {
                    var f3 = c.First<JsonObject>();
                    return new decimal[] { (decimal)f3["Freight"], (decimal)f3["Freight1"] };
                }
                var f4 = freights[0];
                return new decimal[] { (decimal)f4["DefaultFreight"], (decimal)f4["DefaultFreight1"] };
            }
        }

        #region 新增商品图片
        public void AddPicture(ProductPictureObj productPictureObj)
        {
            using (helper = new SqlHelper())
            {
                string sql = "insert into ProductPictures (ProductID,SavePath,Url,PictureDesc,Type) values (@ProductID,@SavePath,@Url,@PictureDesc,@Type) select @PictureID=@@IDENTITY";
                SqlParameter picIDParam = helper.AddOutputParameter("@PictureID");
                helper.AddIntParameter("@ProductID", productPictureObj.ProductID);
                helper.AddStringParameter("@SavePath", 200, productPictureObj.SavePath);
                helper.AddStringParameter("@PictureDesc", 200, productPictureObj.PictureDesc);
                helper.AddStringParameter("@Url", 200, productPictureObj.Url);
                helper.AddIntParameter("@Type", productPictureObj.Type);

                helper.ExecuteNonQuery(sql, CommandType.Text);
                productPictureObj.PictureID = (int)picIDParam.Value;
            }
        }
        #endregion

        #region 删除产品图片
        public int DeleteProductPicture(int pictureID)
        {
            using (helper = new SqlHelper())
            {
                string sql = "delete from ProductPictures where PictureID=@PictureID";
                helper.AddIntParameter("@PictureID", pictureID);

                return helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region 修改商品图片
        public void ModifyPicture(ProductPictureObj productPictureObj)
        {
            using (helper = new SqlHelper())
            {
                string sql = "update ProductPictures set ProductID=@ProductID,PictureDesc=@PictureDesc,SavePath=@SavePath,Url=@Url where PictureID=@PictureID";
                helper.AddIntParameter("@PictureID", productPictureObj.PictureID);
                helper.AddIntParameter("@ProductID", productPictureObj.ProductID);
                helper.AddStringParameter("@SavePath", 200, productPictureObj.SavePath);
                helper.AddStringParameter("@PictureDesc", 200, productPictureObj.PictureDesc);
                helper.AddStringParameter("@Url", 200, productPictureObj.Url);

                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region 根据图片编号获取商品图片
        public ProductPictureObj GetProductPicture(int pictureID)
        {
            using (helper = new SqlHelper())
            {

                helper.AddIntParameter("@PictureID", pictureID);

                string sql = "select PictureID,ProductID,SavePath,Url,PictureDesc,Type from ProductPictures where PictureID=@PictureID";
                using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        ProductPictureObj productPicture = new ProductPictureObj();
                        productPicture.PictureID = (int)dr[0];
                        productPicture.ProductID = (int)dr[1];
                        productPicture.SavePath = dr[2] == DBNull.Value ? null : (string)dr[2];
                        productPicture.Url = dr[3] == DBNull.Value ? null : (string)dr[3];
                        productPicture.PictureDesc = dr[4] == DBNull.Value ? null : (string)dr[4];
                        productPicture.Type = dr[5] == DBNull.Value ? 0 : (int)dr[5];
                        return productPicture;
                    }
                    return null;
                }
            }
        }
        #endregion

        public void AddModel(string modelName, string content)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@ModelName", 100, modelName);
                helper.AddTextParameter("@Content", content);

                helper.ExecuteNonQuery("insert into ProductModels (ModelName,Content) values (@ModelName,@Content)", CommandType.Text);
            }
        }

        public void SaveModel(int modelId, string modelName, string content)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ModelId", modelId);
                helper.AddStringParameter("@ModelName", 100, modelName);
                helper.AddTextParameter("@Content", content);

                helper.ExecuteNonQuery("update ProductModels set ModelName=@ModelName,Content=@Content where ModelID=@ModelID", CommandType.Text);
            }
        }

        public void DeleteModel(int modelId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ModelID", modelId);
                helper.ExecuteNonQuery("delete from ProductModels where ModelID=@ModelID", CommandType.Text);
            }
        }

        public JsonArray GetModels()
        {
            using (helper = new SqlHelper())
            {
                return helper.GetJsonArray("select ModelID,ModelName,Content from ProductModels", CommandType.Text);
            }
        }

        #region 判断用户是否购买过某商品
        public bool IsBuy(int userID, int productID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userID);
                helper.AddIntParameter("@ProductID", productID);

                //Status=4 : 用户已确认收货
                return helper.GetBooleanValue("if exists (select 1 from Orders a inner join OrderDetails b on a.OrderID=b.OrderID where a.Status=4 and ProductID=@ProductID) select 1 else select 0", CommandType.Text);
            }
        }
        #endregion

        #region 添加评论
        public void AddComment(int userID, int productID, string content, int score)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userID);
                helper.AddIntParameter("@ProductID", productID);
                helper.AddIntParameter("@Score", score);
                helper.AddStringParameter("@Content", 2000, content);

                helper.ExecuteNonQuery("insert into ProductComments (ProductID,UserID,Content,CommentTime,Score,IsPass) values (@ProductID,@UserID,@Content,GetDate(),@Score,0)", CommandType.Text);
            }
        }
        #endregion

        #region 回复评论
        public void ReComment(int commentID, string re)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CommentID", commentID);
                helper.AddStringParameter("@Re", 2000, re);

                helper.ExecuteNonQuery("update ProductComments set Re=@Re where CommentID=@CommentID", CommandType.Text);
            }
        }
        #endregion

        #region 删除评论
        public void DeleteComment(int commentID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CommentID", commentID);
                helper.ExecuteNonQuery("delete from ProductComments where CommentID=@CommentID", CommandType.Text);
            }
        }

        public void DeleteComment(int userId, int commentID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userId);
                helper.AddIntParameter("@CommentID", commentID);
                helper.ExecuteNonQuery("delete from ProductComments where UserID=@UserID and CommentID=@CommentID", CommandType.Text);
            }
        }
        #endregion

        #region 获取评论
        public JsonArray GetComments(string userName, string product, DateTime dtFrom, DateTime dtTo, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                StringBuilder where = new StringBuilder("1=1");
                if (!string.IsNullOrEmpty(product))
                {
                    helper.AddStringParameter("@Product", 50, product);
                    where.Append(" and (Name like '%'+@Product+'%' or Code like '%'+@Product+'%')");
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    helper.AddStringParameter("@UserName", 50, userName);
                    where.Append(" and UserName like '%'+@UserName+'%'");
                }

                if (dtFrom != DateTime.MinValue && dtTo != DateTime.MinValue)
                {
                    helper.AddDateTimeParameter("@DtFrom", dtFrom.Date);
                    helper.AddDateTimeParameter("@DtTo", dtTo.Date.AddDays(1).AddMilliseconds(-3));
                    where.Append(" and CommentTime between @DtFrom and @DtTo");
                }

                return helper.GetJsonArray("CommentID",
                    "CommentID,Content,CommentTime,Re,Score,a.UserID,UserName,a.ProductID,Name,Code,Material,Weight,Description,Characteristic,Designer,Price,SpecialPrice,IsNew,CanPurchasedSeparately,CategoryName,Freight,Freight1,Quantity,Inventory,Tags,b.Points,IsOnSale,IsRecommend,PictureDesc,Url as PictureUrl,Status,SellNum",
                    "ProductComments a inner join Products b on a.ProductID=b.ProductID inner join ProductCates c on b.CategoryID=c.CategoryID left join (select PictureDesc,Url,d.ProductID as PID from ProductPictures b inner join (select ProductID,min(PictureID) as PictureID from ProductPictures where Type=0 group by ProductID) d on b.PictureID=d.PictureID) e on PID=a.ProductID inner join Users f on a.UserID=f.UserID",
                    where.ToString(),
                    page,
                    pageSize,
                    out total,
                    "CommentTime",
                    false);
            }
        }

        public JsonArray GetComments(int productId, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ProductID", productId);

                return helper.GetJsonArray("CommentID",
                    "CommentID,Content,CommentTime,Re,Score,a.UserID,UserName,a.ProductID,Name,Code,Material,Weight,Description,Characteristic,Designer,Price,SpecialPrice,IsNew,CanPurchasedSeparately,CategoryName,Freight,Freight1,Quantity,Inventory,Tags,b.Points,IsOnSale,IsRecommend,PictureDesc,Url as PictureUrl,Status,SellNum",
                    "ProductComments a inner join Products b on a.ProductID=b.ProductID inner join ProductCates c on b.CategoryID=c.CategoryID left join (select PictureDesc,Url,d.ProductID as PID from ProductPictures b inner join (select ProductID,min(PictureID) as PictureID from ProductPictures where Type=0 group by ProductID) d on b.PictureID=d.PictureID) e on PID=a.ProductID inner join Users f on a.UserID=f.UserID",
                    "a.ProductID=@ProductID",
                    page,
                    pageSize,
                    out total,
                    "CommentTime",
                    false);
            }
        }

        public string GetRe(int commentID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@CommentID", commentID);

                return helper.GetStringValue("select Re from ProductComments where CommentID=@CommentID", CommandType.Text);
            }
        }
        #endregion

        public JsonArray GetCommentsByUserID(int userId, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userId);

                return helper.GetJsonArray("CommentID",
                    "CommentID,Content,CommentTime,Re,Score,a.UserID,UserName,a.ProductID,Name,Code,Material,Weight,Description,Characteristic,Designer,Price,SpecialPrice,IsNew,CanPurchasedSeparately,CategoryName,Freight,Freight1,Quantity,Inventory,Tags,b.Points,IsOnSale,IsRecommend,PictureDesc,Url as PictureUrl,Status,SellNum",
                    "ProductComments a inner join Products b on a.ProductID=b.ProductID inner join ProductCates c on b.CategoryID=c.CategoryID left join (select PictureDesc,Url,d.ProductID as PID from ProductPictures b inner join (select ProductID,min(PictureID) as PictureID from ProductPictures where Type=0 group by ProductID) d on b.PictureID=d.PictureID) e on PID=a.ProductID inner join Users f on a.UserID=f.UserID",
                    "a.UserID=@UserID",
                    page,
                    pageSize,
                    out total,
                    "CommentTime",
                    false);
            }
        }

        #region AddMessage
        public void AddMessage(string userName, int productID, string content, bool isAnonymity)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ProductID", productID);
                helper.AddStringParameter("@Content", 2000, content);
                helper.AddStringParameter("@UserName", 100, userName);
                helper.AddBoolenParameter("@IsAnonymity", isAnonymity);

                helper.ExecuteNonQuery("insert into ProductMessages (ProductID,Content,UserName,IsAnonymity,ParentID,AddTime) values (@ProductID,@Content,@UserName,@IsAnonymity,0,GetDate())", CommandType.Text);
            }
        }
        #endregion

        #region ReMessage
        public void ReMessage(int messageID, string re)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@MessageID", messageID);
                helper.AddStringParameter("@Re", 2000, re);

                helper.ExecuteNonQuery("update ProductMessages set Re=@Re where MessageID=@MessageID", CommandType.Text);
            }
        }
        #endregion

        #region DeleteMessage
        public void DeleteMessage(int messageID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@MessageID", messageID);

                helper.ExecuteNonQuery("delete from ProductMessages where MessageID=@MessageID", CommandType.Text);
            }
        }
        #endregion

        #region GetMessages
        public JsonArray GetMessages(int productID, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@ProductID", productID);

                return helper.GetJsonArray("MessageID",
                    "MessageID,IsAnonymity,Content,AddTime,Re,UserName,a.ProductID,Name,Code,Material,Weight,Description,Characteristic,Designer,Price,SpecialPrice,IsNew,CanPurchasedSeparately,CategoryName,Freight,Freight1,Quantity,Inventory,Tags,b.Points,IsOnSale,IsRecommend,PictureDesc,Url as PictureUrl,Status,SellNum",
                    "ProductMessages a inner join Products b on a.ProductID=b.ProductID inner join ProductCates c on b.CategoryID=c.CategoryID left join (select PictureDesc,Url,d.ProductID as PID from ProductPictures b inner join (select ProductID,min(PictureID) as PictureID from ProductPictures where Type=0 group by ProductID) d on b.PictureID=d.PictureID) e on PID=a.ProductID",
                    "a.ProductID=@ProductID",
                    page,
                    pageSize,
                    out total,
                    "AddTime",
                    false);
            }
        }
        #endregion

        #region GetMessages
        public JsonArray GetMessages(string product, DateTime dtFrom, DateTime dtTo, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                StringBuilder where = new StringBuilder("1=1");
                if (!string.IsNullOrEmpty(product))
                {
                    helper.AddStringParameter("@Product", 50, product);
                    where.Append(" and (Name like '%'+@Product+'%' or Code like '%'+@Product+'%')");
                }

                if (dtFrom != DateTime.MinValue && dtTo != DateTime.MinValue)
                {
                    helper.AddDateTimeParameter("@DtFrom", dtFrom.Date);
                    helper.AddDateTimeParameter("@DtTo", dtTo.Date.AddDays(1).AddMilliseconds(-3));
                    where.Append(" and AddTime between @DtFrom and @DtTo");
                }

                return helper.GetJsonArray("MessageID",
                    "MessageID,IsAnonymity,Content,AddTime,Re,UserName,a.ProductID,Name,Code,Material,Weight,Description,Characteristic,Designer,Price,SpecialPrice,IsNew,CanPurchasedSeparately,CategoryName,Freight,Freight1,Quantity,Inventory,Tags,b.Points,IsOnSale,IsRecommend,PictureDesc,Url as PictureUrl,Status,SellNum",
                    "ProductMessages a inner join Products b on a.ProductID=b.ProductID inner join ProductCates c on b.CategoryID=c.CategoryID left join (select PictureDesc,Url,d.ProductID as PID from ProductPictures b inner join (select ProductID,min(PictureID) as PictureID from ProductPictures where Type=0 group by ProductID) d on b.PictureID=d.PictureID) e on PID=a.ProductID",
                    where.ToString(),
                    page,
                    pageSize,
                    out total,
                    "AddTime",
                    false);
            }
        }
        #endregion

        #region 获取评论
        public string GetMessageRe(int messageID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@MessageID", messageID);

                return helper.GetStringValue("select Re from ProductMessages where MessageID=@MessageID", CommandType.Text);
            }
        }
        #endregion

        public int GetProductIDByCode(string code)
        {

            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@Code", 100, code);

                return helper.GetIntValue("select ProductID from Products where Code=@Code", CommandType.Text);
            }

        }

        public bool IsComment(int userID, int productId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@UserID", userID);
                helper.AddIntParameter("@ProductID", productId);

                int count = helper.GetIntValue("select count(1) from ProductComments where UserID=@UserID and ProductID=@ProductID", CommandType.Text);

                int count1 = helper.GetIntValue("select count(1) from (select a.OrderID from Orders a inner join OrderDetails b on a.OrderID=b.OrderID inner join Products c on b.ProductID=c.ProductID where a.UserID=@UserID and b.ProductID=@ProductID group by a.OrderID) a", CommandType.Text);

                return count >= count1;
            }
        }
    }
}
