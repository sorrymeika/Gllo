using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gllo.Models;

namespace Gllo.DAL
{
    public class Exchange
    {
        private SqlHelper dbc;
        private IList<Func<bool>> handlers = new List<Func<bool>>();

        public JsonArray GetExchange(int page, int pageSize, out int total,
            int? userID = null,
            int? orderID = null,
            string orderCode = null,
            string userName = null,
            DateTime? addTimeFrom = null,
            DateTime? addTimeTo = null)
        {
            using (dbc = new SqlHelper())
            {
                string where = "1=1";
                if (userID != null)
                {
                    dbc.AddIntParameter("@UserID", (int)userID);
                    where += " and b.UserID=@UserID";
                }
                if (!string.IsNullOrEmpty(orderCode))
                {
                    dbc.AddStringParameter("@OrderCode", 50, orderCode);
                    where += " and b.OrderCode=@OrderCode";
                }
                if (!string.IsNullOrEmpty(userName))
                {
                    dbc.AddStringParameter("@UserName", 50, userName);
                    where += " and e.UserName=@UserName";
                }
                if (orderID != null)
                {
                    dbc.AddIntParameter("@OrderID", (int)orderID);
                    where += " and a.OrderID=@OrderID";
                }
                if (addTimeFrom != null && addTimeTo != null)
                {
                    dbc.AddDateTimeParameter("@AddTimeFrom", addTimeFrom.Value.Date);
                    dbc.AddDateTimeParameter("@AddTimeTo", addTimeTo.Value.Date.AddDays(1).AddMilliseconds(-3));
                    where += " and a.AddTime between @AddTimeFrom and @AddTimeTo";
                }
                return dbc.GetJsonArray("ExchangeID",
                    "a.ExchangeID,a.OrderID,a.AddTime,a.Mobile,a.Phone,a.Address,a.RegionID,a.Zip,a.ReturnProductID,a.ReturnQty,a.ExchangeProductID,a.ExchangeQty,a.Reason,b.OrderCode,b.Receiver,c.Name as ExchangeProductName,c.Price as ExchangeProductPrice,c.IsOnSale as ExchangeProductIsOnSale,c.SpecialPrice as ExchangeProductSpecialPrice,d.Name as ReturnProductName,d.Price as ReturnProductPrice,d.IsOnSale as ReturnProductIsOnSale,d.SpecialPrice as ReturnProductSpecialPrice,b.UserID,e.UserName",
                    "Exchange a inner join Orders b on a.OrderID=b.OrderID left join Products c on a.ExchangeProductID=c.ProductID inner join Products d on a.ReturnProductID=d.ProductID inner join Users e on e.UserID=b.UserID",
                    where,
                    page,
                    pageSize,
                    out total);
            }
        }

