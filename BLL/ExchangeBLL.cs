using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gllo.Models;

namespace Gllo.BLL
{
    public class ExchangeBLL
    {
        private readonly Gllo.DAL.Exchange dal = new DAL.Exchange();

        public JsonArray GetExchange(int page, int pageSize, out int total,
            int? userID = null,
            int? orderID = null,
            string orderCode = null,
            string userName = null,
            DateTime? addTimeFrom = null,
            DateTime? addTimeTo = null)
        {
            return dal.GetExchange(page, pageSize, out total, userID, orderID, orderCode, userName, addTimeFrom, addTimeTo);
        }

        public IList<Tuple<ExchangeInfo, OrderObj, ProductObj, ProductObj>> GetExchange(int userID, int? orderID = null)
        {
            return dal.GetExchange(userID, orderID);
        }

        public ExchangeInfo GetExchangeByID(int exchangeID)
        {
            return dal.GetExchangeByID(exchangeID);
        }

        public void AddExchange(ExchangeInfo exchangeInfo)
        {
            dal.AddExchange(exchangeInfo);
        }

        public void UpdateExchange(ExchangeInfo exchangeInfo)
        {
            dal.UpdateExchange(exchangeInfo);
        }

        public void DeleteExchange(int exchangeID)
        {
            dal.DeleteExchange(exchangeID);
        }

        public bool SubmitChanges()
        {
            return dal.SubmitChanges();
        }

        public int GetReturnQty(int productId, int orderId)
        {
            return dal.GetReturnQty(productId, orderId);
        }
    }
}
