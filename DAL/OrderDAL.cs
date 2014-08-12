using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using Gllo.Models;

namespace Gllo.DAL
{
    public class OrderDAL
    {
        SqlHelper helper;

        #region 添加订单
        public int AddOrder(OrderObj orderObj)
        {
            using (helper = new SqlHelper())
            {
                string sql = "insert into Orders (CreationTime,Status,PayType,PayStatus,Memo,UserID,Receiver,IP,CityID,RegionID,Address,Zip,Mobile,Phone,OriginalAmount,Amount,Freight) values (@CreationTime,@Status,@PayType,@PayStatus,@Memo,@UserID,@Receiver,@IP,@CityID,@RegionID,@Address,@Zip,@Mobile,@Phone,@OriginalAmount,@Amount,@Freight) select @OrderID=@@IDENTITY";

                helper.BeginTran();

                SqlParameter orderIDParam = helper.AddOutputParameter("@OrderID");
                helper.AddDateTimeParameter("@CreationTime", orderObj.CreationTime);
                helper.AddIntParameter("@Status", orderObj.Status);
                helper.AddIntParameter("@PayType", orderObj.PayType);
                helper.AddIntParameter("@PayStatus", orderObj.PayStatus);
                helper.AddStringParameter("@Memo", 2000, orderObj.Memo);
                helper.AddIntParameter("@UserID", orderObj.UserID);
                helper.AddStringParameter("@Receiver", 50, orderObj.Receiver);
                helper.AddStringParameter("@IP", 50, orderObj.IP);
                helper.AddIntParameter("@CityID", orderObj.CityID);
                helper.AddIntParameter("@RegionID", orderObj.RegionID);
                helper.AddStringParameter("@Address", 400, orderObj.Address);
                helper.AddStringParameter("@Zip", 20, orderObj.Zip);
                helper.AddStringParameter("@Mobile", 50, orderObj.Mobile);
                helper.AddStringParameter("@Phone", 50, orderObj.Phone);
                helper.AddDecimalParameter("@OriginalAmount", orderObj.OriginalAmount);
                helper.AddDecimalParameter("@Amount", orderObj.Amount);
                helper.AddDecimalParameter("@Freight", orderObj.Freight);

                helper.ExecuteNonQuery(sql, CommandType.Text);
                orderObj.OrderID = (int)orderIDParam.Value;

                string zero = "0000000000";
                string tmp = zero + orderObj.OrderID;
                orderObj.OrderCode = DateTime.Now.ToString("yyMMdd") + tmp.Substring(tmp.Length - 10, 10);

                OrderProductObj product;

                for (int i = 0; i < orderObj.OrderDetails.Count; i++)
                {
                    sql = "insert into OrderDetails (OrderID,ProductID,Discount,OrignalPrice,Quantity,CreationTime,Status,Express,PackageID) values (@OrderID,@ProductID,@Discount,@OrignalPrice,@Quantity,@CreationTime,@Status,@Express,@PackageID)";

                    product = orderObj.OrderDetails[i];
                    helper.ClearParameters();
                    product.OrderID = orderObj.OrderID;
                    helper.AddIntParameter("@OrderID", product.OrderID);
                    helper.AddIntParameter("@ProductID", product.ProductID);
                    helper.AddIntParameter("@PackageID", product.PackageID);
                    helper.AddDecimalParameter("@Discount", product.Discount);
                    helper.AddDecimalParameter("@OrignalPrice", product.OrignalPrice);
                    helper.AddDecimalParameter("@Freight", product.Freight);
                    helper.AddDecimalParameter("@Quantity", product.Quantity);
                    helper.AddDateTimeParameter("@CreationTime", product.CreationTime);
                    helper.AddIntParameter("@Status", product.Status);
                    helper.AddIntParameter("@Express", product.Express);

                    helper.ExecuteNonQuery(sql, CommandType.Text);

                    sql = "update Products set Quantity=Quantity-@Quantity where ProductID=@ProductID";
                    helper.ExecuteNonQuery(sql, CommandType.Text);
                }

                helper.ClearParameters();
                helper.AddIntParameter("@OrderID", orderObj.OrderID);
                helper.AddStringParameter("@OrderCode", 50, orderObj.OrderCode);
                helper.ExecuteNonQuery("update Orders set OrderCode=@OrderCode where OrderID=@OrderID", CommandType.Text);

                helper.CommitTran();

                return orderObj.OrderID;
            }
        }
        #endregion

