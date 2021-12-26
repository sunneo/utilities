using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Utilities.Database
{
    public class BaseTableToDatasetConverter
    {
        public virtual String GetColumnTypeString(DataColumn oColumn)
        {
            switch (oColumn.DataType.Name)
            {
                case "UInt8": return "INTEGER";
                case "UInt16": return "INTEGER";
                case "UInt32": return "INTEGER";
                case "Int8": return "INTEGER";
                case "Int16": return "INTEGER";
                case "Int32": return "INTEGER";
                case "String": return "VarChar(255)";
                case "Double": return "FLOAT";
                case "Float": return "REAL";
                case "DateTime": return "DATETIME";
            }
            return "VarChar(255)";
        }
        public virtual void DataSetToDBFromConnectString(DataSet ds, String connStr)
        {
            IDbConnection cAccess = DBFactory.Default.LocalDbBuilder.Open(connStr);
            this.DataSetToDBFromConnectString(ds, cAccess);
        }
        public virtual void DataSetToDBFromConnectString(DataSet ds, IDbConnection cAccess)
        {
            DataSetToDBFromConnectString(ds, cAccess, DBFactory.Default.LocalDbBuilder);
        }
        public virtual void DataSetToDBFromConnectString(DataSet ds, String connStr, IDbBuilder localBuilder)
        {
            IDbConnection cAccess = localBuilder.Open(connStr);
            DataSetToDBFromConnectString(ds, cAccess, localBuilder);
        }
        public virtual void DataSetToDBFromConnectString(DataSet ds, IDbConnection cAccess, IDbBuilder localBuilder)
        {
            
            try
            {
                foreach (DataTable oTable in ds.Tables)
                {
                    IDbCommand oCommand = localBuilder.GetCommand(
                        "DROP TABLE [" + oTable.TableName + "]", cAccess);
                    {
                        try
                        {
                            oCommand.ExecuteNonQuery();
                        }
                        catch (Exception) { }

                        string strCreateColumns = "";
                        string strColumnList = "";
                        string strQuestionList = "";
                        foreach (DataColumn oColumn in oTable.Columns)
                        {

                            strCreateColumns += "[" + oColumn.ColumnName + "] " + GetColumnTypeString(oColumn) + ", ";
                            strColumnList += "[" + oColumn.ColumnName + "],";
                            strQuestionList += "?,";
                        }
                        strCreateColumns = strCreateColumns.Remove(strCreateColumns.Length - 2);
                        strColumnList = strColumnList.Remove(strColumnList.Length - 1);
                        strQuestionList = strQuestionList.Remove(strQuestionList.Length - 1);
                        oCommand.Dispose();
                        oCommand = localBuilder.GetCommand("CREATE TABLE [" + oTable.TableName
                            + "] (" + strCreateColumns + ")", cAccess);
                        oCommand.ExecuteNonQuery();
                        oCommand.Dispose();

                        IDbDataAdapter da = localBuilder.GetDataAdapter(
                            "SELECT * FROM [" + oTable.TableName + "]", cAccess);
                        da.MissingSchemaAction = MissingSchemaAction.Add;
                        localBuilder.SetDataAdapterLoadFillOption(da, LoadOption.OverwriteChanges);

                        da.InsertCommand = localBuilder.GetCommand(
                            "INSERT INTO [" + oTable.TableName + "] (" + strColumnList
                            + ") VALUES (" + strQuestionList + ")", cAccess);
                        foreach (DataColumn oColumn in oTable.Columns)
                        {
                            da.InsertCommand.Parameters.Add(
                                localBuilder.CreateStringParameter(
                                oColumn.ColumnName,
                                255,
                                oColumn.ColumnName
                                ));
                        }

                        localBuilder.UpdateDataTable(da, oTable);

                    }
                }
            }
            catch (Exception ee)
            {
                Tracer.D(ee.ToString());
            }
        }
        public virtual DataSet DBToDataSetFromConnectString(IDbConnection conn, params String[] includeTables)
        {   
            return DBToDataSetFromConnectString(conn, DBFactory.Default.LocalDbBuilder, includeTables);
        }
        public virtual DataSet DBToDataSetFromConnectString(String con, params String[] includeTables)
        {
            return DBToDataSetFromConnectString(con, DBFactory.Default.LocalDbBuilder, includeTables);
        }
        public virtual DataSet DBToDataSetFromConnectString(String con, IDbBuilder localBuilder, params String[] includeTables)
        {
            IDbConnection conn = localBuilder.Open(con);
            return DBToDataSetFromConnectString(conn, localBuilder, includeTables);
        }
        public virtual DataSet DBToDataSetFromConnectString(IDbConnection conn, IDbBuilder localBuilder, params String[] includeTables)
        {
            DataSet dataSet = new DataSet();
            Dictionary<String, String> tables = new Dictionary<string, string>();
            bool convertAll = false;
            if (includeTables == null || includeTables.Length == 0)
            {
                convertAll = true;
            }
            else
            {
                foreach (String tbl in includeTables)
                {
                    tables[tbl] = tbl;
                }
            }
            try
            {
                // Retrieve the schema
                DataTable schemaTable = localBuilder.GetSchemaTables(conn);
                // Fill the DataTables.
                DataColumn tableNameColumn = null;
                foreach (DataColumn col in schemaTable.Columns)
                {
                    if (col.ColumnName.Equals("TABLE_NAME", StringComparison.InvariantCultureIgnoreCase))
                    {
                        tableNameColumn = col;
                        break;
                    }
                }
                foreach (DataRow dataTableRow in schemaTable.Rows)
                {
                    String tableName = "";
                    if (tableNameColumn != null)
                    {
                        tableName = dataTableRow[tableNameColumn].ToString();
                    }
                    else
                    {
                        tableName = dataTableRow["Table_Name"].ToString();
                    }
                    if (!convertAll && !tables.ContainsKey(tableName)) continue;
                    if (tableName.StartsWith("~", StringComparison.InvariantCultureIgnoreCase)) continue;
                    FillTable(dataSet, conn, tableName, localBuilder);
                }
            }
            catch (Exception ee)
            {

            }
            return dataSet;
        }

        private void FillTable(DataSet dataSet, IDbConnection conn, string tableName, IDbBuilder localBuilder)
        {
            DataTable dataTable = dataSet.Tables.Add(tableName);
            localBuilder.FillTable(dataTable, "SELECT * from " + tableName, conn, null);
        }
        public virtual List<String> GetTableNames(String cons)
        {
            return GetTableNames(cons, DBFactory.Default.LocalDbBuilder);
        }
        public virtual List<String> GetTableNames(String cons, IDbBuilder localBuilder)
        {
            IDbConnection cn = localBuilder.Open(cons);
            return GetTableNames(cn, localBuilder);
        }
        public virtual List<String> GetTableNames(IDbConnection cn, IDbBuilder localBuilder)
        {
            List<String> ret = new List<string>();
            DataTable _dt = new DataTable();
            try
            {
                _dt = localBuilder.GetTables(cn);
                int cnt = _dt.Rows.Count;
                if (cnt == 0) return ret;
                int colCnt = _dt.Columns.Count;
                if (colCnt == 0) return ret;
                for (int i = 0; i < cnt; ++i)
                {
                    String name = _dt.Rows[i]["TABLE_NAME"].ToString();
                    if (name.StartsWith("MSys") || name.StartsWith("~")) continue;
                    ret.Add(name);
                }
            }
            catch (Exception ee)
            {
                Tracer.D(ee.ToString());
            }
            return ret;
        }
        
    }
}
