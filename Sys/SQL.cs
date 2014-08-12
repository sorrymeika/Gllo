using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Gllo.Models;
using System.Text.RegularExpressions;

namespace Gllo.Sys
{
    public class SQL
    {
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlTransaction tran;
        private SqlParameter param;
        private IList<dynamic> results;
        private dynamic result;
        private bool hasOutput;
        private SqlDataReader dr;
        private static readonly IDictionary<bool, string> ascOrDesc = new Dictionary<bool, string>() { 
            { true, "asc" },
            { false, "desc" }
        };

        public SQL()
            : this(Config.ConnectionString)
        {
        }

        public SQL(string connectionString)
        {
            results = new List<dynamic>();
            hasOutput = false;
            conn = new SqlConnection(connectionString);
            cmd = new SqlCommand();
            cmd.CommandType = CommandType.Text;
            cmd.Connection = conn;
        }

        public SQL Add(string parameterName, SqlDbType dbType, object value)
        {
            param = new SqlParameter(parameterName, dbType);
            cmd.Parameters.Add(param);
            param.Value = value;
            return this;
        }

        public SQL Add(string parameterName, byte value)
        {
            return Add(parameterName, SqlDbType.TinyInt, value);
        }

        public SQL Add(string parameterName, int value)
        {
            return Add(parameterName, SqlDbType.Int, value);
        }

        public SQL Add(string parameterName, string value)
        {
            return Add(parameterName, SqlDbType.Text, value);
        }

        public SQL Add(string parameterName, bool value)
        {
            return Add(parameterName, SqlDbType.Bit, value);
        }

        public SQL Add(string parameterName, DateTime value)
        {
            return Add(parameterName, SqlDbType.DateTime, value == DateTime.MinValue ? DBNull.Value : (object)value);
        }

        public SQL Add(string parameterName, Decimal value)
        {
            return Add(parameterName, SqlDbType.Decimal, value);
        }

        public SQL Add(string parameterName, int size, string value)
        {
            param = new SqlParameter(parameterName, SqlDbType.VarChar, size);
            param.Value = value != null ? (object)value : DBNull.Value;
            cmd.Parameters.Add(param);
            return this;
        }

        public SQL Output(string parameterName)
        {
            hasOutput = true;
            param = new SqlParameter(parameterName, SqlDbType.Int);
            param.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(param);
            return this;
        }

        public SQL Return(SqlDbType sqlDbType)
        {
            param = new SqlParameter("@ReturnValue", SqlDbType.Int);
            param.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(param);
            return this;
        }

        public object ReturnValue()
        {
            return cmd.Parameters["@ReturnValue"].Value;
        }

        public SQL Set(string parameterName, object value)
        {
            cmd.Parameters[parameterName].Value = value;
            return this;
        }

        private SQL clear()
        {
            cmd.Parameters.Clear();
            hasOutput = false;
            return this;
        }

        public SQL Begin()
        {
            this.open();

            tran = conn.BeginTransaction();
            cmd.Transaction = tran;

            return this;
        }

        public SQL Commit()
        {
            tran.Commit();
            return this;
        }

        public SQL Rollback()
        {
            tran.Rollback();
            return this;
        }

        public SQL Excute(string sql)
        {
            open();
            cmd.CommandText = sql;
            result = cmd.ExecuteNonQuery();
            results.Add(result);
            if (hasOutput)
            {
                for (int i = 0; i < cmd.Parameters.Count; i++)
                {
                    if (cmd.Parameters[i].Direction == ParameterDirection.Output)
                    {
                        result = cmd.Parameters[i].Value;
                        results.Add(result);
                    }
                }
            }
            return clear();
        }

        public object GetObject(string sql)
        {
            open();
            cmd.CommandText = sql;
            object obj = cmd.ExecuteScalar();
            cmd.Parameters.Clear();

            if (obj == DBNull.Value)
                return null;

            return obj;
        }

        public int GetInt(string sql)
        {
            object obj = this.GetObject(sql);
            return obj == null ? 0 : Convert.ToInt32(obj);
        }

        public SQL Get(string sql)
        {
            return addResult(GetObject(sql));
        }

