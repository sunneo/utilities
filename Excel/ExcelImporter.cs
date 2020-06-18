using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.Excel
{
    public class ExcelImporter : ExcelFile, IDisposable, Interfaces.IExcelReader
    {
        ExcelPackage pkg;
        ExcelWorksheet currentSheet;
        public bool IsValid { get; private set; }
        public ExcelWorksheet GetSheet(String sheetName)
        {
            if (pkg == null) return null;
            return base.GetSheet(pkg, sheetName);
        }
        ExcelImporter(String filename)
        {
            byte[] filecontent = null;
            FileStream fstream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            filecontent = new byte[fstream.Length];
            fstream.Read(filecontent, 0, filecontent.Length);
            fstream.Close();
            MemoryStream ms = new MemoryStream(filecontent);
            pkg = FromStream(ms);
            this.currentSheet = GetSheetByIndex(1);
            this.IsValid = true;
        }
        public ExcelWorksheet GetSheetByIndex(int idx)
        {
            return pkg.Workbook.Worksheets[idx];
        }
        public void Close()
        {
        }
        public static ExcelImporter FromFile(String filename)
        {
            return new ExcelImporter(filename);
        }
        public void Dispose()
        {
            pkg.Dispose();
            pkg = null;
        }

        public string this[int row, int col]
        {
            get { return GetCellText(row, col); }
        }
        public int RowCount
        {
            get
            {
                if (currentSheet == null) { return 0; }
                return currentSheet.Dimension.Rows;
            }
        }
        public int ColumnCount
        {
            get
            {
                if (currentSheet == null) { return 0; }
                return currentSheet.Dimension.Columns;
            }
        }

        public static string GetSheetCellText(ExcelWorksheet sheet, int row, int col)
        {
            try
            {
                return sheet.Cells[row, col].Text;
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
                return "";
            }
        }
        public string GetCellText(int row, int col)
        {
            return GetSheetCellText(currentSheet, row, col);
        }

        public string CurrentSheetName
        {
            get { return currentSheet.Name; }
        }

        public void SelectSheetByName(string name)
        {
            currentSheet = GetSheet(name);
        }

        public void SelectSheetByIndex(int index)
        {
            currentSheet = GetSheetByIndex(index);
        }

        public int SheetCount
        {
            get 
            {
                try
                {
                    if (pkg == null || pkg.Workbook == null) return 0;
                    return pkg.Workbook.Worksheets.Count;
                }
                catch (Exception ee)
                {
                    return 0;
                }
            }
        }
    }
}
