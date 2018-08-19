using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities.Interfaces;

namespace Utilities.Excel
{
    public class ExcelReaderFactory
    {
        public IExcelReader FromFile(String filename)
        {
            return ExcelImporter.FromFile(filename);
        }
    }
}
