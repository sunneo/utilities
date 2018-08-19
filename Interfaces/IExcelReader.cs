using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.Interfaces
{
    public interface IExcelReader
    {
        bool IsValid { get;  }
        String this[int row, int col] { get; }
        String GetCellText(int row, int col);
        String CurrentSheetName { get;  }
        void SelectSheetByName(String name);
        void SelectSheetByIndex(int index);
        int SheetCount { get; }
    }
}
