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
using System.Reflection;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.OptionParser.Attributes;

namespace Utilities
{
    public class IniWriter : IDisposable
    {
        protected volatile bool bIsDisposed = false;
        public class BasicWriter
        {
            public virtual void WriteRaw(String line)
            {

            }
            public virtual void Write(String key, String val)
            {

            }
            public virtual void Close()
            {

            }
            public virtual void Save()
            {

            }
            public virtual void Reset()
            {

            }
            public override string ToString()
            {
                return base.ToString();
            }
        }
        public class IniLineWriter : BasicWriter
        {
            public String FileName;
            public List<String> Lines = new List<string>();
            public override void WriteRaw(string line)
            {
                Lines.Add(line);
            }
            public override void Write(string key, string val)
            {
                Lines.Add(key + "=" + val);
            }
            public override void Save()
            {
                if (String.IsNullOrEmpty(FileName)) return;
                using (var fs = new FileStream(this.FileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var wr = new StreamWriter(fs))
                {
                    foreach (String s in Lines)
                    {
                        wr.WriteLine(s);
                    }
                }
            }
            public override void Close()
            {
                Save();
                Lines.Clear();
                FileName = "";
            }
            public override string ToString()
            {
                StringBuilder strb = new StringBuilder();
                foreach (String s in Lines)
                {
                    strb.AppendLine(s);
                }
                return strb.ToString();
            }
            public IniLineWriter(String filename)
            {
                this.FileName = filename;
            }
        }
        public class DictionaryWriter : BasicWriter
        {
            IDictionary<String, object> dict = null;

            public override void WriteRaw(string line)
            {

            }
            public override void Write(string key, string val)
            {
                dict[key] = val;
            }
            public override void Save()
            {

            }
            public override void Close()
            {

            }
            public override string ToString()
            {
                StringBuilder strb = new StringBuilder();
                foreach (String s in dict.Keys)
                {
                    strb.AppendLine(s + "=" + dict[s]);
                }
                return strb.ToString();
            }
            public DictionaryWriter(IDictionary<String, object> target)
            {
                this.dict = target;
            }
        }
        BasicWriter writer = null;
        public virtual BasicWriter StartIniLineWriter(String filename)
        {
            if (writer == null)
            {
                writer = new IniLineWriter(filename);
            }
            return writer;
        }
        public virtual BasicWriter StartDictionaryWriter(IDictionary<String, object> dict)
        {
            if (writer == null)
            {
                writer = new DictionaryWriter(dict);
            }
            return writer;
        }



        public Dictionary<String, Object> GivenValue = new Dictionary<string, object>();
        public static IniWriter Open(String file, bool appendFile = false)
        {
            IniWriter writer = new IniWriter();
            BasicWriter basicWriter = writer.StartIniLineWriter(file);
            if (File.Exists(file) && appendFile)
            {
                foreach (String line in File.ReadAllLines(file))
                {
                    basicWriter.WriteRaw(line);
                }
            }
            return writer;
        }
        public static String SerializeToString(Object o)
        {
            IniWriter writer = new IniWriter();
            writer.StartIniLineWriter("");
            writer.Serialize(o);
            return writer.ToString();
        }
        public static void SerializeToDictionary(IDictionary<String, object> dic, Object o)
        {
            IniWriter writer = new IniWriter();
            writer.StartDictionaryWriter(dic);
            writer.Serialize(o);
        }
        /// <summary>
        /// serialize to a given writer
        /// </summary>
        /// <param name="basicWriter"></param>
        /// <param name="o"></param>
        public static void SerializeToWriter(BasicWriter basicWriter, Object o)
        {
            IniWriter writer = new IniWriter();
            writer.writer = basicWriter;
            writer.Serialize(o);
        }

