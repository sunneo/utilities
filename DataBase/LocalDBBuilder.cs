using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities.UI;

namespace Utilities.Database
{
    public class LocalDBBuilder : AbstractDBBuilder
    {
        internal IDbBuilder oleDB;
        internal IDbBuilder sqlite;
        bool UseSqliteFirst = true;
        IDbBuilder m_Selection;
        IDbBuilder Selection
        {
            get
            {
                if (m_Selection == null)
                {
                    if (UseSqliteFirst)
                    {
                        m_Selection = sqlite;
                    }
                    else
                    {
                        m_Selection = oleDB;
                    }
                }
                return m_Selection;
            }
            set
            {
                m_Selection = value;
            }
        }
        public LocalDBBuilder(DBFactory parent, bool useSqliteFirst=true) : base(parent)
        {
            this.oleDB = parent.OleDbBuilder;
            this.sqlite = parent.SQLiteDbBuilder;

        }
        public override void BulkCopy(string tableName, DataTable dt, string con)
        {
            IDbConnection cn = Open(con);
            if (cn is OleDbConnection)
            {
                oleDB.BulkCopy(tableName, dt, con);
                return;
            }
            Selection.BulkCopy(tableName, dt, con);
        }
        public override void BulkCopy(string tableName, DataTable dt, IDbConnection con)
        {
            if (con is OleDbConnection)
            {
                oleDB.BulkCopy(tableName, dt, con);
                return;
            }
            Selection.BulkCopy(tableName, dt, con);
        }

        public override bool CheckFieldExists(string connectString, string tableName, string fieldName)
        {
            IDbConnection cn = Open(connectString);
            if (cn is OleDbConnection)
            {
                return oleDB.CheckFieldExists(connectString, tableName, fieldName);
            }
            return Selection.CheckFieldExists(connectString, tableName, fieldName);
        }

        public override bool CheckIndexExists(string connectString, string tableName, string indexName)
        {
            IDbConnection cn = Open(connectString);
            if (cn is OleDbConnection)
            {
                return oleDB.CheckIndexExists(connectString, tableName, indexName);
            }
            return Selection.CheckIndexExists(connectString, tableName, indexName);
        }

        public override bool CheckPrimaryKeyExists(string connectString, string tableName, ref string pkName)
        {
            IDbConnection cn = Open(connectString);
            if (cn is OleDbConnection)
            {
                return oleDB.CheckPrimaryKeyExists(connectString, tableName, ref pkName);
            }
            return Selection.CheckPrimaryKeyExists(connectString, tableName, ref pkName);
        }

        public override bool CheckTableExists(string connectString, string tableName)
        {
            IDbConnection cn = Open(connectString);
            if (cn is OleDbConnection)
            {
                return oleDB.CheckTableExists(connectString, tableName);
            }
            return Selection.CheckTableExists(connectString, tableName);
        }

        public override void Close(IDbConnection cn, bool forceClose = false)
        {
            if (cn is OleDbConnection)
            {
                oleDB.Close(cn, true);
                return;
            }
            Selection.Close(cn, forceClose);
        }

        public override DbCommandBuilder CreateDbCommandBuilder(IDbDataAdapter adapter)
        {
            if (adapter is OleDbDataAdapter)
            {
                return oleDB.CreateDbCommandBuilder(adapter);
            }
            return Selection.CreateDbCommandBuilder(adapter);
        }

        public override DbParameter CreateParameter(string name, string val)
        {
            return Selection.CreateParameter(name, val);
        }

        public override DbParameter CreateParameter(string name, byte[] val)
        {
            return Selection.CreateParameter(name, val);
        }

        public override DbParameter CreateParameter(string name, Type val)
        {
            return Selection.CreateParameter(name, val);
        }

        public override DbParameter CreateStringParameter(string name, int len, string sourceColumn)
        {
            return Selection.CreateStringParameter(name, len, sourceColumn);
        }

        public override void DisposeAdapter(IDbDataAdapter da)
        {
            if (da is OleDbDataAdapter)
            {
                oleDB.DisposeAdapter(da);
                return;
            }
            Selection.DisposeAdapter(da);
        }

        public override void FillDataSet(DataSet ds, string srcTable, string command, IDbConnection connection, Dictionary<string, string> paras)
        {
            if (connection is OleDbConnection)
            {
                oleDB.FillDataSet(ds, srcTable, command, connection, paras);
                return;
            }
            Selection.FillDataSet(ds, srcTable, command, connection, paras);
        }

