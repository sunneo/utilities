using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Utilities;

namespace Utilities.Database
{
    public class TableToDatasetConverter
    {

        /// <summary>
        /// save dataset to a MDB
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="connStr"></param>
        public class DatasetSerialization
        {
            public static XmlSerializer serializer = new XmlSerializer(typeof(DatasetSerialization));
            public byte[] xmlSchema;
            public byte[] xml;

            public DatasetSerialization()
            {

            }
            public static DatasetSerialization FromDataSet(DataSet dataSet)
            {
                DatasetSerialization ret = new DatasetSerialization();
                using (StringWriter writer = new StringWriter())
                {
                    dataSet.WriteXmlSchema(writer);
                    ret.xmlSchema = Encoding.Default.GetBytes(writer.ToString());
                }
                using (StringWriter writer = new StringWriter())
                {
                    dataSet.WriteXml(writer);
                    ret.xml = Encoding.Default.GetBytes(writer.ToString());
                }
                return ret;
            }
            public DataSet ToDataSet()
            {
                DataSet ret = new DataSet();
                using (StringReader reader = new StringReader(Encoding.Default.GetString(xmlSchema)))
                {
                    ret.ReadXmlSchema(reader);
                }
                using (StringReader reader = new StringReader(Encoding.Default.GetString(xml)))
                {
                    ret.ReadXml(reader);
                }
                return ret;
            }
            public void SaveFile(String filename)
            {
                if (File.Exists(filename)) File.Delete(filename);
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    serializer.Serialize(fs, this);
                }
            }
            public static DatasetSerialization FromFile(String filename)
            {
                DatasetSerialization ret = null;
                if (!File.Exists(filename)) return ret;
                try
                {
                    using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        DatasetSerialization obj = (DatasetSerialization)serializer.Deserialize(fs);
                        if (obj != null)
                        {
                            ret = obj;
                        }
                    }
                }
                catch (Exception ee)
                {
                    Tracer.D(ee.ToString());
                }
                return ret;
            }

        }


    }
   
}
