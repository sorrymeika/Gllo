using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Gllo.Sys;
using Gllo.Models;
using System.Text.RegularExpressions;

namespace Gllo.DAL
{
    internal class SqlHelper : IDisposable
    {
        private bool autoCloseConnection;
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlTransaction tran;
        private static readonly IDictionary<bool, string> ascOrDesc = new Dictionary<bool, string>() { 
            { true, "asc" },
            { false, "desc" }
        };

        public bool AutoCloseConnection
        {
            get { return autoCloseConnection; }
            set { autoCloseConnection = value; }
        }

        public SqlHelper()
            : this(Config.ConnectionString)
        {
        }

        public SqlHelper(string connectionString, bool autoCloseConnection = false)
        {
            this.autoCloseConnection = autoCloseConnection;
            conn = new SqlConnection(connectionString);
            cmd = new SqlCommand();
            cmd.Connection = conn;
        }

        public void BeginTran()
        {
            this.OpenConnection();

            tran = conn.BeginTransaction();
            cmd.Transaction = tran;
        }

        public void CommitTran()
        {
            tran.Commit();
        }

        public void RollbackTran()
        {
            tran.Rollback();
        }

        public void ClearParameters()
        {
            this.cmd.Parameters.Clear();
        }

        public SqlParameter AddTinyIntParameter(string parameterName, byte value)
        {
            return AddParameter(parameterName, SqlDbType.TinyInt, value);
        }

        public SqlParameter AddIntParameter(string parameterName, int value)
        {
            return AddParameter(parameterName, SqlDbType.Int, value);
        }

        public SqlParameter AddTextParameter(string parameterName, string value)
        {
            return AddParameter(parameterName, SqlDbType.Text, value != null ? (object)value : DBNull.Value);
        }

        public SqlParameter AddBoolenParameter(string parameterName, bool value)
        {
            return AddParameter(parameterName, SqlDbType.Bit, value);
        }

        public SqlParameter AddDateTimeParameter(string parameterName, DateTime value)
        {
            return AddParameter(parameterName, SqlDbType.DateTime, value == DateTime.MinValue ? DBNull.Value : (object)value);
        }

        public SqlParameter AddDecimalParameter(string parameterName, Decimal value)
        {
            return AddParameter(parameterName, SqlDbType.Decimal, value);
        }

        public SqlParameter AddStringParameter(string parameterName, int size, string value)
        {
            SqlParameter param = new SqlParameter(parameterName, SqlDbType.VarChar, size);
            param.Value = value != null ? (object)value : DBNull.Value;
            cmd.Parameters.Add(param);
            return param;
        }

        public SqlParameter AddOutputParameter(string parameterName)
        {
            SqlParameter param = new SqlParameter(parameterName, SqlDbType.Int);
            param.Direction = ParameterDirection.Output;
            cmd.Parameters.Add(param);
            return param;
        }

        public SqlParameter AddReturnValueParameter(SqlDbType sqlDbType)
        {
            SqlParameter param = new SqlParameter("@ReturnValue", SqlDbType.Int);
            param.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(param);
            return param;
        }

        public SqlParameter AddParameter(string parameterName, SqlDbType dbType, object value)
        {
            SqlParameter param = new SqlParameter(parameterName, dbType);
            cmd.Parameters.Add(param);
            param.Value = value;
            return param;
        }

        public SqlParameter AddParameter(SqlParameter p)
        {
            return cmd.Parameters.Add(p);
        }

        public void SetParameter(string parameterName, object value)
        {
            cmd.Parameters[parameterName].Value = value;
        }

        public int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text)
        {
            this.OpenConnection();
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;
            int res = cmd.ExecuteNonQuery();

            if (autoCloseConnection)
                conn.Close();

            return res;
        }

        public SqlDataReader ExecuteReader(string pk, string columns, string table, string where, int page, int pageSize, out int total, string sort = null, bool isAsc = false)
        {
            if (string.IsNullOrEmpty(where))
                where = "1=1";

            if (string.IsNullOrEmpty(sort))
                sort = pk;

            IDictionary<string, string> conditions = new Dictionary<string, string>() { 
                {"columns", columns},
                {"table", table},
                {"sort", sort},
                {"where", where},
                {"asc", ascOrDesc[isAsc]}
            };

            total = this.GetIntValue(formatString("select count(1) from {table} where {where}", conditions), CommandType.Text);

            if (total != 0)
            {
                string sql;
                if (page == 1)
                {
                    conditions.Add("top", pageSize.ToString());
                    sql = formatString("select top {top} {columns} from {table} where {where} order by {sort} {asc}", conditions);
                }
                else
                {
                    int max = pageSize * page;
                    int top = max <= total ? pageSize : (total - max + pageSize);

                    conditions.Add("needs", Regex.Replace(columns, @"(^|(?<=,))\s*([^\.,]+\.|[^,]+?\s+as\s+)", ""));
                    conditions.Add("top", top.ToString());
                    conditions.Add("pk", pk);
                    conditions.Add("max", max.ToString());
                    conditions.Add("desc", ascOrDesc[!isAsc]);

                    sql = formatString("select {needs} from (select top {top} _pk_field,_sort_field from (select top {max} {pk} as _pk_field,{sort} as _sort_field from {table} where {where} order by _sort_field {asc}) a order by _sort_field {desc}) a inner join (select {pk} as _pk_field,{columns} from {table}) b on a._pk_field=b._pk_field order by _sort_field {asc}", conditions);
                }

                return this.ExecuteReader(sql, CommandType.Text);
            }

            return null;
        }

