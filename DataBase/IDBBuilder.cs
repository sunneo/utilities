using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities.Database
{
    public interface IDbBuilder
    {
        DBFactory Parent { get; }
        BaseTableToDatasetConverter GetTableToDatasetConverter();

        IDbConnection Open(String strCn);
        bool IsTable(IDbConnection cn, String tableName);
        DataTable GetViews(IDbConnection cn);
        DataTable GetTables(IDbConnection cn);
        DataTable GetSchemaTables(IDbConnection cn);
        IDbCommand GetCommand();



        String ConvertCommand(String cmd);
        IDbCommand GetCommand(String command, IDbConnection cn);
        IDbCommand GetCommand(String command, IDbConnection cn, Dictionary<string, string> param);
        IDbDataAdapter GetDataAdapter(String command, String connection);
        IDbDataAdapter GetDataAdapter(string command, IDbConnection connection);
        IDbDataAdapter GetDataAdapter(string command, IDbConnection connection, Dictionary<string, string> param);
        IDbDataAdapter GetDataAdapter(IDbCommand cmd);

        void SetDataAdapterLoadFillOption(IDbDataAdapter adapter, LoadOption option);

        void FillDataSet(DataSet ds, String srcTable, string command, IDbConnection connection, Dictionary<String, String> paras);
        void FillDataSet(IDbDataAdapter adapter, DataSet ds, String srcTable);
        void UpdateDataSet(IDbDataAdapter adapter, DataSet ds, String srcTable);
        int UpdateDataTable(IDbDataAdapter adapter, DataTable dt);
        DbCommandBuilder CreateDbCommandBuilder(IDbDataAdapter adapter);

        void FillTable(DataTable table, String command, IDbConnection connection, Dictionary<String, String> paras);
        DbParameter CreateParameter(String name, String val);
        DbParameter CreateStringParameter(String name, int len, String sourceColumn);

        DbParameter CreateParameter(String name, byte[] val);
        DbParameter CreateParameter(String name, Type val);
        void BulkCopy(string tableName, DataTable dt, string con);
        void BulkCopy(string tableName, DataTable dt, IDbConnection con);
        void DisposeAdapter(IDbDataAdapter da);
        void Close(IDbConnection cn, bool forceClose = false);

        void AddParamWithValue(DbParameterCollection paras, String key, String value);

        bool CheckTableExists(String connectString, string tableName);

        bool CheckFieldExists(String connectString, string tableName, string fieldName);
        bool CheckIndexExists(String connectString, string tableName, string indexName);

        bool CheckPrimaryKeyExists(String connectString, string tableName, ref string pkName);



    }
    public static class DataRowExtension
    {
        public static int GetInt(this DataRowView row, int idx)
        {
            object obj = row[idx];
            return Convert.ToInt32(obj);
        }
        public static int GetInt(this DataRowView row, String idx)
        {
            object obj = row[idx];
            return Convert.ToInt32(obj);
        }
        public static int GetInt(this DataRow row, int idx)
        {
            object obj = row[idx];
            return Convert.ToInt32(obj);
        }
        public static int GetInt(this DataRow row, String idx)
        {
            object obj = row[idx];
            return Convert.ToInt32(obj);
        }
    }
    

}
