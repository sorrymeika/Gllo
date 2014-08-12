using System;
using System.Collections.Generic;
using System.Text;
using Gllo.Models;

namespace Gllo.BLL
{
    public class ExpressBLL
    {
        Gllo.DAL.ExpressDAL dal = new DAL.ExpressDAL();

        #region 添加快递方式
        public void AddExpress(ExpressObj expressObj)
        {
            dal.AddExpress(expressObj);
        }
        #endregion

        #region 修改快递方式
        public void ModifyExpress(ExpressObj expressObj)
        {
            dal.ModifyExpress(expressObj);

        }
        #endregion

        #region 删除快递方式
        public void DeleteExpress(int expressId, bool realDelete=true)
        {
            dal.DeleteExpress(expressId, realDelete);
        }
        #endregion

        #region 获取快递方式
        public IList<ExpressObj> GetExpress()
        {
            return dal.GetExpress();
        }
        #endregion

        #region 根据编号获取快递方式
        public ExpressObj GetExpress(int expressId)
        {
            return dal.GetExpress(expressId);
        }
        #endregion
    }
}
