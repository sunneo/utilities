using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Utilities
{
    public class MD5Dictionary
    {
        [Serializable]
        [XmlType(TypeName = "MutableKeyValuePair")]
        public struct MutableKeyValuePair<K, V>
        {
            public K Key
            { get; set; }

            public V Value
            { get; set; }
        }

        private static MD5Dictionary Instance;

        private System.Security.Cryptography.MD5 mMD5;
        private String mFilePath;
        private String mWorkingDictionary;
        private Dictionary<String, String> mDictionary = new Dictionary<string, string>();

        public static String FILE_NAME = "FileValidateMD5.xml";

        #region Constructor

        private MD5Dictionary()
        {
        }

        #endregion Constructor

        #region Private Methods

        private void Add(string _filePath, string _md5sum)
        {
            if (mDictionary == null)
                return;

            mDictionary.Add(_filePath, _md5sum);
        }

        #endregion Private Methods

        public static MD5Dictionary InitInstance(string _dictionaryPath)
        {
            if (_dictionaryPath == null)
            {
                _dictionaryPath = "md5sum";
            }
            if (Instance == null)
            {
                Instance = new MD5Dictionary();
            }

            Instance.Clear();
            Instance.mWorkingDictionary = _dictionaryPath;
            Instance.mFilePath = Path.Combine(Instance.mWorkingDictionary, FILE_NAME);
            Instance.Load();
            Instance.mMD5 = System.Security.Cryptography.MD5.Create();

            return Instance;
        }
        public static MD5Dictionary GetInstance()
        {
            if (Instance == null)
            {
                StackFrame stkFrame = new StackFrame(1, true);
                Debug.WriteLine("[MD5Dictionary.GetInstance()] When GetInstance(), instance is null ... calls from File: {0} Line: {1} Method: {2}", stkFrame.GetFileName(), stkFrame.GetFileLineNumber(), stkFrame.GetMethod().ToString());
                InitInstance(null);
            }
            return Instance;
        }
        public static bool AddMD5Sum(string _filePath)
        {
            MD5Dictionary instance = GetInstance();
            if (instance == null)
                return false;

            lock (instance)
            {
                return instance.AddFromFile(_filePath);
            }
        }
        public static void RemoveMD5Sum(string _filePath)
        {
            MD5Dictionary instance = GetInstance();
            if (instance == null)
                return;

            lock (instance)
            {
                instance.Remove(_filePath);
            }
        }

        public Boolean ContainsKey(String _filePath)
        {
            if (string.IsNullOrEmpty(_filePath))
                return false;


            return mDictionary == null ? false : mDictionary.ContainsKey(_filePath);
        }
        public static String GetMD5Hash(String _filePath)
        {
            FileStream sr = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            System.Security.Cryptography.MD5 MD5 = null;
            if (Instance != null)
            {
                MD5 = Instance.mMD5;
            }
            else
            {
                MD5 = System.Security.Cryptography.MD5.Create();
            }
            {
                byte[] hash = Instance.mMD5.ComputeHash(sr);
                sr.Close();
                string hashCode = Convert.ToBase64String(hash);
                return hashCode;
            }
            return "";
        }
        public Boolean AddFromFile(String _filePath)
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine("MD5Dictionary.AddFromFile() No such file or directory:{0}", _filePath);
                return false;
            }


            try
            {
                FileStream sr = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] hash = mMD5.ComputeHash(sr);
                sr.Close();

                string hashCode = Convert.ToBase64String(hash);
                if (mDictionary.ContainsKey(_filePath))
                {
                    string val = mDictionary[_filePath];
                    if (val.Equals(hashCode))
                    {
                        return false;
                    }
                    else
                    {
                        mDictionary[_filePath] = val;
                        return true;
                    }
                }

                Add(_filePath, hashCode);

                return true;
            }
            catch
            {
                Console.WriteLine("MD5Dictionary.AddFromFile() can not open file:{0}", _filePath);
            }

            return false;
        }
        public Boolean UpdateByFile(String _filePath)
        {
            if (string.IsNullOrEmpty(_filePath))
                return false;


            if (!ContainsKey(_filePath))
                return false;

            try
            {
                FileStream sr = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] hash = mMD5.ComputeHash(sr);
                sr.Close();

                string hashCode = Convert.ToBase64String(hash);
                if (mDictionary[_filePath].Equals(hashCode))
                {
                    return true;
                }
                else
                {
                    Remove(_filePath);
                }

                Add(_filePath, hashCode);

                Store();

                return true;
            }
            catch
            {
                Console.WriteLine("MD5Dictionary.AddFromFile() can not open file:{0}", _filePath);
            }

            return false;
        }

        public List<string> GetFiles()
        {
            return mDictionary == null ? new List<string>() : mDictionary.Keys.ToList();
        }

        public object this[string k]
        {
            get { return mDictionary[k]; }
            set { this.Add(k, (string)value); }
        }
        public void Remove(string _filePath)
        {
            if (string.IsNullOrEmpty(_filePath))
                return;


            if (!ContainsKey(_filePath))
                return;

            mDictionary.Remove(_filePath);
        }
        public void Clear()
        {
            mDictionary.Clear();
        }
        public void Load()
        {
            lock (this)
            {
                Clear();

                if (!File.Exists(mFilePath))
                    return;

                try
                {

                    XmlSerializer xml = MutableKVPairListSerializer;
                    StreamReader sr = Utility.SharedStreamReader(mFilePath);

                    List<MutableKeyValuePair<string, string>> list = (List<MutableKeyValuePair<string, string>>)xml.Deserialize(sr);
                    foreach (MutableKeyValuePair<string, string> kv in list)
                    {
                        if (!mDictionary.ContainsKey(kv.Key))
                            mDictionary.Add(kv.Key, kv.Value);
                        else
                            mDictionary[kv.Key] = kv.Value;
                    }

                    sr.Close();
                }
                catch
                {
                }
            }
        }
        static XmlSerializer mMutableKVPairListSerializer = null;
        XmlSerializer MutableKVPairListSerializer
        {
            get
            {
                if (mMutableKVPairListSerializer == null)
                {
                    mMutableKVPairListSerializer = Utility.GetTypeSerializer(typeof(List<MutableKeyValuePair<string, string>>));
                }
                return mMutableKVPairListSerializer;
            }
        }
        public void Store()
        {
            lock (this)
            {
                XmlSerializer xml = MutableKVPairListSerializer;

                List<MutableKeyValuePair<string, string>> list = new List<MutableKeyValuePair<string, string>>();
                foreach (string k in mDictionary.Keys)
                {
                    MutableKeyValuePair<string, string> kv = new MutableKeyValuePair<string, string>();
                    kv.Key = k;
                    kv.Value = mDictionary[k];
                    list.Add(kv);
                }

                try
                {
                    StreamWriter sw = new StreamWriter(mFilePath);
                    xml.Serialize(sw, list);
                    sw.Close();
                }
                catch
                {
                }
            }
        }
        public String GetFilePath() { return mFilePath; }
    }
}
