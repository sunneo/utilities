using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Database
{
    public class OleDbBuilder : AbstractDBBuilder
    {
        public OleDbBuilder(DBFactory parent):base(parent)
        {
        }

        public override  void Close(IDbConnection cn, bool forceClose = false)
        {
            if(forceClose)
            {
                try
                {
                    ((OleDbConnection)cn).Close();
                }
                catch(Exception ee)
                {
                    Tracer.D(ee.ToString());
                }
            }
        }
        public override DataTable GetSchemaTables(IDbConnection cn)
        {
            DataTable schemaTable = ((OleDbConnection)cn).GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            return schemaTable;
        }
        public override  bool IsTable(IDbConnection cn, String tableName)
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
        public override DataTable GetTables(IDbConnection cn)
        {
            return ((OleDbConnection)cn).GetSchema("tables");
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
            OleDbCommand ret = new OleDbCommand();
            if (command != null)
            {
                ret.CommandText = command;
            }
            if (cn != null)
            {
                ret.Connection = (OleDbConnection)cn;
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
            OleDbParameter ret = new OleDbParameter(name, OleDbType.VarChar);
            ret.Size = len;
            ret.SourceColumn = sourceColumn;
            return ret;
        }
        public override void BulkCopy(string tableName, DataTable dt, string con)
        {
            IDbConnection cn = this.Open(con);
            BulkCopy(tableName, dt, cn);
        }
        public override void BulkCopy(string tableName, DataTable dt, IDbConnection cn)
        {
            
            try
            {
                List<string> columnList = new List<string>();
                foreach (DataColumn one in dt.Columns)
                {
                    columnList.Add(one.ColumnName);
                }
                IDbDataAdapter adapter = GetDataAdapter("select * from " + tableName, cn);
                using (DbCommandBuilder builder = CreateDbCommandBuilder(adapter))
                {
                    adapter.InsertCommand = builder.GetInsertCommand();
                    foreach (string one in columnList)
                    {
                        adapter.InsertCommand.Parameters.Add(this.CreateParameter(one, dt.Columns[one].DataType));
                    }
                    UpdateDataTable(adapter,dt);
                }
            }
            finally
            {
                Close(cn);
            }
        }
        public override  void SetDataAdapterLoadFillOption(IDbDataAdapter adapter, LoadOption option)
        {
            OleDbDataAdapter oleAdapter = (OleDbDataAdapter)adapter;
            oleAdapter.FillLoadOption = option;
        }
        public override IDbDataAdapter GetDataAdapter(string command, string connection)
        {
            return new OleDbDataAdapter(command, connection);
        }
        public override IDbDataAdapter GetDataAdapter(IDbCommand cmd)
        {
            return new OleDbDataAdapter((OleDbCommand)cmd);
        }

        public override IDbDataAdapter GetDataAdapter(string command, IDbConnection connection)
        {
            return GetDataAdapter(command, connection, null);
        }
        public override DbCommandBuilder CreateDbCommandBuilder(IDbDataAdapter adapter)
        {
            return new OleDbCommandBuilder((OleDbDataAdapter)adapter);
        }
        public override  void UpdateDataTable(IDbDataAdapter adapter, DataTable dt)
        {
            OleDbDataAdapter sqlAdater = (OleDbDataAdapter)adapter;
            sqlAdater.Update(dt);
        }
        public override DbParameter CreateParameter(String name,String val)
        {
            OleDbParameter ret = new OleDbParameter(name, OleDbType.VarChar);
            ret.Value = val;
            return ret;
        }
        public override DbParameter CreateParameter(String name, byte[] val)
        {
            OleDbParameter ret = new OleDbParameter(name, OleDbType.Binary);
            ret.Value = val;
            return ret;
        }
        public override  void FillDataSet(IDbDataAdapter adapter, DataSet ds, String srcTable)
        {
            OleDbDataAdapter dbAdapter = (OleDbDataAdapter)adapter;
            dbAdapter.Fill(ds, srcTable);
        }
        public override  void UpdateDataSet(IDbDataAdapter adapter, DataSet ds, String srcTable)
        {
            OleDbDataAdapter dbAdapter = (OleDbDataAdapter)adapter;
            dbAdapter.Update(ds, srcTable);
        }
        public override IDbDataAdapter GetDataAdapter(string command, IDbConnection connection, Dictionary<string, string> paras)
        {
            OleDbDataAdapter ret = new OleDbDataAdapter(command, (OleDbConnection)connection);
            if (paras != null)
            {
                foreach (KeyValuePair<string, string> kvp in paras)
                {
                    AddParamWithValue(ret.SelectCommand.Parameters, kvp.Key, kvp.Value);
                }
            }
            return ret;
        }
        public override  void AddParamWithValue(DbParameterCollection paras, String key, String value)
        {
            (paras as OleDbParameterCollection).AddWithValue(key, value);
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
            ret = new OleDbConnection(strCn);
            Parent.SaveConnection(strCn, ret);
            ret.Open();
            return ret;
        }
        public override  void FillDataSet(DataSet ds,String srcTable, string command, IDbConnection connection, Dictionary<String, String> paras)
        {
            using (OleDbDataAdapter ret = (OleDbDataAdapter)GetDataAdapter(command, connection, paras))
            {
                if(String.IsNullOrEmpty(srcTable))
                {
                    ret.Fill(ds);
                }
                else
                {
                    ret.Fill(ds, srcTable);
                }
            }
        }
        public override  void FillTable(DataTable table, String command, IDbConnection connection, Dictionary<String, String> paras)
        {
            using (IDbCommand cmd = GetCommand(command, connection, paras))
            using (IDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
            {
                table.Clear();
                table.Load(reader);
            }
        }
        public override  void DisposeAdapter(IDbDataAdapter da)
        {
            if (!(da is OleDbDataAdapter)) return;
            (da as OleDbDataAdapter).Dispose();
        }
        public override DbParameter CreateParameter(String name, Type type)
        {
            return new OleDbParameter(name, type);
        }


        public override  bool CheckTableExists(String connectString, string tableName)
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


        public override  bool CheckFieldExists(String connectString, string tableName, string fieldName)
        {
            bool isExist = true;
            try
            {
                OleDbConnection con = (OleDbConnection)Open(connectString);
                
                object[] oa = { null, null, tableName, fieldName };
                DataTable schemaTable = con.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Columns, oa);

                isExist = (schemaTable.Rows.Count > 0);
                Close(con);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return isExist;
        }

        public override  bool CheckIndexExists(String connectString, string tableName, string indexName)
        {
            bool isExist = false;
            OleDbConnection con = null;
            try
            {

                con = (OleDbConnection)Open(connectString);
                object[] oa = { null, null, null, null, tableName };
                DataTable schemaTable = con.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Indexes, oa);

                if (schemaTable != null)
                {
                    for (int i = 0; i < schemaTable.Rows.Count; i++)
                    {
                        if (schemaTable.Rows[i]["INDEX_NAME"].ToString().ToLower() == indexName.ToLower()
                            && schemaTable.Rows[i]["PRIMARY_KEY"].ToString().ToLower() == "false")
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
            finally
            {
                if (con != null)
                {
                    Close(con);
                }
            }

            return isExist;
        }

        public override  bool CheckPrimaryKeyExists(String connectString, string tableName, ref string pkName)
        {
            bool isExist = true;
            OleDbConnection con = null;
            try
            {
                con = (OleDbConnection)Open(connectString);
                object[] oa = { null, null, tableName };
                DataTable schemaTable = con.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Primary_Keys, oa);

                isExist = (schemaTable.Rows.Count > 0);
                if (isExist)
                    pkName = schemaTable.Rows[0]["PK_NAME"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                {
                    Close(con);
                }
            }
            return isExist;
        }

        public override string ConvertCommand(string cmd)
        {
            return cmd;
        }
    }
}