        #region 根据编号获取订单信息
        public OrderObj GetOrderByID(int orderId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@OrderID", orderId);
                string sql = "select OrderCode,CreationTime,Status,PayType,PayStatus,Memo,UserID,Receiver,IP,a.CityID,a.RegionID,Address,Zip,Mobile,Phone,OriginalAmount,Amount,Freight,EarnestMoney,Measurement,IsDistribute,IsConsigned,Logistics,LogisticsCode,LogisticStatus,SignIn,Score,Comments,CityName,ProvinceName,RegionName,c.ProvinceID,PayedTime,BuyerEmail,ServiceMemo,TransportType,TradeNo,a.ExpressID,ExpressName,ExpressUrl from Orders a inner join City b on a.CityID=b.CityID inner join Province c on c.ProvinceID=b.ProvinceID left join Region d on d.RegionID=a.RegionID left join Express e on e.ExpressID=a.ExpressID where OrderID=@OrderID";

                using (SqlDataReader dr = helper.ExecuteReader(sql, CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        OrderObj orderObj = new OrderObj();
                        orderObj.OrderID = orderId;
                        orderObj.OrderCode = dr[0] == DBNull.Value ? null : (string)dr[0];
                        orderObj.CreationTime = dr[1] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[1];
                        orderObj.Status = dr[2] == DBNull.Value ? 0 : (int)dr[2];
                        orderObj.PayType = dr[3] == DBNull.Value ? 1 : (int)dr[3];
                        orderObj.PayStatus = dr[4] == DBNull.Value ? 0 : (int)dr[4];
                        orderObj.Memo = dr[5] == DBNull.Value ? null : (string)dr[5];
                        orderObj.UserID = dr[6] == DBNull.Value ? 0 : (int)dr[6];
                        orderObj.Receiver = dr[7] == DBNull.Value ? null : (string)dr[7];
                        orderObj.IP = dr[8] == DBNull.Value ? null : (string)dr[8];
                        orderObj.CityID = dr[9] == DBNull.Value ? 0 : (int)dr[9];
                        orderObj.RegionID = dr[10] == DBNull.Value ? 0 : (int)dr[10];
                        orderObj.Address = dr[11] == DBNull.Value ? null : (string)dr[11];
                        orderObj.Zip = dr[12] == DBNull.Value ? null : (string)dr[12];
                        orderObj.Mobile = dr[13] == DBNull.Value ? null : (string)dr[13];
                        orderObj.Phone = dr[14] == DBNull.Value ? null : (string)dr[14];
                        orderObj.OriginalAmount = dr[15] == DBNull.Value ? 0 : (decimal)dr[15];
                        orderObj.Amount = dr[16] == DBNull.Value ? 0 : (decimal)dr[16];
                        orderObj.Freight = dr[17] == DBNull.Value ? 0 : (decimal)dr[17];
                        orderObj.EarnestMoney = dr[18] == DBNull.Value ? 0 : (decimal)dr[18];
                        orderObj.Measurement = dr[19] == DBNull.Value ? 0 : (int)dr[19];
                        orderObj.IsDistribute = dr[20] == DBNull.Value ? false : (bool)dr[20];
                        orderObj.IsConsigned = dr[21] == DBNull.Value ? false : (bool)dr[21];
                        orderObj.Logistics = dr[22] == DBNull.Value ? null : (string)dr[22];
                        orderObj.LogisticsCode = dr[23] == DBNull.Value ? null : (string)dr[23];
                        orderObj.LogisticStatus = dr[24] == DBNull.Value ? 0 : (int)dr[24];
                        orderObj.SignIn = dr[25] == DBNull.Value ? false : (bool)dr[25];
                        orderObj.Score = dr[26] == DBNull.Value ? 0 : (int)dr[26];
                        orderObj.Comments = dr[27] == DBNull.Value ? null : (string)dr[27];

                        orderObj.CityName = dr[28] == DBNull.Value ? null : (string)dr[28];
                        orderObj.ProvinceName = dr[29] == DBNull.Value ? null : (string)dr[29];
                        orderObj.RegionName = dr[30] == DBNull.Value ? null : (string)dr[30];
                        orderObj.ProvinceID = dr[31] == DBNull.Value ? 0 : (int)dr[31];
                        orderObj.PayedTime = dr[32] == DBNull.Value ? DateTime.MinValue : (DateTime)dr[32];
                        orderObj.BuyerEmail = dr[33] == DBNull.Value ? null : (string)dr[33];
                        orderObj.ServiceMemo = dr[34] == DBNull.Value ? null : (string)dr[34];
                        orderObj.TransportType = dr[35] == DBNull.Value ? null : (string)dr[35];
                        orderObj.TradeNo = dr[36] == DBNull.Value ? null : (string)dr[36];
                        orderObj.ExpressID = dr[37] == DBNull.Value ? 0 : (int)dr[37];
                        orderObj.ExpressName = dr[38] == DBNull.Value ? null : (string)dr[38];
                        orderObj.ExpressUrl = dr[39] == DBNull.Value ? null : (string)dr[39];

                        return orderObj;
                    }
                    return null;
                }
            }
        }
        #endregion