        public override void FillDataSet(IDbDataAdapter adapter, DataSet ds, string srcTable)
        {
            if (adapter is OleDbDataAdapter)
            {
                oleDB.FillDataSet(adapter, ds, srcTable);
                return;
            }
            Selection.FillDataSet(adapter, ds, srcTable);
        }

        public override void FillTable(DataTable table, string command, IDbConnection connection, Dictionary<string, string> paras)
        {
            if (connection is OleDbConnection)
            {
                oleDB.FillTable(table, command, connection, paras);
                return;
            }
            Selection.FillTable(table, command, connection, paras);
        }

        public override IDbCommand GetCommand()
        {
            return Selection.GetCommand();
        }

        public override IDbCommand GetCommand(string command, IDbConnection cn)
        {
            if (cn is OleDbConnection)
            {
                return oleDB.GetCommand(command, cn);
            }
            return Selection.GetCommand(ConvertCommand(command), cn);
        }

        public override IDbCommand GetCommand(string command, IDbConnection cn, Dictionary<string, string> param)
        {
            if (cn is OleDbConnection)
            {
                return oleDB.GetCommand(command, cn, param);
            }
            return Selection.GetCommand(ConvertCommand(command), cn, param);
        }

        public override IDbDataAdapter GetDataAdapter(string command, string connection)
        {
            IDbConnection cn = Open(connection);
            if (cn is OleDbConnection)
            {
                return oleDB.GetDataAdapter(ConvertCommand(command), connection);
            }
            return Selection.GetDataAdapter(ConvertCommand(command), connection);
        }

        public override IDbDataAdapter GetDataAdapter(string command, IDbConnection connection)
        {
            if (connection is OleDbConnection)
            {
                return oleDB.GetDataAdapter(command, connection);
            }
            return Selection.GetDataAdapter(ConvertCommand(command), connection);
        }

        public override IDbDataAdapter GetDataAdapter(string command, IDbConnection connection, Dictionary<string, string> param)
        {
            if (connection is OleDbConnection)
            {
                return oleDB.GetDataAdapter(command, connection, param);
            }
            return Selection.GetDataAdapter(ConvertCommand(command), connection, param);
        }

        public override IDbDataAdapter GetDataAdapter(IDbCommand cmd)
        {
            if (cmd is OleDbCommand)
            {
                return oleDB.GetDataAdapter(cmd);
            }
            return Selection.GetDataAdapter(cmd);
        }

        public override DataTable GetSchemaTables(IDbConnection cn)
        {
            if (cn is OleDbConnection)
            {
                return oleDB.GetSchemaTables(cn);
            }
            return Selection.GetSchemaTables(cn);
        }

        public override DataTable GetTables(IDbConnection cn)
        {
            if (cn is OleDbConnection)
            {
                return oleDB.GetTables(cn);
            }
            return Selection.GetTables(cn);
        }

