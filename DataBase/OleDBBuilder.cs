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
        public OleDbBuilder(DBFactory parent) : base(parent)
        {
        }

        public override void Close(IDbConnection cn, bool forceClose = false)
        {
            if (forceClose)
            {
                try
                {
                    ((OleDbConnection)cn).Close();
                }
                catch (Exception ee)
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
        public virtual OleDbType GetColumnType(DataColumn oColumn)
        {
            switch (oColumn.DataType.Name)
            {
                case "UInt8": return OleDbType.Integer;
                case "UInt16": return OleDbType.Integer;
                case "UInt32": return OleDbType.Integer;
                case "Int8": return OleDbType.Integer;
                case "Int16": return OleDbType.Integer;
                case "Int32": return OleDbType.Integer;
                case "Int64": return OleDbType.Integer;
                case "String": return OleDbType.VarChar;
                case "Double": return OleDbType.Double;
                case "Float": return OleDbType.Double;
                case "DateTime": return OleDbType.Date;
                case "Byte[]": return OleDbType.Binary;
            }
            return OleDbType.VarChar;
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
            DataTable ret = ((OleDbConnection)cn).GetSchema("views");
            return ret;
        }

        public override DataTable GetTables(IDbConnection cn)
        {
            DataTable ret = ((OleDbConnection)cn).GetSchema("tables");
            return ret;
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
        public override void BulkCopy(string tableName, DataTable _dt, IDbConnection cn)
        {

            try
            {

                int _nResult = 0;
                if (_dt == null) return;
                string _sCmdText = string.Format("select * from {0} where 1=2", _dt.TableName);
                OleDbCommand _Command = (OleDbCommand)GetCommand(_sCmdText, cn);
                OleDbDataAdapter _adapter = new OleDbDataAdapter(_Command);
                OleDbDataAdapter _adapter1 = new OleDbDataAdapter(_Command);
                OleDbCommandBuilder _builder = new OleDbCommandBuilder(_adapter1);

                _adapter.InsertCommand = _builder.GetInsertCommand(true);
                List<DataColumn> dateTimeCols = new List<DataColumn>();
                foreach (DataColumn _dc in _dt.Columns)
                {
                    if (GetColumnType(_dc) == OleDbType.Date)
                    {
                        dateTimeCols.Add(_dc);
                    }
                }
                if (_adapter.InsertCommand.Parameters.Count < _dt.Columns.Count)
                {
                    foreach (DataColumn _dc in _dt.Columns)
                    {
                        if (!_adapter.InsertCommand.Parameters.Contains(_dc.ColumnName))
                        {
                            _adapter.InsertCommand.CommandText =
                                _adapter.InsertCommand.CommandText.Insert(_adapter.InsertCommand.CommandText.IndexOf(") VALUES"), ',' + _dc.ColumnName);

                            _adapter.InsertCommand.CommandText =
                                _adapter.InsertCommand.CommandText.Insert(_adapter.InsertCommand.CommandText.Length - 1, ",?");

                            _adapter.InsertCommand.Parameters.Add("@" + _dc.ColumnName, GetColumnType(_dc), _dc.MaxLength, _dc.ColumnName);

                            if (_adapter.InsertCommand.Parameters.Count >= _dt.Columns.Count)
                                break;
                        }
                    }
                }


                IDbTransaction tr = cn.BeginTransaction();
                try
                {
                    _adapter.InsertCommand.Transaction = (OleDbTransaction)tr;

                    for (int i = 0; i < _dt.Rows.Count; ++i)
                    {

                        DataRow row = _dt.Rows[i];
                        row.SetAdded();

                        try
                        {
                            _nResult = _adapter.Update(_dt);
                        }
                        catch (Exception ee)
                        {

                        }
                    }


                    tr.Commit();
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                }
                tr.Dispose();
            }
            finally
            {
                Close(cn);
            }
        }


        public override void SetDataAdapterLoadFillOption(IDbDataAdapter adapter, LoadOption option)
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
        public override int UpdateDataTable(IDbDataAdapter adapter, DataTable dt)
        {
            OleDbDataAdapter sqlAdater = (OleDbDataAdapter)adapter;
            sqlAdater.ContinueUpdateOnError = true;
            return sqlAdater.Update(dt);
        }
        public override DbParameter CreateParameter(String name, String val)
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
        public override void FillDataSet(IDbDataAdapter adapter, DataSet ds, String srcTable)
        {
            OleDbDataAdapter dbAdapter = (OleDbDataAdapter)adapter;
            dbAdapter.Fill(ds, srcTable);
        }
        public override void UpdateDataSet(IDbDataAdapter adapter, DataSet ds, String srcTable)
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
        public override void AddParamWithValue(DbParameterCollection paras, String key, String value)
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
        public override void FillDataSet(DataSet ds, String srcTable, string command, IDbConnection connection, Dictionary<String, String> paras)
        {
            using (OleDbDataAdapter ret = (OleDbDataAdapter)GetDataAdapter(command, connection, paras))
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
            if (!(da is OleDbDataAdapter)) return;
            (da as OleDbDataAdapter).Dispose();
        }
        public override DbParameter CreateParameter(String name, Type type)
        {
            return new OleDbParameter(name, type);
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


        public override bool CheckFieldExists(String connectString, string tableName, string fieldName)
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

        public override bool CheckIndexExists(String connectString, string tableName, string indexName)
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

        public override bool CheckPrimaryKeyExists(String connectString, string tableName, ref string pkName)
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
