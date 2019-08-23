using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTool.Interfaces
{
    public interface IProcessStatusMeasure
    {
        uint MemUsage
        {
            get
            ;
        }
        int CpuPercentage
        {
            get
           ;
        }
        int ThreadCount
        {
            get;
        }
        String MemoryUsageText
        {
            get
           ;
        }
        String CpuUsageText
        {
            get
            ;
        }
    }
}
