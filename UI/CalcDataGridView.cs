using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities.UI
{
    public class CalcDataGridView : DoubleBufferDataGridView
    {
        [DefaultValue(typeof(Color), "0x0000FF")]
        public Color ColorEditingFore { get; set; }
        [DefaultValue(typeof(Color), "0xFFFFFF")]
        public Color ColorEditingBack { get; set; }
        [DefaultValue(typeof(Color), "0x000000")]
        public Color ColorNormalFore { get; set; }
        [DefaultValue(typeof(Color), "0xFFFFFF")]
        public Color ColorNormalBack { get; set; }
        ExcelPackage excel;

        public class OnExceptionOccurredEventArgs:EventArgs
        {
            public System.Exception Exception;
        }
        public event EventHandler<OnExceptionOccurredEventArgs> OnException;

        public CalcDataGridView()
        {
            excel = new ExcelPackage(new MemoryStream());
            excel.Workbook.Worksheets.Add("Sheet1");
            this.DoubleBuffered = true;
            SetStyle(System.Windows.Forms.ControlStyles.DoubleBuffer | System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer | System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
        }
        private object CellValueToObject(string cellVal)
        {
            if (ConvertUtil.IsNumericString(cellVal))
            {
                return double.Parse(cellVal, CultureInfo.InvariantCulture);
            }
            return cellVal;
        }
        private void BindPackageToUI()
        {
            var dataGrid1 = this;
            try
            {
                if (excel.Workbook.Worksheets[1].Dimension == null) return;
                for (var row = 1; row < excel.Workbook.Worksheets[1].Dimension.Rows + 1; row++)
                {
                    for (var col = 1; col <= this.Columns.Count; col++)
                    {
                        var excelCell = excel.Workbook.Worksheets.First().Cells[row, col];
                        var gridViewCell = dataGrid1.Rows[row - 1].Cells[col - 1];
                        gridViewCell.Value = excelCell.Value;
                    }
                }
            }
            catch (Exception ee)
            {
                if (OnException != null)
                {
                    OnException(this, new OnExceptionOccurredEventArgs() { Exception = ee });
                }
            }
            dataGrid1.Refresh();
        }
        protected override void OnCellBeginEdit(DataGridViewCellCancelEventArgs e)
        {
            try
            {
                var dataGrid1 = this;
                var cell = dataGrid1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                var excelCell = excel.Workbook.Worksheets.First().Cells[e.RowIndex + 1, e.ColumnIndex + 1];
                if (!string.IsNullOrEmpty(excelCell.Formula))
                {
                    cell.Value = "=" + excelCell.Formula;
                }
                dataGrid1.Refresh();
            }
            catch (Exception ee)
            {
                if (OnException != null)
                {
                    OnException(this, new OnExceptionOccurredEventArgs() { Exception = ee });
                }
            }
            base.OnCellBeginEdit(e);
        }
        protected override void OnCellEnter(DataGridViewCellEventArgs e)
        {
            try
            {
                var dataGrid1 = this;
                dataGrid1.Refresh();
                BindPackageToUI();
                var cell = dataGrid1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                var excelCell = excel.Workbook.Worksheets.First().Cells[e.RowIndex + 1, e.ColumnIndex + 1];
                if (!string.IsNullOrEmpty(excelCell.Formula))
                {
                    cell.Value = "=" + excelCell.Formula;

                }
                cell.Style.ForeColor = ColorEditingFore;
                cell.Style.BackColor = ColorEditingBack;
            }
            catch (Exception ee)
            {
                if (OnException != null)
                {
                    OnException(this, new OnExceptionOccurredEventArgs() { Exception = ee });
                }
            }
            base.OnCellEnter(e);
        }
        protected override void OnCellLeave(DataGridViewCellEventArgs e)
        {
            try
            {
                var dataGrid1 = this;
                var gridViewCell = dataGrid1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                var excelCell = excel.Workbook.Worksheets.First().Cells[e.RowIndex + 1, e.ColumnIndex + 1];
                gridViewCell.Style.ForeColor = ColorNormalFore;
                gridViewCell.Style.BackColor = ColorNormalBack;
            }
            catch (Exception ee)
            {
                if (OnException != null)
                {
                    OnException(this, new OnExceptionOccurredEventArgs() { Exception = ee });
                }
            }
            base.OnCellLeave(e);
        }
        protected override void OnCellValidating(DataGridViewCellValidatingEventArgs e)
        {
            try
            {
                var f = e.FormattedValue.ToString();
                if (f.StartsWith("="))
                {
                    excel.Workbook.Worksheets.First().Cells[e.RowIndex + 1, e.ColumnIndex + 1].Formula = f.Substring(1);
                }
                else
                {
                    excel.Workbook.Worksheets.First().Cells[e.RowIndex + 1, e.ColumnIndex + 1].Formula = null;
                    excel.Workbook.Worksheets.First().Cells[e.RowIndex + 1, e.ColumnIndex + 1].Value = CellValueToObject(f);
                }
                try
                {
                    excel.Workbook.Calculate();
                }
                catch (Exception ex)
                {
                    if (OnException != null)
                    {
                        OnException(this, new OnExceptionOccurredEventArgs() { Exception = ex });
                    }
                }
            }
            catch (Exception ee)
            {
                if (OnException != null)
                {
                    OnException(this, new OnExceptionOccurredEventArgs() { Exception = ee });
                }
            }
            BindPackageToUI();
            this.Refresh();
            base.OnCellValidating(e);
        }

        public class SheetCellValueChangeEventArgs : EventArgs
        {
            public int Row;
            public int Column;
            public object OriginalValue;
            public object Value;
            public bool Cancelled;
        }
        public event EventHandler<SheetCellValueChangeEventArgs> BeforeCellUpdate;
        public void SetCellValue(String addr, object obj)
        {
            try
            {
                ExcelCellAddress cellAddr = new ExcelCellAddress(addr);
                SetCellValue(cellAddr.Row, cellAddr.Column, obj);
            }
            catch (Exception ee)
            {
                if (OnException != null)
                {
                    OnException(this, new OnExceptionOccurredEventArgs() { Exception = ee });
                }
            }
        }
        public object GetCellValue(int row, int col)
        {
            try
            {
                return Rows[row].Cells[col].Value;
            }
            catch (Exception ee)
            {
                return null;
            }
        }
        public object GetCellValue(String addr)
        {
            try
            {
                ExcelCellAddress cellAddr = new ExcelCellAddress(addr);
                return GetCellValue(cellAddr.Row - 1, cellAddr.Column - 1);
            }
            catch (Exception ee)
            {
                if (OnException != null)
                {
                    OnException(this, new OnExceptionOccurredEventArgs() { Exception = ee });
                }
                return null;
            }
        }
        public void SetCellValue(int row, int col, object obj)
        {
            try
            {
                if (BeforeCellUpdate != null)
                {
                    SheetCellValueChangeEventArgs args = new SheetCellValueChangeEventArgs();
                    args.Row = row;
                    args.Column = col;
                    args.OriginalValue = Rows[row].Cells[col].Value;
                    args.Value = obj;
                    BeforeCellUpdate(this, args);
                    if (args.Cancelled)
                    {
                        return;
                    }
                }
            }
            catch (Exception ex) { ;}
            try
            {
                this.Rows[row - 1].Cells[col - 1].Value = obj;
                {
                    var f = obj.ToString();
                    if (f.StartsWith("="))
                    {
                        excel.Workbook.Worksheets.First().Cells[row, col].Formula = f.Substring(1);
                    }
                    else
                    {
                        excel.Workbook.Worksheets.First().Cells[row, col].Formula = null;
                        excel.Workbook.Worksheets.First().Cells[row, col].Value = CellValueToObject(f);
                    }
                    try
                    {
                        var excelCell = excel.Workbook.Worksheets.First().Cells[row, col];
                        excel.Workbook.Calculate();
                        Rows[row - 1].Cells[col - 1].Value = excelCell.Value;
                    }
                    catch (Exception ex)
                    {
                        if (OnException != null)
                        {
                            OnException(this, new OnExceptionOccurredEventArgs() { Exception = ex });
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                if (OnException != null)
                {
                    OnException(this, new OnExceptionOccurredEventArgs() { Exception = ee });
                }
            }
        }
    }
    internal static class ConvertUtil
    {
        internal static bool IsNumeric(object candidate)
        {
            if (candidate == null) return false;
            return (candidate.GetType().IsPrimitive || candidate is double || candidate is decimal || candidate is DateTime || candidate is TimeSpan || candidate is long);
        }

        internal static bool IsNumericString(object candidate)
        {
            if (candidate != null)
            {
                return Regex.IsMatch(candidate.ToString(), @"^[\d]+(\,[\d])?");
            }
            return false;
        }

        /// <summary>
        /// Convert an object value to a double 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="ignoreBool"></param>
        /// <returns></returns>
        internal static double GetValueDouble(object v, bool ignoreBool = false)
        {
            double d;
            try
            {
                if (ignoreBool && v is bool)
                {
                    return 0;
                }
                if (IsNumeric(v))
                {
                    if (v is DateTime)
                    {
                        d = ((DateTime)v).ToOADate();
                    }
                    else if (v is TimeSpan)
                    {
                        d = DateTime.FromOADate(0).Add((TimeSpan)v).ToOADate();
                    }
                    else
                    {
                        d = Convert.ToDouble(v, CultureInfo.InvariantCulture);
                    }
                }
                else
                {
                    d = 0;
                }
            }

            catch
            {
                d = 0;
            }
            return d;
        }
    }
}
