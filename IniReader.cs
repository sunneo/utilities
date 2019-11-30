/*
* Copyright (c) 2019-2020 [Open Source Developer, Sunneo].
* All rights reserved.
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the [Open Source Developer, Sunneo] nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE [Open Source Developer, Sunneo] AND CONTRIBUTORS "AS IS" AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE [Open Source Developer, Sunneo] AND CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities.OptionParser.Attributes;

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
        public class ExampleClass2
        {
            public class InnerClass
            {
                public String StringField;
                public int IntField;
                public int IntField2;
                public double DoubleField;
                public Color colorField;
            }
            public InnerClass InnerClassField;
            public String StringField;
            public int IntField;
            public double DoubleField;

            [Category("不知道什麼類別")]
            public Color colorField;

            [Category("不知道什麼類別")]
            public Size SizeField;

            [Category("不知道什麼類別")]
            public Rectangle RectangleField;

            [Category("不知道什麼類別")]
            public Point PointField;
        }
        [STAThread]
        public static void Test2()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            IniConfigurationUI<ExampleClass2> ui = new IniConfigurationUI<ExampleClass2>("test.ini");
            Application.Run(ui.BuildForm());
        }

        public static void Main2()
        {
            Test2();
        }
        public static void Main()
        {
            Main2();
        }
        public Dictionary<String, List<String>> Data = new Dictionary<string, List<String>>();

        public Dictionary<String, String> FieldSectionMapper = new Dictionary<string, string>();
        public int[] GetIntsFromString(String name)
        {
            String s = GetString(name);
            if (String.IsNullOrEmpty(s))
            {
                if (String.IsNullOrEmpty(name))
                {
                    return null;
                }
                else
                {
                    if (name.IndexOf(',') > -1)
                    {
                        s = name;
                    }
                }
            }
            if (s.IndexOf(',') > -1)
            {
                String[] splits = s.Split(',');
                int len = splits.Length;
                int[] ret = new int[len];
                for (int i = 0; i < len; ++i)
                {
                    int.TryParse(splits[i], out ret[i]);
                }
                return ret;
            }
            else
            {
                int dummy = 0;
                int.TryParse(s, out dummy);
                return new int[] { dummy };
            }
            return null;
        }
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
        public class OnSerializeNotificationEventArgs
        {
            public IniReader Reader;
            public FieldInfo Field;
            public String FullName;
            public Object Target;
            public Object FieldValue;
            public String Section;
        }
        public static void DeserializeFields(IniReader reader, object ret, String prefix = "", EventHandler<OnSerializeNotificationEventArgs> OnSerializingMember = null)
        {
            Type t = ret.GetType();
            
            foreach (var field in t.GetFields())
            {
                Object FieldValue = null;
                OnSerializeNotificationEventArgs OnSerializeArgs = new OnSerializeNotificationEventArgs();
                OnSerializeArgs.Reader = reader;
                OnSerializeArgs.Field = field;
                OnSerializeArgs.Target = ret;
                if (field.IsLiteral) continue;
                NonSerializedAttribute[] nonSerialize = (NonSerializedAttribute[])field.GetCustomAttributes(typeof(NonSerializedAttribute), false);
                if (nonSerialize != null && nonSerialize.Length > 0)
                {
                    continue;
                }
                if (field.IsPublic)
                {
                    var fieldType = field.FieldType;
                    String name = prefix + field.Name;
                    IniFieldNameAttribute iniFieldName = (IniFieldNameAttribute)field.GetCustomAttribute(typeof(IniFieldNameAttribute), true);
                    if (iniFieldName != null && !String.IsNullOrEmpty(iniFieldName.Name))
                    {
                        name = iniFieldName.Name;
                    }
                    OnSerializeArgs.FullName = name;
                    
                    if (fieldType.IsPrimitive)
                    {
                        if (fieldType == typeof(int))
                        {
                            int val = reader.GetInt(name);
                            field.SetValue(ret, val);
                            FieldValue = val;
                        }
                        else if (fieldType == typeof(bool))
                        {
                            bool val = reader.GetBoolean(name);
                            field.SetValue(ret, val);
                            FieldValue = val;
                        }
                        else if (fieldType == typeof(double))
                        {
                            double val = reader.GetDouble(name);
                            field.SetValue(ret, val);
                            FieldValue = val;
                        }
                        else if (fieldType.IsEnum)
                        {
                            try
                            {
                                String sval = reader.GetString(name);
                                object enumVal = Enum.Parse(fieldType, sval);
                                field.SetValue(ret, enumVal);
                            }
                            catch (Exception)
                            {

                            }
                        }
                        
                    }
                    else if (fieldType == typeof(string))
                    {
                        object val = reader.GetString(name);
                        field.SetValue(ret, val);
                        FieldValue = val;
                    }
                    else if (fieldType == typeof(double[]))
                    {
                        String val = reader.GetString(name);
                        List<double> intList = DoubleListFromString(val);
                        field.SetValue(ret, intList.ToArray());
                        FieldValue = intList.ToArray();
                    }
                    else if (fieldType == typeof(int[]))
                    {
                        String val = reader.GetString(name);
                        List<int> intList = IntListFromString(val);
                        field.SetValue(ret, intList.ToArray());
                        FieldValue = intList.ToArray();
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
                            Color clrVal=Color.FromArgb(a, r, g, b);
                            field.SetValue(ret, clrVal);
                            FieldValue = clrVal;
                        }
                        else if (intList.Count == 3)
                        {
                            r = intList[0];
                            g = intList[1];
                            b = intList[2];
                            Color clrVal = Color.FromArgb(255, r, g, b);
                            field.SetValue(ret, clrVal);
                            FieldValue = clrVal;
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
                        FieldValue = val;
                    }
                    else if (fieldType.IsArray && fieldType.GetElementType().IsClass)
                    {
                        // for case of 
                        // SomeThing.Count=2
                        // SomeThing
                        FlattenArrayLengthName arrayLengthName = (FlattenArrayLengthName)field.GetCustomAttribute(typeof(FlattenArrayLengthName), true);
                        if (arrayLengthName != null && !String.IsNullOrEmpty(arrayLengthName.Name))
                        {
                            Type elementType = fieldType.GetElementType();
                            Array arrayInstance = Array.CreateInstance(elementType, reader.GetInt(arrayLengthName.Name));
                            FlattenArrayName arrayName = (FlattenArrayName)field.GetCustomAttribute(typeof(FlattenArrayName), true);
                            for (int i = 0; i < arrayInstance.Length; ++i)
                            {
                                String flattenArrayName = name + "[" + i.ToString() + "].";
                                if (arrayName != null && !String.IsNullOrEmpty(arrayName.Name) && !String.IsNullOrEmpty(arrayName.Replacement))
                                {
                                    flattenArrayName = arrayName.Name.Replace(arrayName.Replacement, i.ToString())+".";
                                }
                                var constructor = elementType.GetConstructor(new Type[] { });
                                object fieldContent = null;
                                fieldContent = field.GetValue(ret);
                                if (constructor != null)
                                {
                                    fieldContent = constructor.Invoke(new Object[] { });
                                    arrayInstance.SetValue(fieldContent, i);
                                }
                                DeserializeFields(reader, arrayInstance.GetValue(i), flattenArrayName, OnSerializingMember);
                            }
                            field.SetValue(ret, arrayInstance);
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
                        FieldValue = val;
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
                        FieldValue = val;
                    }
                    else if (fieldType.IsEnum)
                    {
                        try
                        {
                            String sval = reader.GetString(name);
                            object enumVal = Enum.Parse(fieldType, sval);
                            field.SetValue(ret, enumVal);
                            FieldValue = sval;
                        }
                        catch (Exception)
                        {

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

                        FieldValue = fieldContent;
                        if(fieldContent != null)
                        {
                            DeserializeFields(reader, fieldContent, field.Name + ".", OnSerializingMember);
                        }
                    }
                    OnSerializeArgs.FieldValue = FieldValue;
                    if (reader.FieldSectionMapper.ContainsKey(name))
                    {
                        OnSerializeArgs.Section = reader.FieldSectionMapper[name];
                    }
                    if (OnSerializingMember != null)
                    {
                        OnSerializingMember(reader, OnSerializeArgs);
                    }
                }
            }
        }
        public static T Deserialize<T>(String filename, EventHandler<OnSerializeNotificationEventArgs> OnSerializingMember = null)
        {
            IniReader reader = IniReader.FromFile(filename);
            Type t = typeof(T) ;
            var constructor = t.GetConstructor(new Type[] { });
            T ret = (T)constructor.Invoke(new object[] { });
            DeserializeFields(reader, ret,"",OnSerializingMember);
            return ret;
        }
        public static T DeserializeString<T>(String stringContent, EventHandler<OnSerializeNotificationEventArgs> OnSerializingMember = null)
        {
            IniReader reader = IniReader.FromString(stringContent);
            Type t = typeof(T);
            var constructor = t.GetConstructor(new Type[] { });
            T ret = (T)constructor.Invoke(new object[] { });
            DeserializeFields(reader, ret, "", OnSerializingMember);
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
        public static List<double> DoubleListFromString(String val)
        {
            if (val.IndexOf(',') > -1)
            {
                String[] splits = val.Split(',');
                List<double> list = new List<double>();
                for (int i = 0; i < splits.Length; ++i)
                {
                    String s = splits[i];
                    if (!String.IsNullOrEmpty(s))
                    {
                        double ival = 0;
                        if (double.TryParse(s, out ival))
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
            return new List<double>();
        }
        String CurrentCategory = "";
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
                            if (line[0] == '[')
                            {
                                String[] splits = line.Split('[', ']');
                                if (splits.Length > 1)
                                {
                                    String sec = splits[1];
                                    if (!CurrentCategory.Equals(sec))
                                    {
                                        CurrentCategory = sec;
                                    }
                                }
                            }
                            if (line.IndexOf('=') > -1)
                            {
                                int idx = line.IndexOf('=');
                                String k = line.Substring(0,idx).Trim();
                                if (idx + 1 < line.Length)
                                {
                                    String v = line.Substring(idx + 1).Trim();
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
                                    FieldSectionMapper[k] = CurrentCategory;
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
