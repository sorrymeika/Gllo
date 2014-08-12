using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Gllo.DAL
{
    public class SettingDAL
    {
        SqlHelper helper;

        public void Set(string key, string value)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@SettingKey", 50, key);
                helper.AddStringParameter("@SettingValue", 200, value);
                string sql = "if exists (select 1 from Settings where SettingKey=@SettingKey) update Settings set SettingValue=@SettingValue where SettingKey=@SettingKey else insert into Settings (SettingKey,SettingValue) values (@SettingKey,@SettingValue)";
                helper.ExecuteNonQuery(sql, CommandType.Text);
            }
        }

        public string Get(string key)
        {
            using (helper = new SqlHelper())
            {
                helper.AddStringParameter("@SettingKey", 50, key);
                return helper.GetStringValue("select SettingValue from Settings where SettingKey=@SettingKey", CommandType.Text);
            }
        }

    }
}
