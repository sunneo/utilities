using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Database
{
    public class DBFactory
    {
        static DBFactory instance;
        public static DBFactory Default
        {
            get
            {
                if (instance == null)
                {
                    instance = new DBFactory();
                }
                return instance;
            }
        }
        Dictionary<String, IDbConnection> connections = new Dictionary<string, IDbConnection>();

        private volatile IDbBuilder m_SqlBuilder;
        private volatile IDbBuilder m_OleDbBuilder;
        private volatile IDbBuilder m_SQLiteDbBuilder;
        private volatile IDbBuilder m_LocalDbBuilder;
        public IDbBuilder SQLBuilder
        {
            get
            {
                if (m_SqlBuilder == null)
                {
                    m_SqlBuilder = new SQLDBBuilder(this);
                }
                return m_SqlBuilder;
            }
        }
        Dictionary<String, String> converted = new Dictionary<string, string>();
        public String GetConvertedString(String strCn)
        {
            while (converted.ContainsKey(strCn))
            {
                String convertedStrCn = converted[strCn];
                if (String.IsNullOrEmpty(convertedStrCn) || convertedStrCn.Equals(strCn))
                {
                    break;
                }
                strCn = convertedStrCn;
            }
            return strCn;
        }
        public void SaveConvertedString(String left, String right)
        {
            converted[left] = right;
        }
        /// <summary>
        /// Use oledb
        /// </summary>
        public IDbBuilder OleDbBuilder
        {
            get
            {
                if (m_OleDbBuilder == null)
                {
                    m_OleDbBuilder = new OleDbBuilder(this);
                }
                return m_OleDbBuilder;
            }
        }
        /// <summary>
        /// Local DB
        /// </summary>
        public IDbBuilder LocalDbBuilder
        {
            get
            {
                if (m_LocalDbBuilder == null)
                {
                    m_LocalDbBuilder = new LocalDBBuilder(this);
                }
                return m_LocalDbBuilder;
            }
        }

        public IDbBuilder SQLiteDbBuilder
        {
            get
            {
                if (m_SQLiteDbBuilder == null)
                {
                    m_SQLiteDbBuilder = new SQLiteDBBuilder(this);
                }
                return m_SQLiteDbBuilder;
            }
        }
        public IDbBuilder DefaultBuilder
        {
            get
            {
                return SQLBuilder;
            }
        }
        object superLock = new object();
        Dictionary<String, Object> lockMap = new Dictionary<string, object>();
        public object GetLock(String str)
        {
            lock (superLock)
            {
                if (!lockMap.ContainsKey(str))
                {
                    lockMap[str] = new object();
                }
                return lockMap[str];
            }
        }

        public bool ContainsConnection(String str)
        {
            return connections.ContainsKey(str);
        }
        public IDbConnection GetConnection(String str)
        {
            IDbConnection ret = connections[str];

            return ret;

        }
        public void RemoveConnection(String str)
        {
            connections.Remove(str);
        }
        public void SaveConnection(String str, IDbConnection conn)
        {
            connections[str] = conn;
        }
    }


}