        public IList<Tuple<ExchangeInfo, OrderObj, ProductObj, ProductObj>> GetExchange(int userID, int? orderID = null)
        {
            using (dbc = new SqlHelper())
            {
                string sql = "select a.ExchangeID,a.OrderID,a.AddTime,a.Mobile,a.Phone,a.Address,a.RegionID,a.Zip,a.ReturnProductID,a.ReturnQty,a.ExchangeProductID,a.ExchangeQty,a.Reason,b.OrderCode,b.Receiver,c.Name as ExchangeProductName,c.Price as ExchangeProductPrice,c.IsOnSale as ExchangeProductIsOnSale,c.SpecialPrice as ExchangeProductSpecialPrice,d.Name as ReturnProductName,d.Price as ReturnProductPrice,d.IsOnSale as ReturnProductIsOnSale,d.SpecialPrice as ReturnProductSpecialPrice from Exchange a inner join Orders b on a.OrderID=b.OrderID inner join Products c on a.ExchangeProductID=c.ProductID inner join Products d on a.ReturnProductID=d.ProductID where UserID=@UserID";
                dbc.AddIntParameter("@UserID", userID);
                if (orderID != null)
                {
                    dbc.AddIntParameter("@OrderID", (int)orderID);
                    sql += " and a.OrderID=@OrderID";
                }

                IList<Tuple<ExchangeInfo, OrderObj, ProductObj, ProductObj>> result = null;

                dbc.Read(sql, System.Data.CommandType.Text, dr =>
                {
                    if (dr.HasRows)
                    {
                        result = new List<Tuple<ExchangeInfo, OrderObj, ProductObj, ProductObj>>();
                        ExchangeInfo exchangeInfo;
                        OrderObj orderObj;
                        ProductObj returnProduct;
                        ProductObj exchangeProduct;
                        while (dr.Read())
                        {
                            exchangeInfo = new ExchangeInfo();
                            exchangeInfo.ExchangeID = dr["ExchangeID"] == DBNull.Value ? 0 : (int)dr["ExchangeID"];
                            exchangeInfo.OrderID = dr["OrderID"] == DBNull.Value ? 0 : (int)dr["OrderID"];
                            exchangeInfo.AddTime = dr["AddTime"] == DBNull.Value ? DateTime.MinValue : (DateTime)dr["AddTime"];
                            exchangeInfo.Mobile = dr["Mobile"] == DBNull.Value ? null : (string)dr["Mobile"];
                            exchangeInfo.Phone = dr["Phone"] == DBNull.Value ? null : (string)dr["Phone"];
                            exchangeInfo.Address = dr["Address"] == DBNull.Value ? null : (string)dr["Address"];
                            exchangeInfo.RegionID = dr["RegionID"] == DBNull.Value ? 0 : (int)dr["RegionID"];
                            exchangeInfo.Zip = dr["Zip"] == DBNull.Value ? null : (string)dr["Zip"];
                            exchangeInfo.ReturnProductID = dr["ReturnProductID"] == DBNull.Value ? 0 : (int)dr["ReturnProductID"];
                            exchangeInfo.ReturnQty = dr["ReturnQty"] == DBNull.Value ? 0 : (int)dr["ReturnQty"];
                            exchangeInfo.ExchangeProductID = dr["ExchangeProductID"] == DBNull.Value ? 0 : (int)dr["ExchangeProductID"];
                            exchangeInfo.ExchangeQty = dr["ExchangeQty"] == DBNull.Value ? 0 : (int)dr["ExchangeQty"];
                            exchangeInfo.Reason = dr["Reason"] == DBNull.Value ? null : (string)dr["Reason"];

                            orderObj = new OrderObj();
                            orderObj.OrderID = exchangeInfo.OrderID;
                            orderObj.OrderCode = dr["OrderCode"] == DBNull.Value ? null : (string)dr["OrderCode"];
                            orderObj.Receiver = dr["Receiver"] == DBNull.Value ? null : (string)dr["Receiver"];

                            returnProduct = new ProductObj();
                            returnProduct.ProductID = exchangeInfo.ReturnProductID;
                            returnProduct.Name = dr["ReturnProductName"] == DBNull.Value ? null : (string)dr["ReturnProductName"];
                            returnProduct.Price = dr["ReturnProductPrice"] == DBNull.Value ? 0 : (decimal)dr["ReturnProductPrice"];
                            returnProduct.IsOnSale = dr["ReturnProductIsOnSale"] == DBNull.Value ? false : (bool)dr["ReturnProductIsOnSale"];
                            returnProduct.Price = dr["ReturnProductPrice"] == DBNull.Value ? 0 : (decimal)dr["ReturnProductPrice"];
                            returnProduct.SpecialPrice = dr["ReturnProductSpecialPrice"] == DBNull.Value ? 0 : (decimal)dr["ReturnProductSpecialPrice"];

                            exchangeProduct = new ProductObj();
                            exchangeProduct.ProductID = exchangeInfo.ExchangeProductID;
                            exchangeProduct.Name = dr["ExchangeProductName"] == DBNull.Value ? null : (string)dr["ExchangeProductName"];
                            exchangeProduct.Price = dr["ExchangeProductPrice"] == DBNull.Value ? 0 : (decimal)dr["ExchangeProductPrice"];
                            exchangeProduct.IsOnSale = dr["ExchangeProductIsOnSale"] == DBNull.Value ? false : (bool)dr["ExchangeProductIsOnSale"];
                            exchangeProduct.Price = dr["ExchangeProductPrice"] == DBNull.Value ? 0 : (decimal)dr["ExchangeProductPrice"];
                            exchangeProduct.SpecialPrice = dr["ExchangeProductSpecialPrice"] == DBNull.Value ? 0 : (decimal)dr["ExchangeProductSpecialPrice"];

                            result.Add(new Tuple<ExchangeInfo, OrderObj, ProductObj, ProductObj>(exchangeInfo, orderObj, returnProduct, exchangeProduct));
                        }

                    }
                });

                return result;
            }
        }

