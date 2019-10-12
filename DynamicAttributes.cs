using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities
{
    public class DynamicAttributes
    {
        public Dictionary<String, object> Attributes = new Dictionary<string, object>();
        Locker mLocker = null;
        bool locked = true;
        public DynamicAttributes(bool bLocked = true)
        {
            this.locked = bLocked;
            mLocker = new Locker(this.locked);
        }
        public void Clear()
        {
            mLocker.Synchronized(() =>
            {
                Attributes.Clear();
            });
        }
        public int Count
        {
            get
            {
                return mLocker.Synchronized(() =>Attributes.Count);
            }
        }
        public object this[String idx]
        {
            get
            {
                return GetAttribute(idx);
            }
            set
            {
                SetAttribute(idx, value);
            }
        }
        public void RemoveAttribute(String key)
        {
            mLocker.Synchronized(() =>
            {
                if (Attributes.ContainsKey(key))
                    Attributes.Remove(key);
            });

        }
        public void SetAttribute(String key, object val)
        {
            mLocker.Synchronized(() =>
            {
                Attributes[key] = val;
            });


        }
        public bool HasAttribute(String key)
        {
            return mLocker.Synchronized(() =>
            {
                return Attributes.ContainsKey(key);
            });

        }
        public object GetAttribute(String key)
        {
            return mLocker.Synchronized(() =>
            {
                if (!Attributes.ContainsKey(key))
                {
                    return null;
                }
                return Attributes[key];
            });

        }
        public int GetAttributeInt(String key, int defaultVal = 0)
        {
            return mLocker.Synchronized(() =>
            {
                if (!Attributes.ContainsKey(key))
                {
                    return defaultVal;
                }
                return (int)Attributes[key];
            });

        }
        public bool GetAttributeBool(String key, bool defaultVal = false)
        {
            return mLocker.Synchronized(() =>
            {
                if (!Attributes.ContainsKey(key))
                {
                    return defaultVal;
                }
                return (bool)Attributes[key];
            });

        }
        public bool GetAttributeStringToBool(String key, String defaultVal = "")
        {
            return mLocker.Synchronized(() =>
            {
                String res = "";
                if (!Attributes.ContainsKey(key))
                {
                    res = defaultVal;
                }
                else
                {
                    res = (String)Attributes[key].ToString();
                }
                bool bret = false;
                bool.TryParse(res, out bret);
                return bret;
            });

        }
        public String GetAttributeString(String key, String defaultVal = "")
        {
            return mLocker.Synchronized(() =>
            {
                if (!Attributes.ContainsKey(key))
                {
                    return defaultVal;
                }
                return (String)Attributes[key].ToString();
            });

        }
    }
}
