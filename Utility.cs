using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Utilities
{
    public class Utility
    {
        public class JSON
        {
            public static T Deserialize<T>(String data)
            {
                try
                {
                    using (JsonTextReader txtReader = new JsonTextReader(new StringReader(data)))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        return serializer.Deserialize<T>(txtReader);
                    }
                }
                catch (Exception ex)
                {
                    return default(T);
                }
            }
            public static String Serialize(object o)
            {
                StringBuilder strb = new StringBuilder();
                using (JsonTextWriter writer = new JsonTextWriter(new StringWriter(strb)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, o);
                }
                return strb.ToString();
            }
        }
        public static StreamReader SharedStreamReader(String filepath, Encoding _encoding)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader freader = new StreamReader(fs, _encoding, true);
            return freader;
        }
        public static StreamReader SharedStreamReader(String filepath)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader freader = new StreamReader(fs, Encoding.Default, true);
            return freader;
        }
        public static StreamReader SharedUTF8StreamReader(String filepath)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader freader = new StreamReader(fs, Encoding.UTF8, true);
            return freader;
        }
        static LRUDictionary<Type, XmlSerializer> mTypePool;
        static LRUDictionary<Type, XmlSerializer> TypePool
        {
            get
            {
                if (mTypePool == null)
                {
                    mTypePool = new LRUDictionary<Type, XmlSerializer>(128);
                }
                return mTypePool;
            }
        }
        public static XmlSerializer GetTypeSerializer(Type type, params Type[] extraTypes)
        {
            XmlSerializer serializer = TypePool.Get(type);
            if (serializer == null)
            {
                serializer = new XmlSerializer(type, extraTypes);
                //serializer = XmlSerializer.FromTypes(new Type[] { type })[0];
                TypePool.Put(type, serializer);
            }
            return serializer;
        }

        public static string Serialize(object o,params Type[] extraTypes)
        {
            try
            {
                if (o != null)
                {
                    XmlSerializer ser = GetTypeSerializer(o.GetType(), extraTypes);
                    StringBuilder sb = new StringBuilder();
                    StringWriter writer = new StringWriter(sb);
                    ser.Serialize(writer, o);
                    return sb.ToString();
                }
                
            }
            catch (Exception ee)
            {

            }
            return "";

        }

        public static T Deserialize<T>(string s,params Type[] extra)
        {
            if (string.IsNullOrEmpty(s))
                return default(T);

            try
            {
                XmlSerializer ser = GetTypeSerializer(typeof(T),extra);
                using (var fileReader = new FileStream(s, FileMode.Open, FileAccess.Read))
                using(var sr = new StreamReader(fileReader))
                {
                    object obj = ser.Deserialize(sr);

                    return (T)obj;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return default(T);
            }
        }
        public static IEnumerable<String> EnumeratorStringToLines(String stringContent)
        {
            int length = stringContent.Length;
            StringBuilder strb = new StringBuilder();
            for (int i = 0; i < length; ++i)
            {
                char c = stringContent[i];
                if (c == '\n')
                {
                    yield return strb.ToString();
                    strb.Clear();
                    continue;
                }
                else if (c == '\r')
                {
                    if (i + 1 < length && stringContent[i + 1] == '\n')
                    {
                        yield return strb.ToString();
                        strb.Clear();
                        i += 1;
                        continue;
                    }
                }
                strb.Append(c);
            }
            if (strb.Length > 0)
            {
                yield return strb.ToString();
            }
            yield break;
        }
    }
}