        public ExchangeInfo GetExchangeByID(int exchangeID)
        {
            using (dbc = new SqlHelper())
            {
                dbc.AddIntParameter("@ExchangeID", exchangeID);

                ExchangeInfo exchangeInfo = null;
                dbc.Read("select ExchangeID,OrderID,AddTime,Mobile,Phone,Address,RegionID,Zip,ReturnProductID,ReturnQty,ExchangeProductID,ExchangeQty,Reason from Exchange where ExchangeID=@ExchangeID", System.Data.CommandType.Text, dr =>
                {
                    if (dr.HasRows && dr.Read())
                    {
                        exchangeInfo = new ExchangeInfo();
                        exchangeInfo.ExchangeID = dr["ExchangeID"] == DBNull.Value ? 0 : (int)dr["ExchangeID"];
                        exchangeInfo.OrderID = dr["OrderID"] == DBNull.Value ? 0 : (int)dr["OrderID"];
                        exchangeInfo.AddTime = dr["AddTime"] == DBNull.Value ? DateTime.MinValue : (DateTime)dr["AddTime"];
                        exchangeInfo.Mobile = dr["Mobile"] == DBNull.Value ? null : (string)dr["Mobile"];
                        exchangeInfo.Phone = dr["Phone"] == DBNull.Value ? null : (string)dr["Phone"];
                        exchangeInfo.Address = dr["Address"] == DBNull.Value ? null : (string)dr["Address"];
                        exchangeInfo.RegionID = dr["RegionID"] == DBNull.Value ? 0 : (int)dr["RegionID"];
                        exchangeInfo.Zip = dr["Zip"] == DBNull.Value ? null : (string)dr["Zip"];
                        exchangeInfo.ReturnProductID = dr["ReturnProductID"] == DBNull.Value ? 0 : (int)dr["ReturnProductID"];
                        exchangeInfo.ReturnQty = dr["ReturnQty"] == DBNull.Value ? 0 : (int)dr["ReturnQty"];
                        exchangeInfo.ExchangeProductID = dr["ExchangeProductID"] == DBNull.Value ? 0 : (int)dr["ExchangeProductID"];
                        exchangeInfo.ExchangeQty = dr["ExchangeQty"] == DBNull.Value ? 0 : (int)dr["ExchangeQty"];
                        exchangeInfo.Reason = dr["Reason"] == DBNull.Value ? null : (string)dr["Reason"];
                    }
                });

                return exchangeInfo;
            }
        }

