using System;
using System.Collections.Generic;
using System.Text;

namespace Gllo.BLL
{
    public class SettingBLL
    {
        Gllo.DAL.SettingDAL dal = new DAL.SettingDAL();

        public void Set(string key, string value)
        {
            dal.Set(key, value);
        }

        public string Get(string key)
        {
            return dal.Get(key);
        }
    }
}
