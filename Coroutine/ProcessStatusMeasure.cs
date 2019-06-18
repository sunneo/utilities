using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Interfaces;

namespace Utilities.Coroutine
{
    public class ProcessStatusMeasure : IProcessStatusMeasure
    {
        static ProcessStatusMeasure mInstance;
        public static ProcessStatusMeasure Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new ProcessStatusMeasure();
                }
                return mInstance;
            }
        }
        private PerformanceCounter cpuCounter;
        String mMemoryUsageText;
        String mCpuUsageText;
        private volatile uint mMemUsage;
        private volatile int mCpuPercentage;
        private volatile int mThreadCount;
        
        public int ThreadCount
        {
            get
            {
                return mThreadCount;
            }
        }
        public uint MemUsage
        {
            get
            {
                return mMemUsage;
            }
        }
        public int CpuPercentage
        {
            get
            {
                return mCpuPercentage;
            }
        }
        public String MemoryUsageText
        {
            get
            {
                return mMemoryUsageText;
            }
        }
        public String CpuUsageText
        {
            get
            {
                return mCpuUsageText;
            }
        }
        DateTime LastMeasureDateTime;
        public void Measure(Process process)
        {
            DateTime now = DateTime.Now;
            if (now.Subtract(LastMeasureDateTime).TotalSeconds < 1.0) return;
            mMemUsage = unchecked((uint)process.PrivateMemorySize);
            mThreadCount = process.Threads.Count;
            mMemoryUsageText = (Math.Round((double)mMemUsage / (1024.0 * 1024.0), 2)).ToString() + "MB";
            float cpuVal = cpuCounter.NextValue();
            mCpuPercentage = (int)cpuVal;
            mCpuUsageText = Math.Round(cpuVal, 2).ToString();
        }
        public void Step(Process process=null)
        {
            if (cpuCounter == null)
            {
                cpuCounter = new PerformanceCounter(
                  "Processor",
                  "% Processor Time",
                  "_Total",
                  true
               );
            }
            bool canDispose = false;
            if (process == null)
            {
                process = Process.GetCurrentProcess();
                canDispose = true;
            }
            if (process != null)
            {
                if (process.HasExited) return;
                Measure(process);
                if (canDispose)
                {
                    process.Dispose();
                }
            }
        }
        public ProcessStatusMeasure()
        {
           
        }
    }
}