        public virtual void WriteCategory(String category)
        {
            if (this.writer == null) return;
            this.writer.WriteRaw("[" + category + "]");
        }
        public virtual void WriteComment(String content)
        {
            if (this.writer == null) return;
            StringReader reader = new StringReader(content);
            List<String> lines = new List<string>();
            int maxLength = 0;
            while (true)
            {
                String s = reader.ReadLine();
                if (s == null)
                {
                    break;
                }
                maxLength = Math.Max(maxLength, s.Length);
                lines.Add(s);
            }
            if (lines.Count == 1)
            {
                this.writer.WriteRaw("#  " + lines[0]);
            }
            else
            {
                String separator = "#".PadLeft(maxLength + 3, '#');
                this.writer.WriteRaw(separator);
                foreach (String s in lines)
                {
                    this.writer.WriteRaw("#  " + s);
                }
                this.writer.WriteRaw(separator);
            }
        }
        public virtual void Write(String key, String val)
        {
            if (this.writer == null) return;
            this.writer.Write(key, val);

        }
        public virtual void Write(String key, bool val)
        {
            this.Write(key, val.ToString());
        }
        public virtual void Write(String key, int val)
        {
            this.Write(key, val.ToString());
        }
        public virtual void Write(String key, double val)
        {
            this.Write(key, val.ToString());
        }
        public virtual void Write(String key, int[] val)
        {
            StringBuilder strb = new StringBuilder();
            for (int i = 0; i < val.Length; ++i)
            {
                String v = val[i].ToString();
                strb.Append(v);
                if (i + 1 < val.Length)
                {
                    strb.Append(',');
                }
            }
            this.Write(key, strb.ToString());
        }
        public virtual void Write(String key, double[] val)
        {
            StringBuilder strb = new StringBuilder();
            for (int i = 0; i < val.Length; ++i)
            {
                String v = val[i].ToString();
                strb.Append(v);
                if (i + 1 < val.Length)
                {
                    strb.Append(',');
                }
            }
            this.Write(key, strb.ToString());
        }
        public virtual void Write(String key, Point val)
        {
            this.Write(key, val.ToIntArray());
        }
        public virtual void Write(String key, Size val)
        {
            this.Write(key, val.ToIntArray());
        }
        public virtual void Write(String key, Rectangle val)
        {
            this.Write(key, val.ToIntArray());
        }
        public virtual void Write(String key, Color val)
        {
            this.Write(key, val.ToIntArray());
        }
        protected virtual void SerializeObject(object ret, String prefix)
        {
            Type t = ret.GetType();
            if (t == typeof(DBNull)) return;
            foreach (var field in t.GetFields())
            {
                try
                {

                    if (field.IsSpecialName) continue;

                    if (field.IsPublic)
                    {
                        var fieldType = field.FieldType;
                        if (fieldType == typeof(DBNull)) continue;
                        String name = prefix + field.Name;
                        object val = field.GetValue(ret);
                        NonSerializedAttribute[] nonSerialize = (NonSerializedAttribute[])field.GetCustomAttributes(typeof(NonSerializedAttribute), false);
                        if (nonSerialize != null && nonSerialize.Length > 0)
                        {
                            continue;
                        }
                        DescriptionAttribute[] attrs = (DescriptionAttribute[])field.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        if (attrs != null)
                        {
                            for (int i = 0; i < attrs.Length; ++i)
                            {
                                WriteComment(attrs[i].Description);
                            }
                        }
                        if (GivenValue != null && GivenValue.ContainsKey(name))
                        {
                            val = GivenValue[name];
                        }
                        if (fieldType.IsPrimitive)
                        {

                            if (fieldType == typeof(int))
                            {
                                Write(name, (int)val);
                            }
                            else if (fieldType == typeof(bool))
                            {
                                Write(name, (bool)val);
                            }
                            else if (fieldType == typeof(double))
                            {
                                Write(name, (double)val);
                            }
                            else if (fieldType.IsEnum)
                            {
                                Write(name, (string)val);
                            }
                        }
                        else if (fieldType == typeof(string))
                        {
                            Write(name, (String)val);
                        }
                        else if (fieldType == typeof(int[]))
                        {
                            Write(name, (int[])val);
                        }
                        else if (fieldType == typeof(double[]))
                        {
                            Write(name, (double[])val);
                        }
                        else if (fieldType == typeof(Color))
                        {
                            Write(name, (Color)val);
                        }
                        else if (fieldType == typeof(Size))
                        {
                            Write(name, (Size)val);
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
                                Array arrayInstance = (Array)val;
                                Write(arrayLengthName.Name, arrayInstance.Length);

                                FlattenArrayName arrayName = (FlattenArrayName)field.GetCustomAttribute(typeof(FlattenArrayName), true);
                                for (int i = 0; i < arrayInstance.Length; ++i)
                                {
                                    String flattenArrayName = name + "[" + i.ToString() + "].";
                                    if (arrayName != null && !String.IsNullOrEmpty(arrayName.Name) && !String.IsNullOrEmpty(arrayName.Replacement))
                                    {
                                        flattenArrayName = arrayName.Name.Replace(arrayName.Replacement, i.ToString()) + ".";
                                    }
                                    SerializeObject(arrayInstance.GetValue(i), flattenArrayName);
                                }
                            }
                        }
                        else if (fieldType == typeof(Rectangle))
                        {
                            Write(name, (Rectangle)val);
                        }
                        else if (fieldType == typeof(Point))
                        {
                            Write(name, (Point)val);
                        }
                        else if (fieldType.IsEnum)
                        {
                            Write(name, (string)val.ToString());
                        }
                        else if (fieldType.IsClass)
                        {
                            if (fieldType == typeof(Color))
                            {
                                Write(name, (Color)val);
                            }
                            else if (fieldType == typeof(Size))
                            {
                                Write(name, (Size)val);
                            }
                            else if (fieldType == typeof(Rectangle))
                            {
                                Write(name, (Rectangle)val);
                            }
                            else if (fieldType == typeof(Point))
                            {
                                Write(name, (Point)val);
                            }
                            object fieldContent = null;
                            fieldContent = field.GetValue(ret);
                            if (fieldContent != null)
                            {
                                WriteComment("Class:" + fieldType.Name + Environment.NewLine +
                                    "FieldName:" + field.Name + Environment.NewLine);
                                SerializeObject(fieldContent, field.Name + ".");
                            }
                        }
                    }
                }
                catch (Exception ee)
                {

                }

            }


        }
        public virtual void Serialize(object o)
        {
            SerializeObject(o, "");
        }
        public virtual void Save()
        {
            if (writer != null)
            {
                writer.Save();
            }

        }
        public virtual void Close()
        {
            if (writer != null)
            {
                writer.Close();
            }

        }
        public virtual void Dispose()
        {
            if (bIsDisposed) return;
            this.Close();
            bIsDisposed = true;
        }