        public override bool IsTable(IDbConnection cn, string tableName)
        {
            if (cn is OleDbConnection)
            {
                return oleDB.IsTable(cn, tableName);
            }
            return Selection.IsTable(cn, tableName);
        }
        Dictionary<char, char> specialOp = new Dictionary<char, char>();
        char[] opList = "()[]+-*/&^$#!,'\" \t\n".ToCharArray();
        private List<SQLCommandToken> TokenizeCommand(String cmd)
        {
            List<SQLCommandToken> tokenList = new List<SQLCommandToken>();
            for (int i = 0; i < opList.Length; ++i)
            {
                char c = opList[i];
                specialOp[c] = c;
            }
            StringBuilder strb = new StringBuilder();
            for (int ii = 0; ii < cmd.Length; ++ii)
            {
                char c = cmd[ii];
                if (specialOp.ContainsKey(c))
                {
                    if (strb.Length > 0)
                    {
                        tokenList.Add(new SQLCommandToken() { literal = strb.ToString() });
                        strb.Clear();
                    }
                    tokenList.Add(new SQLCommandToken() { literal = c.ToString() });
                }
                else
                {
                    strb.Append(c);
                }
            }
            if (strb.Length > 0)
            {
                tokenList.Add(new SQLCommandToken() { literal = strb.ToString() });
                strb.Clear();
            }
            return tokenList;
        }
        public class SQLCommandToken
        {
            public bool isSubCommand;
            public String literal;
            public override string ToString()
            {
                if (!isSubCommand)
                {
                    return literal;
                }
                return base.ToString();
            }
        }
        LRUDictionary<String, String> lru = new LRUDictionary<string, string>(128);
        public override String ConvertCommand(String cmd)
        {
            if (!UseSqliteFirst)
            {
                return cmd;
            }
            if (lru.ContainsKey(cmd))
            {
                return lru.Get(cmd);
            }
            List<SQLCommandToken> tokens = TokenizeCommand(cmd);
            SQLExpression expr = ScanTokens(tokens.GetEnumerator(), null, "");
            ConvertSQLExpression(expr);
            String newCommand = expr.ToString();
            lru.Put(cmd, newCommand);
            return newCommand;
        }
        public void ConvertSQLExpression(SQLExpression expr)
        {
            List<SQLCommandToken> topTokens = new List<SQLCommandToken>();
            for (LinkedListNode<SQLCommandToken> topToken = expr.Children.First; topToken != null; topToken = topToken.Next)
            {
                if (topToken.Value == null)
                {
                    continue;
                }
                if (topToken.Value.isSubCommand && topToken.Value is SQLExpression)
                {
                    ConvertSQLExpression((SQLExpression)topToken.Value);
                    continue;
                }
                if ("top".Equals(topToken.Value.literal, StringComparison.InvariantCultureIgnoreCase))
                {
                    int intval = 0;
                    while (topToken != null)
                    {
                        topTokens.Add(topToken.Value);
                        String strVal = topToken.Value.literal;
                        LinkedListNode<SQLCommandToken> topTokenRemove = topToken;
                        topToken = topToken.Next;
                        expr.Children.Remove(topTokenRemove);
                        if (int.TryParse(strVal, out intval))
                        {
                            break;
                        }
                    }

                    break;
                }
            }
            if (topTokens.Count > 0)
            {
                topTokens[0].literal = "limit";
                if (!expr.Children.First.Value.isSubCommand && expr.Children.First.Value.literal.Equals("("))
                {
                    expr.Children.AddBefore(expr.Children.Last, new SQLCommandToken() { literal = " " });
                    foreach (SQLCommandToken tok in topTokens)
                    {
                        expr.Children.AddBefore(expr.Children.Last, tok);
                    }
                }
                else
                {
                    expr.Children.AddLast(new SQLCommandToken() { literal = " " });
                    foreach (SQLCommandToken tok in topTokens)
                    {
                        expr.Children.AddLast(tok);
                    }
                }
            }

        }
        public class SQLExpression : SQLCommandToken
        {
            public SQLExpression Parent;

