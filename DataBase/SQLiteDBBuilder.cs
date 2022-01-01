using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Utilities.Database
{
    /// <summary>
    /// SQLite implementation
    /// </summary>
    public class SQLiteDBBuilder : AbstractDBBuilder
    {
        public SQLiteDBBuilder(DBFactory parent) : base(parent)
        {
        }
        SQLiteBaseTableToDatasetConverter tableToDataSetConverter = null;
        public override BaseTableToDatasetConverter GetTableToDatasetConverter()
        {
            if (tableToDataSetConverter == null)
            {
                tableToDataSetConverter = new SQLiteBaseTableToDatasetConverter();
            }
            return tableToDataSetConverter;
        }
        public override void Close(IDbConnection cn, bool forceClose = false)
        {
            if (forceClose)
            {
                try
                {
                    ((SQLiteConnection)cn).Close();
                }
                catch (Exception ee)
                {
                    Tracer.D(ee.ToString());
                }
            }
        }
        public override DataTable GetSchemaTables(IDbConnection cn)
        {
            DataTable schemaTable = ((SQLiteConnection)cn).GetSchema("Tables");
            return schemaTable;
        }
        public override bool IsTable(IDbConnection cn, String tableName)
        {
            DataTable schemaTable = GetSchemaTables(cn);

            foreach (DataRow row in schemaTable.Rows)
            {
                if (row[2].ToString().Equals(tableName))
                {
                    return true;
                }
            }
            return false;
        }
        public override DataTable GetViews(IDbConnection cn)
        {
            return ((SQLiteConnection)cn).GetSchema("Views");
        }
        public override DataTable GetTables(IDbConnection cn)
        {
            return ((SQLiteConnection)cn).GetSchema("Tables");
        }

        public override IDbCommand GetCommand()
        {
            return GetCommand(null, null, null);
        }
        public override IDbCommand GetCommand(String command, IDbConnection cn)
        {
            return GetCommand(command, cn, null);
        }
        public override IDbCommand GetCommand(String command, IDbConnection cn, Dictionary<string, string> paras)
        {
            SQLiteCommand ret = new SQLiteCommand();
            if (command != null)
            {
                ret.CommandText = command;
            }
            if (cn != null)
            {
                ret.Connection = (SQLiteConnection)cn;
            }
            if (paras != null)
            {
                foreach (KeyValuePair<string, string> kvp in paras)
                {
                    AddParamWithValue(ret.Parameters, kvp.Key, kvp.Value);
                }
            }
            return ret;
        }
        public override DbParameter CreateStringParameter(String name, int len, String sourceColumn)
        {
            SQLiteParameter ret = new SQLiteParameter(name, DbType.String);
            ret.Size = len;
            ret.SourceColumn = sourceColumn;
            return ret;
        }

        public override void BulkCopy(string tableName, DataTable oTable, IDbConnection cAccess)
        {
            IDbTransaction tr = cAccess.BeginTransaction();
            string strCreateColumns = "";
            string strColumnList = "";
            string strQuestionList = "";
            foreach (DataColumn oColumn in oTable.Columns)
            {
                strCreateColumns += "\"" + oColumn.ColumnName + "\" " + this.GetTableToDatasetConverter().GetColumnTypeString(oColumn) + (oColumn.AllowDBNull ? " NULL" : "") + ", ";
                strColumnList += "[" + oColumn.ColumnName + "],";
                strQuestionList += "?,";
            }
            strColumnList = strColumnList.Remove(strColumnList.Length - 1);

            foreach (DataRow row in oTable.Rows)
            {
                String filteredColumnList = strColumnList;
                String valueList = "";
                List<String> addValueList = new List<string>();
                List<String> addValueColList = new List<string>();
                foreach (DataColumn oColumn in oTable.Columns)
                {
                    String columnType = this.GetTableToDatasetConverter().GetColumnTypeString(oColumn);
                    bool isString = "TEXT".Equals(columnType);
                    object obj = row[oColumn];
                    bool isDbNull = false;
                    if (obj == null || obj is DBNull)
                    {
                        if (oColumn.AllowDBNull)
                        {
                            continue;
                        }
                    }
                    addValueColList.Add(oColumn.ColumnName);
                    if (isString)
                    {
                        String val = row[oColumn].ToString();
                        if (val.IndexOf('\'') > -1)
                        {
                            val = val.Replace("'", "''");
                        }
                        addValueList.Add("'" + val + "'");
                    }
                    else
                    {

                        String val = row[oColumn].ToString();

                        if ("DATETIME".Equals(columnType))
                        {
                            if (isDbNull)
                            {
                                val = "datetime('now')";
                            }
                            else
                            {
                                if (obj is DateTime)
                                {
                                    DateTime dt = (DateTime)obj;
                                    val = "'" + dt.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                }
                            }
                        }
                        else
                        {
                            if (isDbNull)
                            {
                                val = "0";
                            }
                        }
                        addValueList.Add(val);
                    }
                }
                if (addValueList.Count == 0)
                {
                    continue;
                }
                valueList = String.Join(",", addValueList);
                String cmd = "INSERT INTO \"" + oTable.TableName + "\" (" + String.Join(",", addValueColList)
                + ") VALUES (" + valueList + ")";
                using (var cmdObj = GetCommand(cmd, cAccess))
                {
                    cmdObj.Transaction = tr;
                    cmdObj.ExecuteNonQuery();
                }

            }
            tr.Commit();
            tr.Dispose();
        }
        public override void SetDataAdapterLoadFillOption(IDbDataAdapter adapter, LoadOption option)
        {
            SQLiteDataAdapter oleAdapter = (SQLiteDataAdapter)adapter;
            oleAdapter.FillLoadOption = option;
        }
        public override IDbDataAdapter GetDataAdapter(string command, string connection)
        {
            return new SQLiteDataAdapter(command, connection);
        }
        public override IDbDataAdapter GetDataAdapter(IDbCommand cmd)
        {
            return new SQLiteDataAdapter((SQLiteCommand)cmd);
        }

        public override IDbDataAdapter GetDataAdapter(string command, IDbConnection connection)
        {
            return GetDataAdapter(command, connection, null);
        }
        public override DbCommandBuilder CreateDbCommandBuilder(IDbDataAdapter adapter)
        {
            return new SQLiteCommandBuilder((SQLiteDataAdapter)adapter);
        }
        public override int UpdateDataTable(IDbDataAdapter adapter, DataTable dt)
        {
            SQLiteDataAdapter sqlAdater = (SQLiteDataAdapter)adapter;
            return sqlAdater.Update(dt);
        }

        public override DbParameter CreateParameter(String name, String val)
        {
            SQLiteParameter ret = new SQLiteParameter(name, DbType.String);
            ret.Value = val;
            return ret;
        }
        public override DbParameter CreateParameter(String name, byte[] val)
        {
            SQLiteParameter ret = new SQLiteParameter(name, DbType.Binary);
            ret.Value = val;
            return ret;
        }
        public override void FillDataSet(IDbDataAdapter adapter, DataSet ds, String srcTable)
        {
            SQLiteDataAdapter dbAdapter = (SQLiteDataAdapter)adapter;
            dbAdapter.Fill(ds, srcTable);
        }
        public override void UpdateDataSet(IDbDataAdapter adapter, DataSet ds, String srcTable)
        {
            SQLiteDataAdapter dbAdapter = (SQLiteDataAdapter)adapter;
            dbAdapter.Update(ds, srcTable);
        }
        public override IDbDataAdapter GetDataAdapter(string command, IDbConnection connection, Dictionary<string, string> paras)
        {
            SQLiteDataAdapter ret = new SQLiteDataAdapter(command, (SQLiteConnection)connection);
            if (paras != null)
            {
                foreach (KeyValuePair<string, string> kvp in paras)
                {
                    AddParamWithValue(ret.SelectCommand.Parameters, kvp.Key, kvp.Value);
                }
            }
            return ret;
        }
        public override void AddParamWithValue(DbParameterCollection paras, String key, String value)
        {
            (paras as SQLiteParameterCollection).AddWithValue(key, value);
        }

        public override IDbConnection Open(string strCn)
        {
            IDbConnection ret = null;
            if (Parent.ContainsConnection(strCn))
            {
                ret = Parent.GetConnection(strCn);
                if (ret.State != ConnectionState.Open)
                {
                    Parent.RemoveConnection(strCn);
                }
                else
                {
                    return ret;
                }
            }

            ret = new SQLiteConnection(strCn);
            Parent.SaveConnection(strCn, ret);
            ret.Open();
            return ret;
        }
        public override void FillDataSet(DataSet ds, String srcTable, string command, IDbConnection connection, Dictionary<String, String> paras)
        {
            using (SQLiteDataAdapter ret = (SQLiteDataAdapter)GetDataAdapter(command, connection, paras))
            {
                if (String.IsNullOrEmpty(srcTable))
                {
                    ret.Fill(ds);
                }
                else
                {
                    ret.Fill(ds, srcTable);
                }
            }
        }
        public override void FillTable(DataTable table, String command, IDbConnection connection, Dictionary<String, String> paras)
        {
            using (IDbCommand cmd = GetCommand(command, connection, paras))
            using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
            {
                table.Clear();
                table.Load(reader);
            }
        }
        public override void DisposeAdapter(IDbDataAdapter da)
        {
            if (!(da is SQLiteDataAdapter)) return;
            (da as SQLiteDataAdapter).Dispose();
        }
        public override DbParameter CreateParameter(String name, Type type)
        {
            return new SQLiteParameter(name, type);
        }


        public override bool CheckTableExists(String connectString, string tableName)
        {
            bool isExist = false;
            try
            {
                IDbConnection con = Open(connectString);

                DataTable schemaTable = GetSchemaTables(con);

                if (schemaTable != null)
                {
                    for (int i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        if (schemaTable.Rows[i]["TABLE_NAME"].ToString().ToLower() == tableName.ToLower())
                        {
                            isExist = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return isExist;
        }


        public override bool CheckFieldExists(String connectString, string tableName, string columnName)
        {
            bool isExist = true;
            SQLiteConnection conn = (SQLiteConnection)Open(connectString);
            try
            {
                using (var cmd = conn.CreateCommand())
                {

                    cmd.CommandText = string.Format("PRAGMA table_info({0})", tableName);

                    var reader = cmd.ExecuteReader();
                    int nameIndex = reader.GetOrdinal("Name");
                    while (reader.Read())
                    {
                        if (reader.GetString(nameIndex).Equals(columnName))
                        {
                            Close(conn);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close(conn);
            }
            return isExist;
        }

        public override bool CheckIndexExists(String connectString, string tableName, string indexName)
        {
            SQLiteConnection conn = (SQLiteConnection)Open(connectString);
            bool ret = false;
            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = string.Format("SELECT count(*) FROM sqlite_master WHERE type='index' and name={0}", indexName);
                    ret = ((int)cmd.ExecuteScalar(CommandBehavior.SequentialAccess)) > 0;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn != null)
                {
                    Close(conn);
                }
            }
            return false;
        }

        public override bool CheckPrimaryKeyExists(String connectString, string tableName, ref string pkName)
        {
            bool isExist = true;
            SQLiteConnection conn = (SQLiteConnection)Open(connectString);
            try
            {
                using (var cmd = conn.CreateCommand())
                {

                    cmd.CommandText = string.Format("PRAGMA table_info({0})", tableName);

                    var reader = cmd.ExecuteReader();
                    int pkIndex = reader.GetOrdinal("pk");
                    while (reader.Read())
                    {
                        if (!reader.GetString(pkIndex).Equals("0"))
                        {
                            Close(conn);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close(conn);
            }
            return isExist;
        }
        public override string ConvertCommand(string cmd)
        {
            return cmd;
        }
    }
}
