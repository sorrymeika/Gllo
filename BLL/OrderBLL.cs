using System;
using System.Collections.Generic;
using System.Text;

using Gllo.Models;

namespace Gllo.BLL
{
    public class OrderBLL
    {
        private readonly Gllo.DAL.OrderDAL dal = new DAL.OrderDAL();

        #region 添加订单
        public int AddOrder(OrderObj orderObj)
        {
            return dal.AddOrder(orderObj);
        }
        #endregion

        #region 根据编号获取订单信息
        public OrderObj GetOrderByID(int orderId)
        {
            return dal.GetOrderByID(orderId);
        }
        #endregion

        #region 根据编号获取订单商品清单
        public JsonArray GetOrderDetails(int orderId, int userId)
        {
            return dal.GetOrderDetails(orderId, userId);
        }
        #endregion

        #region 根据订单号查询订单ID
        public int GetOrderIDByCode(string orderCode)
        {
            return dal.GetOrderIDByCode(orderCode);
        }
        #endregion

        #region 支付订单
        public void PayOrder(int orderId, decimal payedAmount, string tradeNo, string buyerEmail)
        {
            dal.PayOrder(orderId, payedAmount, tradeNo, buyerEmail);
        }
        #endregion

        #region 查询订单
        public JsonArray GetOrders(int userId, int orderId, string orderCode, int isSurvey, DateTime dtFrom, DateTime dtTo, int payStatus, int status, string receiver, int page, int pageSize, out int total)
        {
            return dal.GetOrders(userId, orderId, orderCode, isSurvey, dtFrom, dtTo, payStatus, status, receiver, page, pageSize, out  total);
        }
        #endregion

        #region 查询前台订单
        public JsonArray GetOrders(int userId, string orderCode, DateTime dtFrom, DateTime dtTo, int payStatus, int page, int pageSize, out int total)
        {
            return dal.GetOrders(userId, orderCode, dtFrom, dtTo, payStatus, page, pageSize, out  total);
        }
        #endregion

        #region 修改未支付的订单
        public void ModifyOrder(OrderObj orderObj, IDictionary<int, decimal> perf)
        {
            dal.ModifyOrder(orderObj, perf);
        }
        #endregion

        public void ModifyLogistic(OrderObj orderObj)
        {
            dal.ModifyLogistic(orderObj);

        }

        public void ModifyServiceMemo(OrderObj orderObj)
        {
            dal.ModifyServiceMemo(orderObj);
        }

        public void CancelOrder(OrderObj orderObj)
        {
            dal.CancelOrder(orderObj);

        }

        #region 等待用户确认收货
        public void WaitConfirmGoods(int orderId)
        {
            dal.WaitConfirmGoods(orderId);

        }
        #endregion

        #region 确认收货
        public void ConfirmGoods(int orderId)
        {
            dal.ConfirmGoods(orderId);
        }
        #endregion

        #region GetSurveyByID
        public SurveyObj GetSurveyByID(int orderID)
        {
            return dal.GetSurveyByID(orderID);
        }
        #endregion

        #region SetSurvey
        public void SetSurvey(SurveyObj surveyObj)
        {
            dal.SetSurvey(surveyObj);
        }
        #endregion

        #region DeleteDetail
        public void DeleteDetail(int orderId, int detailId)
        {
            dal.DeleteDetail(orderId, detailId);
        }
        #endregion
    }
}
