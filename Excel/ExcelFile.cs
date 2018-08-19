using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Excel
{
    public class ExcelFile
    {
        public ExcelWorksheet CreateSheet(ExcelPackage p, string sheetName)
        {
            ExcelWorksheet ws = p.Workbook.Worksheets.Add(sheetName);
            //ws.Name = sheetName; //Setting Sheet's name
            ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
            ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet
            return ws;
        }
        protected ExcelWorksheet GetSheet(ExcelPackage p, String sheetName)
        {
            try
            {
                int sheetsCnt = p.Workbook.Worksheets.Count;
                for (int i = 1; i <= sheetsCnt; ++i)
                {
                    if (p.Workbook.Worksheets[i].Name.Equals(sheetName))
                    {
                        return p.Workbook.Worksheets[i];
                    }
                }
            }
            catch (Exception ee)
            {

                Console.WriteLine(ee.ToString());
            }
            return null;
        }
        protected ExcelPackage FromStream(Stream s)
        {
            return new ExcelPackage(s);
        }
        
    }
}
