using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Utilities.Database
{
    public class SQLDBBuilder : AbstractDBBuilder
    {
        public SQLDBBuilder(DBFactory parent) : base(parent)
        {
        }

        public override void Close(IDbConnection cn, bool forceClose = false)
        {
            if (forceClose)
            {
                try
                {
                    ((SqlConnection)cn).Close();
                }
                catch (Exception ee)
                {
                    Tracer.D(ee.ToString());
                }
            }
        }
        public override DataTable GetSchemaTables(IDbConnection cn)
        {
            DataTable schemaTable = ((SqlConnection)cn).GetSchema("Tables");
            return schemaTable;
        }
        public override bool IsTable(IDbConnection cn, String tableName)
        {
            DataTable schemaTable = ((SqlConnection)cn).GetSchema("Tables");

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
            return ((SqlConnection)cn).GetSchema("Views");
        }

        public override DataTable GetTables(IDbConnection cn)
        {
            return ((SqlConnection)cn).GetSchema("Tables");
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
            SqlCommand ret = new SqlCommand();
            if (command != null)
            {
                ret.CommandText = command;
            }
            if (cn != null)
            {
                ret.Connection = (SqlConnection)cn;
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
        public override void AddParamWithValue(DbParameterCollection paras, String key, String value)
        {
            (paras as SqlParameterCollection).AddWithValue(key, value);
        }
        public override void BulkCopy(string TableName, DataTable dt, IDbConnection con)
        {
            try
            {
                using (SqlBulkCopy sqlbulkcopy = new SqlBulkCopy((SqlConnection)con, SqlBulkCopyOptions.UseInternalTransaction, null))
                {
                    try
                    {
                        sqlbulkcopy.DestinationTableName = TableName;
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            sqlbulkcopy.ColumnMappings.Add(dt.Columns[i].ColumnName, dt.Columns[i].ColumnName);
                        }
                        sqlbulkcopy.WriteToServer(dt);
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            catch (Exception ee)
            {
                Tracer.D(ee.ToString());
            }
            Close(con);
        }
        public override DbParameter CreateStringParameter(String name, int len, String sourceColumn)
        {
            SqlParameter ret = new SqlParameter(name, SqlDbType.VarChar);
            ret.Size = len;
            ret.SourceColumn = sourceColumn;
            return ret;
        }
        public override void SetDataAdapterLoadFillOption(IDbDataAdapter adapter, LoadOption option)
        {
            SqlDataAdapter sqlAdapter = (SqlDataAdapter)adapter;
            sqlAdapter.FillLoadOption = option;
        }
        public override IDbDataAdapter GetDataAdapter(string command, string connection)
        {
            return new SqlDataAdapter(command, connection);
        }
        public override IDbDataAdapter GetDataAdapter(string command, IDbConnection connection)
        {
            return GetDataAdapter(command, connection, null);
        }
        public override IDbDataAdapter GetDataAdapter(string command, IDbConnection connection, Dictionary<string, string> paras)
        {
            SqlDataAdapter ret = new SqlDataAdapter(command, (SqlConnection)connection);
            if (paras != null)
            {
                foreach (KeyValuePair<string, string> kvp in paras)
                {
                    AddParamWithValue(ret.SelectCommand.Parameters, kvp.Key, kvp.Value);
                }
            }
            return ret;
        }
        public override IDbDataAdapter GetDataAdapter(IDbCommand cmd)
        {
            return new SqlDataAdapter((SqlCommand)cmd);
        }
        public override int UpdateDataTable(IDbDataAdapter adapter, DataTable dt)
        {
            SqlDataAdapter sqlAdater = (SqlDataAdapter)adapter;
            return sqlAdater.Update(dt);
        }
        public override DbParameter CreateParameter(String name, String val)
        {
            SqlParameter ret = new SqlParameter(name, SqlDbType.VarChar);
            ret.Value = val;
            return ret;
        }
        public override DbParameter CreateParameter(String name, byte[] val)
        {
            SqlParameter ret = new SqlParameter(name, SqlDbType.Binary);
            ret.Value = val;
            return ret;
        }
        public override void FillDataSet(IDbDataAdapter adapter, DataSet ds, String srcTable)
        {
            SqlDataAdapter dbAdapter = (SqlDataAdapter)adapter;
            dbAdapter.Fill(ds, srcTable);
        }
        public override DbCommandBuilder CreateDbCommandBuilder(IDbDataAdapter adapter)
        {
            return new SqlCommandBuilder((SqlDataAdapter)adapter);
        }
        public override void UpdateDataSet(IDbDataAdapter adapter, DataSet ds, String srcTable)
        {
            SqlDataAdapter dbAdapter = (SqlDataAdapter)adapter;
            dbAdapter.Update(ds, srcTable);
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
            ret = new SqlConnection(strCn);
            Parent.SaveConnection(strCn, ret);
            ret.Open();
            return ret;
        }
        public override void FillDataSet(DataSet ds, String srcTable, string command, IDbConnection connection, Dictionary<String, String> paras)
        {
            using (SqlDataAdapter ret = (SqlDataAdapter)GetDataAdapter(command, connection, paras))
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
            if (!(da is SqlDataAdapter)) return;
            (da as SqlDataAdapter).Dispose();
        }
        public override DbParameter CreateParameter(String name, Type type)
        {
            return new SqlParameter(name, type);
        }
        private int Fill_tb(String connectString, string sql, ref DataTable dt)
        {
            int _ret = -1;
            IDbConnection cn = Open(connectString);
            FillTable(dt, sql, cn, null);
            Close(cn);

            _ret = 0;
            if (dt != null) _ret = dt.Rows.Count;

            return _ret;
        }

        public override bool CheckTableExists(String connectString, string tableName)
        {
            bool isExist = true;
            try
            {
                DataTable dt = new DataTable();
                string _sql = "select * from sys.objects where object_id = OBJECT_ID(N'" + tableName + "') and type in (N'U')";
                Fill_tb(connectString, _sql, ref dt);
                if (dt.Rows.Count == 0)
                    isExist = false;
            }
            catch (Exception ex)
            {
                isExist = false;
                throw ex;
            }

            return isExist;
        }

        public override bool CheckFieldExists(String connectString, string tableName, string fieldName)
        {
            bool isExist = true;
            try
            {
                DataTable dt = new DataTable();
                string _sql = "select id from syscolumns where id=object_id('" + tableName + "') and name='" + fieldName + "'";
                Fill_tb(connectString, _sql, ref dt);
                if (dt.Rows.Count == 0)
                    isExist = false;
            }
            catch (Exception ex)
            {
                isExist = false;
                throw ex;
            }

            return isExist;

        }
        public override bool CheckIndexExists(String connectString, string tableName, string indexName)
        {
            bool isExist = true;
            try
            {
                DataTable dt = new DataTable();
                string _sql = "select name from sys.indexes where name ='" + indexName + "'";
                Fill_tb(connectString, _sql, ref dt);
                if (dt.Rows.Count == 0)
                    isExist = false;
            }
            catch (Exception ex)
            {
                isExist = false;
                throw ex;
            }

            return isExist;

        }

        public override bool CheckPrimaryKeyExists(String connectString, string tableName, ref string pkName)
        {
            bool isExist = true;
            try
            {
                DataTable dt = new DataTable();
                string _sql = "select name from sys.indexes where type = 1 and name ='" + pkName + "'";
                Fill_tb(connectString, _sql, ref dt);
                if (dt.Rows.Count == 0)
                    isExist = false;
            }
            catch (Exception ex)
            {
                isExist = false;
                throw ex;
            }

            return isExist;
        }
        public override string ConvertCommand(string cmd)
        {
            return cmd;
        }

    }
}
