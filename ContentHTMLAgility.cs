using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class ContentHTMLAgility : IDisposable
    {
        HtmlAgilityPack.HtmlDocument document;
        public ContentHTMLAgility(String html)
        {
            document = new HtmlAgilityPack.HtmlDocument();
            document.LoadHtml(html);
        }
        public HtmlAgilityPack.HtmlNodeCollection GetElementById(String id)
        {
            return SelectNodes("//*[@name='" + id + "']");
        }
        public HtmlAgilityPack.HtmlNodeCollection SelectNodes(String path)
        {
            return document.DocumentNode.SelectNodes(path);
        }
        public HtmlAgilityPack.HtmlNodeCollection GetElementsByClassNameLike(String className)
        {
            return document.DocumentNode.SelectNodes("//*[contains(concat(' ',@class,' '),'" + className + "'  )]");
        }
        public HtmlAgilityPack.HtmlNodeCollection GetElementsByClassName(String className)
        {
            return document.DocumentNode.SelectNodes("//*[contains(@class,'" + className + "')]");
        }
        public HtmlAgilityPack.HtmlNodeCollection GetElementsByTagName(String tagName)
        {
            return document.DocumentNode.SelectNodes("//" + tagName);
        }
        private class RowSpanRecord
        {
            public String data;
            public int rowSpan;
            public int rowIdx;
        }
        private List<String[]> fetchTableWithoutRowSpan(HtmlAgilityPack.HtmlNode table)
        {
            List<String[]> ret = new List<String[]>();
            //Console.WriteLine("Serialize Table {0}", table);
            //Console.WriteLine("Table InnerHTML:{0}", invokeScript(wb, table + ".innerHTML"));
            try
            {
                HtmlAgilityPack.HtmlNodeCollection rows = table.SelectNodes("tr");
                int goProcessLen = rows.Count;
                //Console.WriteLine("goProcessLen={0}", goProcessLen);
                RowSpanRecord[] rowSpanRecord = null;
                for (int i = 0; i < goProcessLen; ++i) // foreach row in table.rows
                {
                    HtmlAgilityPack.HtmlNodeCollection cells = rows[i].SelectNodes("td|th");
                    int columnLen = cells.Count;
                    if (columnLen == 0) continue;
                    if (rowSpanRecord == null)
                    {
                        rowSpanRecord = new RowSpanRecord[columnLen];
                    }
                    else
                    {
                        if (columnLen > rowSpanRecord.Length)
                        {
                            RowSpanRecord[] newRowSpan = new RowSpanRecord[columnLen];
                            for (int j = 0; j < newRowSpan.Length; ++j)
                            {
                                newRowSpan[j] = rowSpanRecord[j];
                            }
                            rowSpanRecord = newRowSpan;
                        }
                    }

                    String[] cellBucket = new String[rowSpanRecord.Length];
                    // pre-fill if it has rowspan
                    for (int j = 0; j < rowSpanRecord.Length; ++j) // foreach cells in row.cells
                    {
                        if (rowSpanRecord[j] != null) continue;
                        int rowspan = 1;
                        if (j < cells.Count && cells[j].HasAttributes)
                        {

                            HtmlAgilityPack.HtmlAttribute rowspan_attr = cells[j].Attributes["rowspan"];
                            if (rowspan_attr != null)
                            {
                                if (!int.TryParse(rowspan_attr.Value, out rowspan))
                                {
                                    rowspan = 1;
                                }
                            }
                        }
                        if (rowspan > 1)
                        {
                            RowSpanRecord rowspanRec = new RowSpanRecord();
                            rowSpanRecord[j] = rowspanRec;
                            rowspanRec.rowSpan = rowspan;
                            rowspanRec.rowIdx = i;
                            rowspanRec.data = HtmlAgilityPack.HtmlEntity.DeEntitize(cells[j].InnerText).Trim();
                        }
                    }
                    int dataIndex = 0;
                    // fill data into bucket
                    for (int j = 0; j < rowSpanRecord.Length; ++j)
                    {
                        // if exist rowspan in the position, fetch data from rowspanRecord.
                        if (rowSpanRecord[j] != null)
                        {
                            cellBucket[j] = rowSpanRecord[j].data;
                            --rowSpanRecord[j].rowSpan;
                            if (rowSpanRecord[j].rowIdx == i) ++dataIndex;
                            if (rowSpanRecord[j].rowSpan <= 0)
                            {
                                rowSpanRecord[j] = null;
                            }
                        }
                        else
                        {
                            cellBucket[j] = HtmlAgilityPack.HtmlEntity.DeEntitize(cells[dataIndex].InnerText).Trim();
                            ++dataIndex; // increase dataIndex
                        }
                    }
                    //Console.WriteLine("Add cellBucket({0}) to list",ret.Count);
                    ret.Add(cellBucket);
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            return ret;
        }
        public List<KeyValuePair<int,int>> GetTableHeaderIndexes(HtmlAgilityPack.HtmlNode table, params String[] vals)
        {
            List<KeyValuePair<int, int>> ret = new List<KeyValuePair<int, int>>();
            HtmlAgilityPack.HtmlNodeCollection rows = table.SelectNodes("tr");
            HtmlAgilityPack.HtmlNodeCollection cells = null;
            if (rows != null)
            {
                HtmlAgilityPack.HtmlNode header = rows[0];
                cells = header.SelectNodes("th|td");
            }
            else
            {
                HtmlAgilityPack.HtmlNodeCollection thead = table.SelectNodes("thead");
                if (thead != null)
                {
                    rows = thead[0].SelectNodes("tr");
                    cells = rows[0].SelectNodes("th|td");
                }
            } 
            
            
            int colAbsIdx = 0;
            for (int i = 0; i < cells.Count; ++i)
            {
                HtmlAgilityPack.HtmlNode cell = cells[i];
                String txt = cell.InnerText.Trim();
                for (int j = 0; j < vals.Length; ++j)
                {
                    String match = vals[j];
                    if (txt.IndexOf(match) > -1)
                    {
                        ret.Add(new KeyValuePair<int, int>(colAbsIdx, j));
                        break;
                    }
                }
                HtmlAgilityPack.HtmlAttribute colspan_attr = cell.Attributes["colspan"];
                int colSpan = 1;
                if (colspan_attr != null)
                {
                    if (!int.TryParse(colspan_attr.Value, out colSpan))
                    {
                        colSpan = 1;
                    }
                } 
                colAbsIdx += colSpan;
            }
            return ret;
        }
        public HtmlAgilityPack.HtmlNode findTableByHeaderPattern(params String[] vals)
        {
            HtmlAgilityPack.HtmlNodeCollection tables = GetElementsByTagName("table");
            if (tables == null) return null;
            foreach (HtmlAgilityPack.HtmlNode table in tables)
            {
                HtmlAgilityPack.HtmlNodeCollection rows = table.SelectNodes("tr");
                HtmlAgilityPack.HtmlNodeCollection cells=null;
                if(rows != null)
                {
                   HtmlAgilityPack.HtmlNode header = rows[0];
                   cells = header.SelectNodes("th|td");
                }
                else
                {
                    HtmlAgilityPack.HtmlNodeCollection thead = table.SelectNodes("thead");
                    if (thead != null)
                    {
                        rows = thead[0].SelectNodes("tr");
                        cells = rows[0].SelectNodes("th|td");
                    }
                } 
                int len = cells.Count;
                int valIdx = 0;
                int matchCnt = 0;
                for (int i = 0; i < len; ++i)
                {
                    if (i >= cells.Count) break;
                    if (valIdx >= vals.Length) break;
                    if (cells[i].InnerText.IndexOf(vals[valIdx]) != -1)
                    {
                        matchCnt += 1;
                        ++valIdx;
                    }
                    else
                    {
                        valIdx = 0;
                        matchCnt = 0;
                    }
                }
                if (matchCnt == vals.Length)
                {
                    return table;
                }
            }
            return null;
        }
        public List<String[]> getTableContentByHeaderPattern(params String[] vals)
        {
            List<String[]> ret = new List<string[]>();
            HtmlAgilityPack.HtmlNode table = findTableByHeaderPattern(vals);
            if (table != null)
            {
                ret = fetchTableWithoutRowSpan(table);
            }
            return ret;
        }
       



        ~ContentHTMLAgility()
        {

        }
        public void Dispose()
        {

        }

    }
    public static class ExtensionAgility
    {
        public static HtmlAgilityPack.HtmlNodeCollection GetElementsByClassName(this HtmlAgilityPack.HtmlNode pthis, String className)
        {
            return pthis.SelectNodes("//*[contains(@class,'" + className + "')]");
        }
    }
}
