using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class IniReader
    {
        public class ExampleClass
        {
            public class InnerClass
            {
                public String StringField;
                public int IntField;
                public int IntField2;
                public double DoubleField;
            }
            public InnerClass InnerClassField;
            public String StringField;
            public int IntField;
            public double DoubleField;
            public Size SizeField;
            public Rectangle RectangleField;
            public Point PointField;
        }
        public static void Test()
        {
            //可以從字串Deserialize到物件
            ExampleClass example = IniReader.DeserializeString<ExampleClass>(
                "StringField=Hello World"+Environment.NewLine+
                "IntField=1234567890"+Environment.NewLine+
                "DoubleField=3.1415926"+Environment.NewLine+
                "SizeField=100,100"+Environment.NewLine+
                "RectangleField=1,2,3,4"+Environment.NewLine+
                "PointField=5,6"+Environment.NewLine+
                "InnerClassField.StringField=InnerClass String"+Environment.NewLine+
                "InnerClassField.IntField=1000"+Environment.NewLine+
                "InnerClassField.IntField2=2000"+Environment.NewLine+
                "InnerClassField.DoubleField=3000"+Environment.NewLine
                );

        }
        public static void Main()
        {
            Test();
        }
        public Dictionary<String, List<String>> Data = new Dictionary<string, List<String>>();

        public double GetDouble(String name, double defaultValue = 0)
        {
            List<String> list = GetList(name);
            double ret = defaultValue;
            if (list.Count > 0)
            {
                double.TryParse(list[0], out ret);
            }
            return ret;
        }
        public int GetInt(String name, int defaultValue = 0)
        {
            List<String> list = GetList(name);
            int ret = defaultValue;
            if (list.Count > 0)
            {
                int.TryParse(list[0], out ret);
            }
            return ret;
        }
        public List<String> GetList(String s)
        {
            List<String> list = new List<string>();
            if (Data.ContainsKey(s))
            {
                list = Data[s];
            }
            return list;
        }
        public bool GetBoolean(String s,bool defaultValue=false)
        {
            String str = GetString(s);
            if (String.IsNullOrEmpty(str)) return defaultValue;
            bool ret = defaultValue;
            if (bool.TryParse(str, out ret))
            {
                return ret;
            }
            else
            {
                return defaultValue;
            }
            
        }
        public String GetString(String s, String defaultValue = "")
        {
            List<String> list = GetList(s);
            String ret = "";
            if (list.Count > 0)
            {
                if (list.Count == 1)
                {
                    ret = list[0];
                }
                else
                {
                    StringBuilder strb = new StringBuilder();
                    for (int i = 0; i < list.Count; ++i)
                    {
                        strb.Append(list[i] + Environment.NewLine);
                    }
                    ret = strb.ToString();
                }
            }
            else
            {
                return defaultValue;
            }
            return ret;
        }
        private IniReader()
        {

        }
        public static void DeserializeFields(IniReader reader, object ret,String prefix="")
        {
            Type t = ret.GetType();
            foreach (var field in t.GetFields())
            {
                if (field.IsPublic)
                {
                    var fieldType = field.FieldType;
                    String name = prefix + field.Name;
                    if (fieldType.IsPrimitive)
                    {
                        if (fieldType == typeof(int))
                        {
                            object val = reader.GetInt(name);
                            field.SetValue(ret, val);
                        }
                        else if (fieldType == typeof(bool))
                        {
                            object val = reader.GetBoolean(name);
                            field.SetValue(ret, val);
                        }
                        else if (fieldType == typeof(double))
                        {
                            object val = reader.GetDouble(name);
                            field.SetValue(ret, val);
                        }
                        
                        
                    }
                    else if (fieldType == typeof(string))
                    {
                        object val = reader.GetString(name);
                        field.SetValue(ret, val);
                    }
                    else if (fieldType == typeof(int[]))
                    {
                        String val = reader.GetString(name);
                        List<int> intList = IntListFromString(val);
                        field.SetValue(ret, intList.ToArray());
                    }
                    else if (fieldType == typeof(Color))
                    {
                        String val = reader.GetString(name);
                        List<int> intList = IntListFromString(val);
                        int a = 0;
                        int r = 0;
                        int g = 0;
                        int b = 0;
                        if (intList.Count == 4)
                        {
                            a = intList[0];
                            r = intList[1];
                            g = intList[2];
                            b = intList[3];
                            field.SetValue(ret, Color.FromArgb(a, r, g, b));
                        }
                        else if (intList.Count == 3)
                        {
                            r = intList[0];
                            g = intList[1];
                            b = intList[2];
                            field.SetValue(ret, Color.FromArgb(255, r, g, b));
                        }
                    }
                    else if (fieldType == typeof(Size))
                    {
                        String val = reader.GetString(name);
                        List<int> intList = IntListFromString(val);
                        int w = 0;
                        int h = 0;
                        if (intList.Count == 2)
                        {
                            w = intList[0];
                            h = intList[1];
                            field.SetValue(ret, new Size(w, h));
                        }
                    }
                    else if (fieldType == typeof(Rectangle))
                    {
                        String val = reader.GetString(name);
                        List<int> intList = IntListFromString(val);
                        int x = 0;
                        int y = 0;
                        int w = 0;
                        int h = 0;
                        if (intList.Count == 4)
                        {
                            x = intList[0];
                            y = intList[1];
                            w = intList[2];
                            h = intList[3];
                            field.SetValue(ret, new Rectangle(x, y, w, h));
                        }
                    }
                    else if (fieldType == typeof(Point))
                    {
                        String val = reader.GetString(name);
                        List<int> intList = IntListFromString(val);
                        int x = 0;
                        int y = 0;
                        if (intList.Count == 2)
                        {
                            x = intList[0];
                            y = intList[1];
                            field.SetValue(ret, new Point(x, y));
                        }
                    }
                    else if(fieldType.IsClass)
                    {
                        var constructor=fieldType.GetConstructor(new Type[] { });
                        object fieldContent = null;
                        fieldContent = field.GetValue(ret);
                        if (fieldContent == null)
                        {
                            if (constructor != null)
                            {
                                fieldContent = constructor.Invoke(new Object[] { });
                                field.SetValue(ret, fieldContent);
                            }
                        }
                        
                        
                        if(fieldContent != null)
                        {
                            DeserializeFields(reader, fieldContent, field.Name + ".");
                        }
                    }
                }
            }
        }
        public static T Deserialize<T>(String filename)
        {
            IniReader reader = IniReader.FromFile(filename);
            Type t = typeof(T) ;
            var constructor = t.GetConstructor(new Type[] { });
            T ret = (T)constructor.Invoke(new object[] { });
            DeserializeFields(reader, ret);
            return ret;
        }
        public static T DeserializeString<T>(String stringContent)
        {
            IniReader reader = IniReader.FromString(stringContent);
            Type t = typeof(T);
            var constructor = t.GetConstructor(new Type[] { });
            T ret = (T)constructor.Invoke(new object[] { });
            DeserializeFields(reader, ret);
            return ret;
        }
        public static List<int> IntListFromString(String val)
        {
            if (val.IndexOf(',') > -1)
            {
                String[] splits = val.Split(',');
                List<int> list = new List<int>();
                for (int i = 0; i < splits.Length; ++i)
                {
                    String s = splits[i];
                    if (!String.IsNullOrEmpty(s))
                    {
                        int ival = 0;
                        if (int.TryParse(s, out ival))
                        {
                            list.Add(ival);
                        }
                        else
                        {
                            list.Add(0);
                        }
                    }
                }
                return list;
            }
            return new List<int>();
        }
        private void ParseStream(TextReader fs)
        {
            {
                try
                {
                    while (true)
                    {
                        string line = fs.ReadLine();
                        if (line == null) break;
                        line = line.Trim();
                        if (!String.IsNullOrEmpty(line))
                        {
                            if (line[0] == '#') continue;
                            if (line.IndexOf('=') > -1)
                            {
                                String[] splits = line.Split('=');
                                if (splits.Length > 1)
                                {
                                    String k = splits[0].Trim();
                                    String v = splits[1].Trim();
                                    List<String> list = null;
                                    if (Data.ContainsKey(k))
                                    {
                                        list = Data[k];
                                    }
                                    else
                                    {
                                        list = new List<string>();
                                        Data[k] = list;
                                    }
                                    list.Add(v);
                                }
                            }
                        }
                        if (line == null)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee.ToString());
                }
            }
        }
        private void ParseFile(String IniFilePath)
        {
            using (StreamReader fs = new StreamReader(new BufferedStream(new FileStream(IniFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))))
            {
                ParseStream(fs);
            }
        }
        private void ParseString(String stringContent)
        {
            using (StringReader fs = new StringReader(stringContent))
            {
                ParseStream(fs);
            }
        }
        public static IniReader FromString(String stringContent)
        {
            IniReader ret = new IniReader();
            ret.ParseString(stringContent);
            return ret;
        }
        public static IniReader FromFile(String filename)
        {
            IniReader ret = new IniReader();
            if (File.Exists(filename))
            {
                ret.ParseFile(filename);
            }
            return ret;
        }
    }
}