        public SqlDataReader ExecuteReader(string[] pks, string columns, string table, string where, int page, int pageSize, out int total, IDictionary<string, bool> sorts = null)
        {
            if (string.IsNullOrEmpty(where))
                where = "1=1";

            IDictionary<string, string> conditions = new Dictionary<string, string>() { 
                {"table", table},
                {"where", where}
            };

            total = this.GetIntValue(formatString("select count(1) from {table} where {where}", conditions), CommandType.Text);

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
                    conditions.Add("needs", Regex.Replace(columns, @"(^|(?<=,))\s*([^\.,]+\.|[^,]+?\s+as\s+)", ""));

                    sql = formatString("select {needs} from (select top {top} {pkFields},{sortFields} from (select top {max} {pkAs},{sortAs} from {table} where {where} {sortAsc}) a {sortDesc}) a inner join (select {pkAs},{columns} from {table}) b on {join} {sortAsc}", conditions);
                }

                return this.ExecuteReader(sql, CommandType.Text);
            }

            return null;
        }

        private JsonArray GetJsonArray(SqlDataReader dr, IDictionary<string, string> columnMappings)
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

        public JsonArray GetJsonArray(string sql, CommandType commandType, IDictionary<string, string> columnMappings = null)
        {
            using (SqlDataReader dr = this.ExecuteReader(sql, CommandType.Text))
            {
                return this.GetJsonArray(dr, columnMappings);
            }
        }

        public JsonArray GetJsonArray(string[] pks, string columns, string table, string where, int page, int pageSize, out int total, IDictionary<string, bool> sorts = null, IDictionary<string, string> columnMappings = null)
        {
            SqlDataReader dr = this.ExecuteReader(pks, columns, table, where, page, pageSize, out total, sorts);
            if (dr != null)
            {
                using (dr)
                {
                    return GetJsonArray(dr, columnMappings);
                }
            }
            return null;
        }

        public JsonArray GetJsonArray(string pk, string columns, string table, string where, int page, int pageSize, out int total, string sort = null, bool isAsc = false, IDictionary<string, string> columnMappings = null)
        {
            SqlDataReader dr = this.ExecuteReader(pk, columns, table, where, page, pageSize, out total, sort, isAsc);
            if (dr != null)
            {
                using (dr)
                {
                    return GetJsonArray(dr, columnMappings);
                }
            }
            return null;
        }

        public void Read(string commandText, CommandType commandType, Action<SqlDataReader> read)
        {
            using (SqlDataReader dr = ExecuteReader(commandText, commandType))
            {
                read(dr);
            }
        }

        public SqlDataReader ExecuteReader(string commandText, CommandType commandType)
        {
            this.OpenConnection();
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;
            return cmd.ExecuteReader();
        }

        public object ExecuteScalar(string commandText, CommandType commandType)
        {
            this.OpenConnection();
            cmd.CommandText = commandText;
            cmd.CommandType = commandType;
            object obj = cmd.ExecuteScalar();

            if (autoCloseConnection)
                conn.Close();
            if (obj == DBNull.Value)
            {
                return null;
            }
            return obj;
        }

        public int GetIntValue(string commandText, CommandType commandType)
        {
            object obj = this.ExecuteScalar(commandText, commandType);
            return obj == null ? 0 : Convert.ToInt32(obj);
        }

        public bool GetBooleanValue(string commandText, CommandType commandType)
        {
            object obj = this.ExecuteScalar(commandText, commandType);
            return obj == null ? false : Convert.ToBoolean(obj);
        }

        public string GetStringValue(string commandText, CommandType commandType)
        {
            object obj = this.ExecuteScalar(commandText, commandType);
            return obj == null ? "" : (string)obj;
        }

        public Decimal GetDecimalValue(string commandText, CommandType commandType)
        {
            object obj = this.ExecuteScalar(commandText, commandType);
            return obj == null ? 0 : Convert.ToDecimal(obj);
        }

        public DateTime GetDateTimeValue(string commandText, CommandType commandType)
        {
            object obj = this.ExecuteScalar(commandText, commandType);
            return obj == null ? DateTime.MinValue : (DateTime)obj;
        }

        private void OpenConnection()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
        }

        public void Close()
        {
            if (cmd != null)
            {
                if (conn != null)
                {
                    conn.Dispose();
                    conn = null;
                }
                cmd.Dispose();
                cmd = null;
            }
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Close();
                if (tran != null)
                {
                    tran.Dispose();
                    tran = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
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