        private JsonArray Json(IDictionary<string, string> columnMappings)
        {
            if (dr.HasRows)
            {
                JsonArray jsonArray = new JsonArray();
                JsonObject json;
                string columnName;
                Type type;
                while (dr.Read())
                {
                    json = new JsonObject();
                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        columnName = dr.GetName(i);
                        columnName = columnMappings == null || !columnMappings.ContainsKey(columnName) ? columnName : columnMappings[columnName];
                        type = dr.GetFieldType(i);
                        if (type == typeof(DateTime))
                        {
                            json.Add(columnName, dr[i] == DBNull.Value ? null : dr.GetDateTime(i).ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        else if (dr[i] == DBNull.Value)
                        {
                            if (type == typeof(bool))
                                json.Add(columnName, false);
                            else if (type.IsValueType)
                                json.Add(columnName, 0);
                            else
                                json.Add(columnName, null);
                        }
                        else
                        {
                            json.Add(columnName, dr[i]);
                        }
                    }
                    jsonArray.Add(json);
                }

                return jsonArray;
            }
            return null;
        }

        private SqlDataReader Read(string sql)
        {
            this.open();
            cmd.CommandText = sql;
            return cmd.ExecuteReader();
        }

        public SQL Json(string sql, IDictionary<string, string> columnMappings = null)
        {
            using (dr = this.Read(sql))
            {
                return addResult(this.Json(columnMappings));
            }
        }

        public SQL Json(string pk, string columns, string table, string where, int page, int pageSize, out int total, string sorts = null, bool isAsc = true, IDictionary<string, string> mappings = null)
        {
            return Json(new string[] { pk }, columns, table, where, page, pageSize, out total, sorts == null ? null : new Dictionary<string, bool>() { { sorts, isAsc } }, mappings);
        }

        public SQL Json(string pk, string columns, string table, string where, int page, int pageSize, out int total, IDictionary<string, bool> sorts = null, IDictionary<string, string> mappings = null)
        {
            return Json(new string[] { pk }, columns, table, where, page, pageSize, out total, sorts, mappings);
        }

        public SQL Json(string[] pks, string columns, string table, string where, int page, int pageSize, out int total, IDictionary<string, bool> sorts = null, IDictionary<string, string> mappings = null)
        {
            if (string.IsNullOrEmpty(where))
                where = "1=1";

            IDictionary<string, string> conditions = new Dictionary<string, string>() { 
                {"table", table},
                {"where", where}
            };

            total = this.GetInt(formatString("select count(1) from {table} where {where}", conditions));

            if (total != 0)
            {
                StringBuilder sbSortFields = new StringBuilder();
                StringBuilder sbSortAs = new StringBuilder();
                StringBuilder sbSortAsc = new StringBuilder(" order by ");
                StringBuilder sbSortDesc = new StringBuilder(" order by ");
                StringBuilder sbSortOrg = new StringBuilder(" order by ");
                if (sorts != null && sorts.Count != 0)
                {
                    int i = 0;
                    foreach (var sort in sorts)
                    {
                        if (i != 0)
                        {
                            sbSortAs.Append(",");
                            sbSortFields.Append(",");
                            sbSortAsc.Append(",");
                            sbSortDesc.Append(",");
                            sbSortOrg.Append(",");
                        }
                        sbSortFields.Append("_sort_field")
                            .Append(i.ToString());

                        sbSortAs.Append(sort.Key)
                            .Append(" as _sort_field")
                            .Append(i.ToString());

                        sbSortAsc.Append("_sort_field")
                            .Append(i.ToString())
                            .Append(" ")
                            .Append(ascOrDesc[sort.Value]);

                        sbSortDesc.Append("_sort_field")
                            .Append(i.ToString())
                            .Append(" ")
                            .Append(ascOrDesc[!sort.Value]);

                        sbSortOrg.Append(sort.Key)
                            .Append(" ")
                            .Append(ascOrDesc[sort.Value]);

                        i++;
                    }
                }
                else
                {
                    sbSortAsc.Append(pks[0])
                        .Append(ascOrDesc[true]);

                    sbSortDesc.Append(pks[0])
                        .Append(ascOrDesc[false]);
                }

                conditions.Add("sortOrg", sbSortOrg.ToString());
                conditions.Add("columns", columns);

                string sql;
                if (page == 1)
                {
                    conditions.Add("top", pageSize.ToString());
                    sql = formatString("select top {top} {columns} from {table} where {where} {sortOrg}", conditions);
                }
                else
                {
                    int max = pageSize * page;
                    int top = max <= total ? pageSize : (total - max + pageSize);

                    StringBuilder sbPK = new StringBuilder();
                    StringBuilder sbPKFields = new StringBuilder();
                    StringBuilder sbJoin = new StringBuilder();
                    string tmp;
                    for (int i = 0; i < pks.Length; i++)
                    {
                        if (i != 0)
                        {
                            sbPK.Append(",");
                            sbPKFields.Append(",");
                            sbPKFields.Append(" and");
                        }
                        tmp = "_pk_field" + i.ToString();
                        sbPK.Append(pks[i])
                            .Append(" as ")
                            .Append(tmp);

                        sbPKFields.Append(tmp);

                        sbJoin.Append(" a.")
                            .Append(tmp)
                            .Append("=b.")
                            .Append(tmp);
                    }

                    conditions.Add("top", top.ToString());
                    conditions.Add("max", max.ToString());
                    conditions.Add("pkAs", sbPK.ToString());
                    conditions.Add("sortAs", sbSortAs.ToString());
                    conditions.Add("pkFields", sbPKFields.ToString());
                    conditions.Add("sortFields", sbSortFields.ToString());
                    conditions.Add("sortAsc", sbSortAsc.ToString());
                    conditions.Add("sortDesc", sbSortDesc.ToString());
                    conditions.Add("join", sbJoin.ToString());
                    conditions.Add("needs", Regex.Replace(columns, @",{0,1}\s*[^\.,]+\.", ","));

                    sql = formatString("select {needs} from (select top {top} {pkFields},{sortFields} from (select top {max} {pkAs},{sortAs} from {table} where {where} {sortAsc}) a {sortDesc}) a inner join (select {pkAs},{columns} from {table}) b on {join} {sortAsc}", conditions);

                    return Json(sql, mappings);
                }
            }
            return addResult(null);
        }

        private SQL addResult(dynamic res)
        {
            result = res;
            results.Add(result);
            return this.clear();
        }

        public dynamic Result
        {
            get { return result; }
        }

        public IList<dynamic> Results
        {
            get { return results; }
        }

        private void open()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
        }

        public SQL Close()
        {
            if (cmd != null)
            {
                if (conn != null)
                {
                    if (dr != null)
                    {
                        dr.Dispose();
                        dr = null;
                    }
                    conn.Dispose();
                    conn = null;
                }
                cmd.Dispose();
                cmd = null;
            }
            return this;
        }

        private string formatString(string oriString, IDictionary<string, string> format)
        {
            foreach (KeyValuePair<string, string> kp in format)
            {
                oriString = oriString.Replace("{" + kp.Key + "}", kp.Value);
            }
            return oriString;
        }

    }
}
