using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Excel
{
    public class ExcelExporter : ExcelFile
    {
        ExcelPackage ep;
        
        String sheetName;
        ExcelWorksheet workSheet;
        String holdFileName = null;
        bool createNew = false;
        private ExcelExporter(String filename,String sheetName="sheet1",bool createNew=false)
        {
            
          
            this.holdFileName = filename;
            this.sheetName = sheetName;
            this.createNew = createNew;
            if (!createNew)
            {
                ep = new ExcelPackage();
                if (File.Exists(filename))
                {
                    FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    ep.Load(fs);
                    fs.Close();
                }
                try
                {
                    this.workSheet = GetSheet(ep, sheetName);
                }
                catch (Exception ee)
                {

                }
                try
                {
                    if (this.workSheet == null)
                        this.workSheet = CreateSheet(ep, sheetName);
                }
                catch (Exception ee)
                {

                }
            }
            else
            {
                FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
                ep = new ExcelPackage(fs);
                if (this.workSheet == null)
                    this.workSheet = CreateSheet(ep, sheetName);
            }
        }
        private ExcelExporter()
        {

        }
        public static ExcelExporter OpenOrCreate(String filename,String sheetName="Kernel",bool alwaysCreate=false)
        {
            return new ExcelExporter(filename, sheetName, alwaysCreate);   
        }
        public void SetSheet(String sheetName)
        {
            try
            {
                this.workSheet = GetSheet(ep, sheetName);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                if (this.workSheet == null)
                {
                    this.workSheet = CreateSheet(ep, sheetName);
                }
            }catch(Exception ee){
                Console.WriteLine(ee.ToString());
            }
        }
        public void FitColumn(int colIdx)
        {
            this.workSheet.Column(colIdx).AutoFit();
        }
        public void SetCellNote(int row, int col, String note)
        {
            if (workSheet.Cells[row, col].Comment == null)
            {
                workSheet.Cells[row, col].AddComment(note, Environment.UserDomainName + "\\" + Environment.UserName);
            }
            else
            {
                workSheet.Cells[row, col].Comment.Author = Environment.UserDomainName + "\\" + Environment.UserName;
                workSheet.Cells[row, col].Comment.Text = note;
            }
        }
        public int AddColorRow(Color c,params String[] strings)
        {
            int endRow = 1;
            if (workSheet.Dimension != null)
            {
                endRow = workSheet.Dimension.Rows + 1;
            }
            for (int i = 0; i < strings.Length; ++i)
            {
                workSheet.Cells[endRow, i + 1].Value = strings[i];
                workSheet.Cells[endRow, i + 1].Style.Font.Color.SetColor(c);
            }
            return endRow;
        }
        public void SetRowBackColor(int row, Color c)
        {
            workSheet.Row(row).Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Row(row).Style.Fill.BackgroundColor.SetColor(c);
        }
        public void SetCellBackColor(int row,int col,Color c)
        {
            workSheet.Cells[row,col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(c);
        }
        public void SetCellFont(int row, int col, Font f)
        {
            workSheet.Cells[row, col].Style.Font.SetFromFont(f);
        }
        public void SetCellText(int row, int col, String val)
        {
            workSheet.Cells[row, col].Value = val;
        }
        public bool SetSheetByIndex(int idx)
        {
            ExcelWorksheet sheet = null;
            try
            {
                sheet = ep.Workbook.Worksheets[idx];
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            if (sheet != null)
            {
                workSheet = sheet;
                return true;
            }
            return false;
        }
        public String GetCellText(int row, int col)
        {
            return workSheet.Cells[row, col].Text;
        }
        public List<int> GetEmptyRow(int columnWidth = -1)
        {
            List<int> ret = new List<int>();
            int endrow = 0;
            if (columnWidth == -1)
            {
                columnWidth = workSheet.Dimension.Columns;
            }
            endrow = workSheet.Dimension.Rows;
            for (int i = endrow; i >= 1; --i)
            {
                bool isempty = true;
                for (int j = 1; j <= columnWidth; ++j)
                {
                    if (!String.IsNullOrEmpty(workSheet.Cells[i, j].Text))
                    {
                        isempty = false;
                        break;
                    }
                }
                if (isempty)
                {
                    ret.Add(i);
                }
            }
            return ret;
        }
        List<int> EmptyRow = null;
        public int AddToEmptyRow(params String[] strings)
        {
            if (EmptyRow == null)
            {
                EmptyRow = GetEmptyRow(strings.Length);
            }
            int endRow = 1;
            try
            {
                if (EmptyRow.Count > 0)
                {
                    endRow = EmptyRow[0];
                    EmptyRow.RemoveAt(0);
                }
                else
                {
                    if (workSheet.Dimension != null)
                    {
                        endRow = workSheet.Dimension.Rows + 1;
                    }
                }
                
                for (int i = 0; i < strings.Length; ++i)
                {
                    workSheet.Cells[endRow, i + 1].Value = strings[i];
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            return endRow;
        }
        public int AddRow(params String[] strings)
        {
            int endRow = 1;
            try
            {
                if (workSheet.Dimension != null)
                {
                    endRow = workSheet.Dimension.Rows + 1;
                }
                for (int i = 0; i < strings.Length; ++i)
                {
                    workSheet.Cells[endRow, i + 1].Value = strings[i];

                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            return endRow;
        }
        public void SetCellDimension(int row,int col,int width,int height)
        {
            workSheet.Row(row).Height = height;
            workSheet.Column(col).Width = width;
        }
        public void SetPicture(int row, int col, String picname,Bitmap bmp)
        {
            var pic =  workSheet.Drawings.AddPicture(picname, bmp);
            try
            {
                pic.SetSize((int)workSheet.Column(col).Width, (int)workSheet.Row(row).Height);
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            pic.SetPosition(row-1,0,col-1,0);
        }
        public void Close()
        {
            try
            {
                if (!createNew)
                {
                    File.WriteAllBytes(holdFileName, ep.GetAsByteArray());
                }
                else
                {
                    ep.Save();
                }
                ep.Dispose();
            }
            catch (Exception ee)
            {
                
            }
        }
    }
}
