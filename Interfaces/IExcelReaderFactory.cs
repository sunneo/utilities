using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.Interfaces
{
    public interface IExcelReaderFactory
    {
        IExcelReader FromFile(String filename);
    }
}