        public void AddExchange(ExchangeInfo exchangeInfo)
        {
            handlers.Add(() =>
            {
                dbc.AddIntParameter("@OrderID", exchangeInfo.OrderID);
                dbc.AddDateTimeParameter("@AddTime", exchangeInfo.AddTime);
                dbc.AddStringParameter("@Mobile", 50, exchangeInfo.Mobile);
                dbc.AddStringParameter("@Phone", 50, exchangeInfo.Phone);
                dbc.AddStringParameter("@Address", 100, exchangeInfo.Address);
                dbc.AddIntParameter("@RegionID", exchangeInfo.RegionID);
                dbc.AddStringParameter("@Zip", 20, exchangeInfo.Zip);
                dbc.AddIntParameter("@ReturnProductID", exchangeInfo.ReturnProductID);
                dbc.AddIntParameter("@ReturnQty", exchangeInfo.ReturnQty);
                dbc.AddIntParameter("@ExchangeProductID", exchangeInfo.ExchangeProductID);
                dbc.AddIntParameter("@ExchangeQty", exchangeInfo.ExchangeQty);
                dbc.AddTextParameter("@Reason", exchangeInfo.Reason);

                dbc.ExecuteNonQuery("insert into Exchange (OrderID,AddTime,Mobile,Phone,Address,RegionID,Zip,ReturnProductID,ReturnQty,ExchangeProductID,ExchangeQty,Reason) values (@OrderID,@AddTime,@Mobile,@Phone,@Address,@RegionID,@Zip,@ReturnProductID,@ReturnQty,@ExchangeProductID,@ExchangeQty,@Reason)", System.Data.CommandType.Text);

                return true;
            });
        }

        public void DeleteExchange(int exchangeID)
        {
            handlers.Add(() =>
            {
                dbc.AddIntParameter("@ExchangeID", exchangeID);
                dbc.ExecuteNonQuery("delete from Exchange where ExchangeID=@ExchangeID", System.Data.CommandType.Text);
                return true;
            });
        }

        public void UpdateExchange(ExchangeInfo exchangeInfo)
        {
            handlers.Add(() =>
            {
                dbc.AddIntParameter("@ExchangeID", exchangeInfo.ExchangeID);
                dbc.AddDateTimeParameter("@AddTime", exchangeInfo.AddTime);
                dbc.AddStringParameter("@Mobile", 50, exchangeInfo.Mobile);
                dbc.AddStringParameter("@Phone", 50, exchangeInfo.Phone);
                dbc.AddStringParameter("@Address", 100, exchangeInfo.Address);
                dbc.AddIntParameter("@RegionID", exchangeInfo.RegionID);
                dbc.AddStringParameter("@Zip", 20, exchangeInfo.Zip);
                dbc.AddIntParameter("@ReturnProductID", exchangeInfo.ReturnProductID);
                dbc.AddIntParameter("@ReturnQty", exchangeInfo.ReturnQty);
                dbc.AddIntParameter("@ExchangeProductID", exchangeInfo.ExchangeProductID);
                dbc.AddIntParameter("@ExchangeQty", exchangeInfo.ExchangeQty);
                dbc.AddTextParameter("@Reason", exchangeInfo.Reason);

                dbc.ExecuteNonQuery("update Exchange set AddTime=@AddTime,Mobile=@Mobile,Phone=@Phone,Address=@Address,RegionID=@RegionID,Zip=@Zip,ReturnProductID=@ReturnProductID,ReturnQty=@ReturnQty,ExchangeProductID=@ExchangeProductID,ExchangeQty=@ExchangeQty,Reason=@Reason where ExchangeID=@ExchangeID", System.Data.CommandType.Text);

                return true;
            });
        }

        public int GetReturnQty(int productId, int orderId)
        {
            using (dbc = new SqlHelper())
            {
                dbc.AddIntParameter("@ReturnProductID", productId);
                dbc.AddIntParameter("@OrderID", orderId);

                return dbc.GetIntValue("select sum(ReturnQty) from Exchange where ReturnProductID=@ReturnProductID and OrderID=@OrderID", System.Data.CommandType.Text);
            }
        }

        public bool SubmitChanges()
        {
            if (handlers.Count != 0)
            {
                using (dbc = new SqlHelper())
                {
                    dbc.BeginTran();

                    bool res = handlers.All<Func<bool>>(func =>
                    {
                        return func();
                    });
                    handlers.Clear();

                    if (res)
                    {
                        dbc.CommitTran();
                    }
                    else
                    {
                        dbc.RollbackTran();
                    }
                    return res;
                }
            }
            return false;
        }
    }
}