        public override string ToString()
        {
            if (writer != null)
            {
                return writer.ToString();
            }
            return "";
        }

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
            //可以從物件Serialize到Ini
            ExampleClass example = new ExampleClass();

            example.StringField = "Hello World";
            example.IntField = 1234567890;
            example.DoubleField = 3.1415926;
            example.SizeField = new Size(100, 100);
            example.RectangleField = new Rectangle(1, 2, 3, 4);
            example.PointField = new Point(5, 6);
            example.InnerClassField = new ExampleClass.InnerClass();
            example.InnerClassField.StringField = "InnerClass String";
            example.InnerClassField.IntField = 1000;
            example.InnerClassField.IntField2 = 2000;
            example.InnerClassField.DoubleField = 3000;
            IniWriter writer = new IniWriter();
            writer.Serialize(example);
            String str = writer.ToString();

        }
        public static void Main()
        {
            Test();
        }

    }
    public static class Extensions
    {
        public static int[] ToIntArray(this Point pthis)
        {
            return new int[] { pthis.X, pthis.Y };
        }
        public static int[] ToIntArray(this Size pthis)
        {
            return new int[] { pthis.Width, pthis.Height };
        }
        public static int[] ToIntArray(this Rectangle pthis)
        {
            return new int[] { pthis.Left, pthis.Top, pthis.Width, pthis.Height };
        }
        public static int[] ToIntArray(this Color pthis)
        {
            return new int[] { pthis.A, pthis.R, pthis.G, pthis.B };
        }
    }
}