        #region 根据编号获取订单商品清单
        public JsonArray GetOrderDetails(int orderId, int userId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@OrderID", orderId);
                helper.AddIntParameter("@UserID", userId);

                int regionID = helper.GetIntValue("select RegionID from Orders where OrderID=@OrderID", CommandType.Text);

                string sql = "select DetailID,a.OrderID,a.ProductID,a.Discount,OrignalPrice,a.Quantity,b.Name,b.Code,Url,Points,Express.ExpressID,ExpressName,b.Freight,b.Freight1 from OrderDetails a inner join Products b on a.ProductID=b.ProductID inner join Orders c on a.OrderID=c.OrderID left join (select PictureDesc,Url,a.ProductID as PID from ProductPictures b inner join (select ProductID,min(PictureID) as PictureID from ProductPictures group by ProductID) a on a.PictureID=b.PictureID) d on PID=a.ProductID left join Express on Express.ExpressID=c.ExpressID where UserID=@UserID and a.OrderID=@OrderID";

                var details = helper.GetJsonArray(sql, CommandType.Text);
                ProductDAL productDAL = new ProductDAL();
                int productID, qty;
                foreach (var detail in details)
                {
                    productID = (int)detail["ProductID"];
                    qty = (int)detail["Quantity"];
                    var freight = productDAL.GetProductFreight(productID, regionID);
                    detail["Freight"] = freight[0];
                    detail["Freight1"] = freight[1];
                }

                return details;
            }
        }
        #endregion

        #region 根据订单号查询订单ID
        public int GetOrderIDByCode(string orderCode)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@OrderCode", 50, orderCode);
                return helper.GetIntValue("select OrderID from Orders where OrderCode=@OrderCode", CommandType.Text);
            }
        }
        #endregion

        #region 支付订单
        public void PayOrder(int orderId, decimal payedAmount, string tradeNo, string buyerEmail)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@OrderID", orderId);
                helper.AddDecimalParameter("@PayedAmount", payedAmount);
                helper.AddDateTimeParameter("@PayedTime", DateTime.Now);
                helper.AddStringParameter("@TradeNo", 50, tradeNo);
                helper.AddStringParameter("@BuyerEmail", 50, buyerEmail);

                int payStatus = helper.GetIntValue("select PayStatus from Orders where OrderID=@OrderID", CommandType.Text);


                helper.ExecuteNonQuery("update Orders set PayedAmount=@PayedAmount,PayStatus=1,Status=1,TradeNo=@TradeNo,PayedTime=@PayedTime,BuyerEmail=@BuyerEmail where OrderID=@OrderID", CommandType.Text);

                if (payStatus != 1)
                {
                    helper.ExecuteNonQuery("update a set SellNum=SellNum+1 from Products a  inner join OrderDetails b on a.ProductID=b.ProductID where OrderID=@OrderID", CommandType.Text);

                    int userID = helper.GetIntValue("select UserID from Orders where OrderID=@OrderID", CommandType.Text);

                    helper.AddIntParameter("@UserID", userID);
                    int points = helper.GetIntValue("select Points from Users where UserID=@UserID", CommandType.Text);

                    points += helper.GetIntValue("select sum(Points*b.Quantity) from Orders a inner join OrderDetails b on a.OrderID=b.OrderID inner join Products c on b.ProductID=c.ProductID where a.OrderID=@OrderID", CommandType.Text);

                    helper.AddIntParameter("@Points", points);
                    helper.ExecuteNonQuery("update Users set Points=@Points where UserID=@UserID");
                }
                helper.CommitTran();
            }
        }
        #endregion

        #region 查询订单
        public JsonArray GetOrders(int userId, int orderId, string orderCode, int isSurvey, DateTime dtFrom, DateTime dtTo, int payStatus, int status, string receiver, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                StringBuilder where = new StringBuilder("1=1");
                if (userId > 0)
                {
                    helper.AddIntParameter("@UserID", userId);
                    where.Append(" and UserID=@UserID");
                }
                if (orderId > 0)
                {
                    helper.AddIntParameter("@OrderId", orderId);
                    where.Append(" and OrderId=@OrderId");
                }
                if (!string.IsNullOrEmpty(orderCode))
                {
                    helper.AddStringParameter("@OrderCode", 50, orderCode);
                    where.Append(" and OrderCode=@OrderCode");
                }
                if (dtFrom != DateTime.MinValue && dtTo != DateTime.MinValue)
                {
                    helper.AddDateTimeParameter("@DtFrom", dtFrom.Date);
                    helper.AddDateTimeParameter("@DtTo", dtTo.Date.AddDays(1).AddMilliseconds(-3));
                    where.Append(" and CreationTime between @DtFrom and @DtTo");
                }
                if (payStatus != -1)
                {
                    helper.AddIntParameter("@PayStatus", payStatus);
                    where.Append(" and PayStatus=@PayStatus");
                }
                if (status != -1)
                {
                    helper.AddIntParameter("@Status", status);
                    where.Append(" and Status=@Status");
                }
                if (!string.IsNullOrEmpty(receiver))
                {
                    helper.AddStringParameter("@Receiver", 20, receiver);
                    where.Append(" and Receiver like '%'+@Receiver+'%'");
                }
                if (isSurvey != -1)
                {
                    if (isSurvey == 0)
                    {
                        where.Append(" and (SurveyID=0 or SurveyID is null)");
                    }
                    else
                    {
                        where.Append(" and (SurveyID<>0 and SurveyID is not null)");
                    }
                }
                JsonArray orders = helper.GetJsonArray("a.OrderID", "a.OrderID,OrderCode,CreationTime,Status,PayType,PayStatus,a.Memo,UserID,Receiver,IP,a.CityID,a.RegionID,a.Address,Zip,a.Mobile,Phone,OriginalAmount,Amount,Freight,EarnestMoney,Measurement,IsDistribute,IsConsigned,Logistics,LogisticsCode,LogisticStatus,SignIn,Score,Comments,CityName,ProvinceName,RegionName,ServiceMemo,PayedTime,BuyerEmail,TransportType,TradeNo,SurveyID,f.ExpressID,ExpressName,ExpressUrl", "Orders a inner join City b on a.CityID=b.CityID inner join Province c on c.ProvinceID=b.ProvinceID left join Region d on d.RegionID=a.RegionID left join Survey e on a.OrderID=e.OrderID left join Express f on f.ExpressID=a.ExpressID", where.ToString(), page, pageSize, out total, "CreationTime");

                if (orders != null)
                {
                    string sql = "select DetailID,a.OrderID,a.ProductID,a.Discount,OrignalPrice,a.Quantity,b.Name,b.Code,Url,Points,b.Freight,b.Freight1,Express from OrderDetails a inner join Products b on a.ProductID=b.ProductID inner join Orders c on a.OrderID=c.OrderID left join (select PictureDesc,Url,a.ProductID as PID from ProductPictures b inner join (select ProductID,min(PictureID) as PictureID from ProductPictures group by ProductID) a on a.PictureID=b.PictureID) d on PID=a.ProductID where a.OrderID=@OrderID";

                    JsonObject orderObj;
                    helper.ClearParameters();
                    helper.AddIntParameter("@OrderID", 0);
                    ProductDAL productDAL = new ProductDAL();
                    int regionID, productID, qty;
                    for (int i = 0; i < orders.Count; i++)
                    {
                        orderObj = orders[i];
                        helper.SetParameter("@OrderID", (int)orderObj["OrderID"]);
                        regionID = (int)orderObj["RegionID"];

                        var details = helper.GetJsonArray(sql, CommandType.Text);
                        foreach (var detail in details)
                        {
                            productID = (int)detail["ProductID"];
                            qty = (int)detail["Quantity"];
                            var freight = productDAL.GetProductFreight(productID, regionID);
                            detail["Freight"] = freight[0];
                            detail["Freight1"] = freight[1];
                        }

                        orderObj["children"] = details;
                    }
                }

                return orders;
            }
        }
        #endregion

        #region 查询前台订单
        public JsonArray GetOrders(int userId, string orderCode, DateTime dtFrom, DateTime dtTo, int payStatus, int page, int pageSize, out int total)
        {
            using (helper = new SqlHelper())
            {
                StringBuilder where = new StringBuilder("1=1");
                if (userId > 0)
                {
                    helper.AddIntParameter("@UserID", userId);
                    where.Append(" and UserID=@UserID");
                }
                if (!string.IsNullOrEmpty(orderCode))
                {
                    helper.AddStringParameter("@OrderCode", 50, orderCode);
                    where.Append(" and OrderCode=@OrderCode");
                }
                if (dtFrom != DateTime.MinValue && dtTo != DateTime.MinValue)
                {
                    helper.AddDateTimeParameter("@DtFrom", dtFrom.Date);
                    helper.AddDateTimeParameter("@DtTo", dtTo.Date.AddDays(1).AddMilliseconds(-3));
                    where.Append(" and CreationTime between @DtFrom and @DtTo");
                }
                if (payStatus != -1)
                {
                    helper.AddIntParameter("@PayStatus", payStatus);
                    where.Append(" and PayStatus=@PayStatus");
                }
                return helper.GetJsonArray("OrderID", "OrderID,OrderCode,CreationTime,Status,PayType,PayStatus,Memo,UserID,Receiver,IP,a.CityID,a.RegionID,Address,Zip,Mobile,Phone,OriginalAmount,Amount,Freight,EarnestMoney,Measurement,IsDistribute,IsConsigned,Logistics,LogisticsCode,LogisticStatus,SignIn,Score,Comments,CityName,ProvinceName,RegionName,ServiceMemo,PayedTime,BuyerEmail,TransportType,TradeNo,a.ExpressID,ExpressName,ExpressUrl", "Orders a inner join City b on a.CityID=b.CityID inner join Province c on c.ProvinceID=b.ProvinceID left join Region d on d.RegionID=a.RegionID left join Express e on e.ExpressID=a.ExpressID", where.ToString(), page, pageSize, out total, "CreationTime");
            }
        }
        #endregion

        public void ModifyLogistic(OrderObj orderObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@OrderID", orderObj.OrderID);
                helper.AddStringParameter("@Logistics", 50, orderObj.Logistics);
                helper.AddStringParameter("@LogisticsCode", 50, orderObj.LogisticsCode);
                helper.AddBoolenParameter("@IsConsigned", orderObj.IsConsigned);
                helper.AddStringParameter("@TransportType", 50, orderObj.TransportType);
                helper.AddIntParameter("@Status", orderObj.Status);
                helper.AddIntParameter("@ExpressID", orderObj.ExpressID);

                string sql = "update Orders set IsConsigned=@IsConsigned,Logistics=@Logistics,LogisticsCode=@LogisticsCode,TransportType=@TransportType,Status=@Status,ExpressID=@ExpressID where OrderID=@OrderID";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }

        public void ModifyServiceMemo(OrderObj orderObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@OrderID", orderObj.OrderID);
                helper.AddStringParameter("@ServiceMemo", 2000, orderObj.ServiceMemo);

                string sql = "update Orders set ServiceMemo=@ServiceMemo where OrderID=@OrderID";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }

        #region 确认收货
        public void ConfirmGoods(int orderId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@OrderID", orderId);

                string sql = "update Orders set SignIn=1,Status=4 where OrderID=@OrderID";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        #region 等待用户确认收货
        public void WaitConfirmGoods(int orderId)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@OrderID", orderId);

                string sql = "update Orders set SignIn=1,Status=3 where OrderID=@OrderID";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }
        #endregion

        public void CancelOrder(OrderObj orderObj)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@OrderID", orderObj.OrderID);

                string sql = "update Orders set Status=2 where OrderID=@OrderID";
                helper.ExecuteNonQuery(sql, CommandType.Text);

                sql = "update a set a.Quantity=a.Quantity+b.Quantity from Products a inner join OrderDetails b on a.ProductID=b.ProductID where OrderID=@OrderID";
                helper.ExecuteNonQuery(sql, CommandType.Text);

                helper.CommitTran();
            }
        }

        #region 修改未支付的订单
        public void ModifyOrder(OrderObj orderObj, IDictionary<int, decimal> discount)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();

                helper.AddIntParameter("@DetailID", 0);
                helper.AddDecimalParameter("@Discount", 0);
                string sql = "update OrderDetails set Discount=@Discount where DetailID=@DetailID";

                foreach (KeyValuePair<int, decimal> item in discount)
                {
                    helper.SetParameter("@DetailID", item.Key);
                    helper.SetParameter("@Discount", item.Value);

                    helper.ExecuteNonQuery(sql, CommandType.Text);
                }
                helper.ClearParameters();

                helper.AddIntParameter("@OrderID", orderObj.OrderID);
                sql = "select sum(OrignalPrice*Quantity-Discount) from OrderDetails where OrderID=@OrderID";
                orderObj.Amount = helper.GetDecimalValue(sql, CommandType.Text);

                helper.AddDateTimeParameter("@CreationTime", orderObj.CreationTime);
                helper.AddStringParameter("@Memo", 2000, orderObj.Memo);
                helper.AddStringParameter("@Receiver", 50, orderObj.Receiver);
                helper.AddIntParameter("@CityID", orderObj.CityID);
                helper.AddIntParameter("@RegionID", orderObj.RegionID);
                helper.AddStringParameter("@Address", 400, orderObj.Address);
                helper.AddStringParameter("@Zip", 20, orderObj.Zip);
                helper.AddStringParameter("@Mobile", 50, orderObj.Mobile);
                helper.AddStringParameter("@Phone", 50, orderObj.Phone);
                helper.AddDecimalParameter("@Amount", orderObj.Amount);
                helper.AddDecimalParameter("@Freight", orderObj.Freight);

                sql = "update Orders set CreationTime=@CreationTime,Memo=@Memo,Receiver=@Receiver,CityID=@CityID,RegionID=@RegionID,Address=@Address,Zip=@Zip,Mobile=@Mobile,Phone=@Phone,Amount=@Amount,Freight=@Freight where OrderID=@OrderID";
                helper.ExecuteNonQuery(sql, CommandType.Text);

                helper.CommitTran();
            }
        }
        #endregion

        #region GetSurveyByID
        public SurveyObj GetSurveyByID(int orderID)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@OrderID", orderID);

                using (SqlDataReader dr = helper.ExecuteReader("select OrderID,SurveyLike,CustomerName,Gender,Mobile,Email,Survey.RegionID,Address,Type,SquareType,Options,Days,HopeTime,Pic,Memo,City.CityID,City.ProvinceID,HopeHour,RegionName,CityName,ProvinceName from Survey left join Region on Region.RegionID=Survey.RegionID left join City on Region.CityID=City.CityID left join Province on Province.ProvinceID=City.ProvinceID where OrderID=@OrderID", CommandType.Text))
                {
                    if (dr.HasRows && dr.Read())
                    {
                        SurveyObj surveyObj = new SurveyObj();
                        surveyObj.OrderID = dr[0] == DBNull.Value ? 0 : (int)dr[0];
                        surveyObj.SurveyLike = dr[1] == DBNull.Value ? 0 : (int)dr[1];
                        surveyObj.CustomerName = dr[2] == DBNull.Value ? null : (string)dr[2];
                        surveyObj.Gender = dr[3] == DBNull.Value ? false : (bool)dr[3];
                        surveyObj.Mobile = dr[4] == DBNull.Value ? null : (string)dr[4];
                        surveyObj.Email = dr[5] == DBNull.Value ? null : (string)dr[5];
                        surveyObj.RegionID = dr[6] == DBNull.Value ? 0 : (int)dr[6];
                        surveyObj.Address = dr[7] == DBNull.Value ? null : (string)dr[7];
                        surveyObj.Type = dr[8] == DBNull.Value ? 0 : (int)dr[8];
                        surveyObj.SquareType = dr[9] == DBNull.Value ? 0 : (int)dr[9];
                        surveyObj.Options = dr[10] == DBNull.Value ? null : (string)dr[10];
                        surveyObj.Days = dr[11] == DBNull.Value ? 0 : (int)dr[11];
                        surveyObj.HopeTime = dr[12] == DBNull.Value ? null : (string)dr[12];
                        surveyObj.Pic = dr[13] == DBNull.Value ? null : (string)dr[13];
                        surveyObj.Memo = dr[14] == DBNull.Value ? null : (string)dr[14];
                        surveyObj.CityID = dr[15] == DBNull.Value ? 0 : (int)dr[15];
                        surveyObj.ProvinceID = dr[16] == DBNull.Value ? 0 : (int)dr[16];
                        surveyObj.HopeHour = dr[17] == DBNull.Value ? 0 : (int)dr[17];
                        surveyObj.RegionName = dr[18] == DBNull.Value ? null : (string)dr[18];
                        surveyObj.CityName = dr[19] == DBNull.Value ? null : (string)dr[19];
                        surveyObj.ProvinceName = dr[20] == DBNull.Value ? null : (string)dr[20];

                        return surveyObj;
                    }
                    return null;
                }
            }
        }
        #endregion

        #region SetSurvey
        public void SetSurvey(SurveyObj surveyObj)
        {
            using (helper = new SqlHelper())
            {
                helper.AddIntParameter("@OrderID", surveyObj.OrderID);
                helper.AddIntParameter("@SurveyLike", surveyObj.SurveyLike);
                helper.AddStringParameter("@CustomerName", 50, surveyObj.CustomerName);
                helper.AddBoolenParameter("@Gender", surveyObj.Gender);
                helper.AddStringParameter("@Mobile", 50, surveyObj.Mobile);
                helper.AddStringParameter("@Email", 100, surveyObj.Email);
                helper.AddIntParameter("@RegionID", surveyObj.RegionID);
                helper.AddStringParameter("@Address", 400, surveyObj.Address);
                helper.AddIntParameter("@Type", surveyObj.Type);
                helper.AddIntParameter("@SquareType", surveyObj.SquareType);
                helper.AddStringParameter("@Options", 50, surveyObj.Options);
                helper.AddIntParameter("@Days", surveyObj.Days);
                helper.AddStringParameter("@HopeTime", 100, surveyObj.HopeTime);
                helper.AddStringParameter("@Pic", 200, surveyObj.Pic);
                helper.AddTextParameter("@Memo", surveyObj.Memo);
                helper.AddIntParameter("@HopeHour", surveyObj.HopeHour);

                helper.ExecuteNonQuery("if exists (select 1 from Survey where OrderID=@OrderID) update Survey set SurveyLike=@SurveyLike,CustomerName=@CustomerName,Gender=@Gender,Mobile=@Mobile,Email=@Email,RegionID=@RegionID,Address=@Address,Type=@Type,SquareType=@SquareType,Options=@Options,Days=@Days,HopeTime=@HopeTime,Pic=@Pic,Memo=@Memo,HopeHour=@HopeHour where OrderID=@OrderID else insert into Survey (OrderID,SurveyLike,CustomerName,Gender,Mobile,Email,RegionID,Address,Type,SquareType,Options,Days,HopeTime,Pic,Memo,HopeHour) values (@OrderID,@SurveyLike,@CustomerName,@Gender,@Mobile,@Email,@RegionID,@Address,@Type,@SquareType,@Options,@Days,@HopeTime,@Pic,@Memo,@HopeHour)", CommandType.Text);
            }
        }
        #endregion

        public void DeleteDetail(int orderId, int detailId)
        {
            using (helper = new SqlHelper())
            {
                helper.BeginTran();
                helper.AddIntParameter("@DetailID", detailId);
                helper.AddIntParameter("@OrderID", orderId);

                helper.ExecuteNonQuery("update a set a.Quantity=a.Quantity+b.Quantity from Products a inner join OrderDetails b on a.ProductID=b.ProductID where DetailID=@DetailID", CommandType.Text);

                helper.ExecuteNonQuery("delete from OrderDetails where OrderID=@OrderID and DetailID=@DetailID", CommandType.Text);

                int count = helper.GetIntValue("select count(1) from OrderDetails where OrderID=@OrderID", CommandType.Text);
                if (count == 0)
                {
                    helper.ExecuteNonQuery("delete from Orders where OrderID=@OrderID", CommandType.Text);
                }
                else
                {
                    decimal amount = helper.GetDecimalValue("select sum(OrignalPrice*Quantity-Discount) from OrderDetails where OrderID=@OrderID", CommandType.Text);
                    helper.AddDecimalParameter("@Amount", amount);
                    helper.AddDecimalParameter("@Freight", amount);
                    helper.ExecuteNonQuery("update Orders set Amount=@Amount where OrderID=@OrderID", CommandType.Text);
                }
                helper.CommitTran();
            }
        }
    }
}
