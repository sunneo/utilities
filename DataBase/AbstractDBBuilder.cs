using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Database
{
    public abstract class AbstractDBBuilder : IDbBuilder
    {
        internal DBFactory m_Parent;
        public DBFactory Parent
        {
            get
            {
                return m_Parent;
            }
            private set
            {
                m_Parent = value;
            }
        }

        BaseTableToDatasetConverter tableToDataSetConverter = null;
        public virtual BaseTableToDatasetConverter GetTableToDatasetConverter()
        {
            if (tableToDataSetConverter == null)
            {
                tableToDataSetConverter = new BaseTableToDatasetConverter();
            }
            return tableToDataSetConverter;
        }
        public AbstractDBBuilder(DBFactory parent)
        {
            this.Parent = parent;
        }
        public virtual void AddParamWithValue(DbParameterCollection paras, string key, string value)
        {

        }

        public virtual void BulkCopy(string tableName, DataTable dt, string con)
        {
            IDbConnection cn = this.Open(con);
            BulkCopy(tableName, dt, cn);
        }
        public virtual void BulkCopy(string tableName, DataTable dt, IDbConnection cn)
        {


        }

        public virtual bool CheckFieldExists(string connectString, string tableName, string fieldName)
        {
            return false;
        }

        public virtual bool CheckIndexExists(string connectString, string tableName, string indexName)
        {
            return false;
        }

        public virtual bool CheckPrimaryKeyExists(string connectString, string tableName, ref string pkName)
        {
            return false;
        }

        public virtual bool CheckTableExists(string connectString, string tableName)
        {
            return false;
        }

        public virtual void Close(IDbConnection cn, bool forceClose = false)
        {

        }

        public virtual string ConvertCommand(string cmd)
        {
            return cmd;
        }

        public virtual DbCommandBuilder CreateDbCommandBuilder(IDbDataAdapter adapter)
        {
            return null;
        }

        public virtual DbParameter CreateParameter(string name, string val)
        {
            return null;
        }

        public virtual DbParameter CreateParameter(string name, byte[] val)
        {
            return null;
        }

        public virtual DbParameter CreateParameter(string name, Type val)
        {
            return null;
        }

        public virtual DbParameter CreateStringParameter(string name, int len, string sourceColumn)
        {
            return null;
        }

        public virtual void DisposeAdapter(IDbDataAdapter da)
        {

        }

        public virtual void FillDataSet(DataSet ds, string srcTable, string command, IDbConnection connection, Dictionary<string, string> paras)
        {

        }

        public virtual void FillDataSet(IDbDataAdapter adapter, DataSet ds, string srcTable)
        {

        }

        public virtual void FillTable(DataTable table, string command, IDbConnection connection, Dictionary<string, string> paras)
        {

        }

        public virtual IDbCommand GetCommand()
        {
            return null;
        }

        public virtual IDbCommand GetCommand(string command, IDbConnection cn)
        {
            return null;
        }

        public virtual IDbCommand GetCommand(string command, IDbConnection cn, Dictionary<string, string> param)
        {
            return null;
        }

        public virtual IDbDataAdapter GetDataAdapter(string command, string connection)
        {
            return null;
        }

        public virtual IDbDataAdapter GetDataAdapter(string command, IDbConnection connection)
        {
            return null;
        }

        public virtual IDbDataAdapter GetDataAdapter(string command, IDbConnection connection, Dictionary<string, string> param)
        {
            return null;
        }

        public virtual IDbDataAdapter GetDataAdapter(IDbCommand cmd)
        {
            return null;
        }

        public virtual DataTable GetSchemaTables(IDbConnection cn)
        {
            return null;
        }
        public virtual DataTable GetViews(IDbConnection cn)
        {
            return null;
        }
        public virtual DataTable GetTables(IDbConnection cn)
        {
            return null;
        }

        public virtual bool IsTable(IDbConnection cn, string tableName)
        {
            return false;
        }

        public virtual IDbConnection Open(string strCn)
        {
            return null;
        }

        public virtual void SetDataAdapterLoadFillOption(IDbDataAdapter adapter, LoadOption option)
        {

        }

        public virtual void UpdateDataSet(IDbDataAdapter adapter, DataSet ds, string srcTable)
        {

        }

        public virtual int UpdateDataTable(IDbDataAdapter adapter, DataTable dt)
        {
            return 0;
        }
    }
}