            public LinkedList<SQLCommandToken> Children = new LinkedList<SQLCommandToken>();
            public override string ToString()
            {// serialize command
                if (this.Children.Count > 0)
                {
                    StringBuilder strb = new StringBuilder();
                    bool inStr = false;
                    String endStrToken = "";

                    foreach (SQLCommandToken token in Children)
                    {
                        if (inStr)
                        {
                            if (token.ToString() == endStrToken)
                            {
                                inStr = false;
                            }
                        }
                        else
                        {
                            if (token.ToString() == "'")
                            {
                                endStrToken = "'";
                                inStr = true;
                            }
                            else if (token.ToString() == "\"")
                            {
                                endStrToken = "\"";
                                inStr = true;
                            }
                            else if (token.ToString() == "[")
                            {
                                endStrToken = "]";
                                inStr = true;
                            }
                        }
                        strb.Append(token.ToString());

                    }
                    return strb.ToString();
                }
                return base.ToString();
            }
        }
        internal SQLExpression ScanTokens(IEnumerator<SQLCommandToken> tokens, SQLCommandToken currentToken, string endString)
        {
            SQLExpression expr = new SQLExpression();
            bool hasNext = true;
            int level = 0;
            if (currentToken != null)
            {
                expr.Children.AddLast(currentToken);
            }
            while (hasNext)
            {
                if (tokens.Current != null)
                {
                    if (tokens.Current.literal.Equals("(", StringComparison.InvariantCultureIgnoreCase))
                    {
                        ++level;
                        SQLCommandToken current = tokens.Current;
                        hasNext = tokens.MoveNext();
                        SQLExpression next = ScanTokens(tokens, current, ")");
                        next.isSubCommand = true;
                        // add sub command
                        next.isSubCommand = true;
                        expr.Children.AddLast(next);
                    }
                    if (!String.IsNullOrEmpty(endString))
                    {
                        if (tokens.Current.literal.Equals(endString))
                        {
                            expr.Children.AddLast(tokens.Current);
                            tokens.MoveNext();
                            // sub command ends here
                            return expr;
                        }
                    }
                    if (tokens.Current != null)
                    {
                        expr.Children.AddLast(tokens.Current);
                    }
                }
                hasNext = tokens.MoveNext();
            }
            return expr;
        }
        /// <summary>
        /// 因為unitdatabase直接被C++存取
        /// 這邊讓他沿用oledb
        /// </summary>
        bool UnitDatabaseAlwaysUseOleDB = true;
        protected IDbConnection OpenBody(string strCn)
        {
            if (!UseSqliteFirst)
            {
                return oleDB.Open(strCn);
            }
            IDbConnection ret = null;
            strCn = Parent.GetConvertedString(strCn);
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
            // original one
            String origStrCn = strCn;
            if (!strCn.EndsWith(".db"))
            {
                String[] parts = strCn.Split(';');
                String dataSource = "";
                foreach (String part in parts)
                {
                    if (part.StartsWith("Data Source="))
                    {
                        String[] dataSourceParts = part.Split('=');
                        if (dataSourceParts.Length > 1)
                        {
                            dataSource = dataSourceParts[1];
                        }
                    }
                }
                // got data source
                // if it was mdb
                String mdbDataSource = dataSource;
                String dbDataSource = dataSource;
                if (mdbDataSource.EndsWith("UnitDatabase.mdb", StringComparison.InvariantCultureIgnoreCase))
                {
                    return oleDB.Open(strCn);
                }
                if (!String.IsNullOrEmpty(mdbDataSource))
                {
                    Parent.SaveConvertedString(strCn, "Data Source=" + mdbDataSource);
                }
                else
                {

                }

                if (!String.IsNullOrEmpty(dataSource))
                {
                    if (dataSource.EndsWith(".mdb"))
                    {
                        dbDataSource = dataSource.Replace(".mdb", ".db");
                        strCn = "DataSource=" + dbDataSource;
                    }
                    if (!File.Exists(dbDataSource) && File.Exists(dataSource))
                    {
                        //TODO DB convert from MDB
                        ConvertMDBToSqlite(oleDB, sqlite, origStrCn, strCn);
                        Parent.SaveConvertedString(origStrCn, strCn);
                    }
                }
            }
            if (!strCn.EndsWith(".db"))
            {

            }

            ret = sqlite.Open(strCn);
            if (ret != null)
            {
                Selection = sqlite;
                using (IDbCommand command = GetCommand())
                {
                    command.CommandText = "PRAGMA mmap_size= 32768";
                    command.Connection = ret;
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                using (IDbCommand command = GetCommand())
                {
                    command.CommandText = "PRAGMA cache=shared";
                    command.Connection = ret;
                    command.ExecuteNonQuery();
                    command.Dispose();
                }
                return ret;
            }
            else
            {
                Selection = oleDB;
                ret = oleDB.Open(origStrCn);
                return ret;
            }
        }
        public override IDbConnection Open(string strCn)
        {
            object oleDbLock = Parent.GetLock(strCn);

            lock (oleDbLock)
            {
                return OpenBody(strCn);
            }


        }
        public static Form GetCurrentForm()
        {
            Form frm = null;
            if (Application.OpenForms != null && Application.OpenForms.Count > 0)
            {
                frm = Application.OpenForms[Application.OpenForms.Count - 1];
            }
            return frm;
        }
        public static bool InvokeRequired()
        {
            Form frm = GetCurrentForm();
            if (frm == null) return false;
            // in main thread
            return (frm.InvokeRequired);
        }


        public static void DoConvertProgressive(IDbBuilder builderFrom, IDbBuilder builderTo, String origStrCn, String strCn, ProgressDialog reporter)
        {
            IDbConnection oleConnect = builderFrom.Open(origStrCn);
            List<String> tableNames = builderFrom.GetTableToDatasetConverter().GetTableNames(oleConnect, builderFrom);
            DataTable views = builderFrom.GetViews(oleConnect);
            int viewCount = views.Rows.Count;
            int tableCount = tableNames.Count;
            IDbConnection sqliteConnection = builderTo.Open(strCn);
            int totalProgress = tableCount + viewCount;
            if (reporter != null)
            {
                reporter.SetAutoClose(false);
                reporter.BeginTask("轉換資料庫", tableNames.Count + viewCount);
            }
            try
            {

                for (int i = 0; i < tableNames.Count; ++i)
                {
                    String tableName = tableNames[i];
                    if (reporter.IsCancelled)
                    {
                        break;
                    }
                    if (reporter != null)
                    {
                        reporter.SubTask(String.Format("[{0}/{1}] 轉換資料表 {2} ", i + 1, totalProgress, tableName));
                    }
                    else
                    {
                        Console.WriteLine("[{0}/{1}] 轉換資料表 {2} ", i + 1, totalProgress, tableName);
                    }

                    DataSet ds = builderFrom.GetTableToDatasetConverter().DBToDataSetFromConnectString(oleConnect, builderFrom, tableName);
                    builderTo.GetTableToDatasetConverter().DataSetToDBFromConnectString(ds, sqliteConnection, builderTo);
                    if (reporter != null)
                    {
                        reporter.Work(1);
                        reporter.SubTask(String.Format("[{0}/{1}] 轉換資料表 {2} ...OK", i + 1, totalProgress, tableName));
                    }
                }

                if (!reporter.IsCancelled)
                {

                    for (int i = 0; i < viewCount; ++i)
                    {
                        if (reporter.IsCancelled)
                        {
                            break;
                        }
                        DataRow row = views.Rows[i];
                        String viewName = row["TABLE_NAME"].ToString();
                        if (reporter != null)
                        {
                            reporter.SubTask(String.Format("[{0}/{1}] 轉換View {2}", tableCount + i + 1, totalProgress, viewName));
                        }
                        else
                        {
                            Console.WriteLine("[{0}/{1}] 轉換View {2} 從 MDB 到 DB", tableCount + i + 1, totalProgress, viewName);
                        }
                        using (IDbCommand cmd = builderTo.GetCommand("DROP VIEW IF EXISTS " + viewName, sqliteConnection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                        using (IDbCommand cmd = builderTo.GetCommand("CREATE VIEW " + viewName + " AS " + row["VIEW_DEFINITION"] + ";", sqliteConnection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                Thread.ResetAbort();
            }

            builderFrom.Parent.RemoveConnection(origStrCn);
            builderFrom.Close(oleConnect, true);

            builderFrom.Parent.RemoveConnection(strCn);
            builderTo.Close(sqliteConnection, true);
            if (reporter != null)
            {
                reporter.Done();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origStrCn">connect string to access mdb</param>
        /// <param name="strCn">connect string to sqlite db</param>
        public static void ConvertMDBToSqlite(IDbBuilder oleDB, IDbBuilder sqlite, String origStrCn, String strCn, bool canCancel = false, Form parentForm = null)
        {
            try
            {
                //TODO DB convert from MDB

                ProgressDialog progressDialog = new ProgressDialog(canCancel);
                progressDialog.StartPosition = FormStartPosition.CenterParent;
                Thread th = new Thread(() =>
                {
                    DoConvertProgressive(oleDB, sqlite, origStrCn, strCn, progressDialog);
                });
                progressDialog.Load += (s, e) =>
                {
                    th.Start();
                };
                progressDialog.OnCancel += (s, e) =>
                {
                    try
                    {
                        if (th != null && th.IsAlive)
                        {
                            th.Abort();
                        }
                    }
                    catch (Exception ee)
                    {

                    }
                };
                progressDialog.Text = "轉換資料庫";
                progressDialog.ShowDialog(parentForm);
            }
            catch (Exception ee)
            {

            }




        }
        public override void SetDataAdapterLoadFillOption(IDbDataAdapter adapter, LoadOption option)
        {
            if (adapter is OleDbDataAdapter)
            {
                oleDB.SetDataAdapterLoadFillOption(adapter, option);
                return;
            }
            Selection.SetDataAdapterLoadFillOption(adapter, option);
        }

        public override void UpdateDataSet(IDbDataAdapter adapter, DataSet ds, string srcTable)
        {
            if (adapter is OleDbDataAdapter)
            {
                oleDB.UpdateDataSet(adapter, ds, srcTable);
                return;
            }
            Selection.UpdateDataSet(adapter, ds, srcTable);
        }

        public override int UpdateDataTable(IDbDataAdapter adapter, DataTable dt)
        {
            if (adapter is OleDbDataAdapter)
            {
                return oleDB.UpdateDataTable(adapter, dt);
            }
            return Selection.UpdateDataTable(adapter, dt);
        }

        public override void AddParamWithValue(DbParameterCollection paras, string key, string value)
        {
            if (paras is OleDbParameterCollection)
            {
                oleDB.AddParamWithValue(paras, key, value);
                return;
            }
            Selection.AddParamWithValue(paras, key, value);
        }
    }


}
