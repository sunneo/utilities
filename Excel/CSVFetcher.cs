using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Utilities.Excel
{
    public class CSVFetcher
    {
        public DataTable datatable = null;
        private String LoadFileContent(String filename)
        {
            FileInfo finfo = new FileInfo(filename);
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] buf = new byte[finfo.Length];
            fs.Read(buf, 0, buf.Length);
            fs.Close();
            return Encoding.UTF8.GetString(buf);
        }
        private bool LoadFile(String filename)
        {
            if (!File.Exists(filename))
            {
                return false;
            }

            try
            {
                String fileContent = LoadFileContent(filename);
                datatable = new DataTable(Path.GetFileNameWithoutExtension(filename));
                String[] lines = fileContent.Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    for (int i = 0; i < lines.Length; ++i)
                    {
                        String line = lines[i].Trim();
                        String[] seps = line.Split(',');
                        if (seps.Length > 0)
                        {
                            DataRow row = datatable.NewRow();
                            int lastJ = 0;
                            for (int j = 0; j < seps.Length; ++j)
                            {
                                lastJ = j;
                                try
                                {
                                    String token = seps[j].Trim();
                                    if (i == 0)
                                    {
                                        datatable.Columns.Add(j.ToString());
                                    }
                                    row[j] = token;
                                }
                                catch (Exception ee)
                                {
                                    Console.WriteLine(ee.ToString());
                                }
                            }
                            datatable.Rows.Add(row);
                        }
                    }

                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
                return false;
            }
            return true;
        }
        CSVFetcher()
        {

        }

        public static CSVFetcher FromFile(String filename)
        {
            CSVFetcher ret = new CSVFetcher();
            if (!ret.LoadFile(filename))
            {
                return null;
            }
            return ret;
        }
    }
}
