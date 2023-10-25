using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities
{
    public class ProcessChecker
    {
        public class ProcessCheckerOnExitArg : EventArgs
        {
            public int ProcessId { get; private set; }
            public ProcessCheckerOnExitArg(int id)
            {
                this.ProcessId = id;
            }
        }
        int processID;
        Thread thread;
        Boolean runFlag = false;
        public delegate void OnProcessExitedHandler(object sender, ProcessCheckerOnExitArg args);
        public event OnProcessExitedHandler OnProcessExited;
        volatile bool ByID = false;
        volatile String ProcName;
        public ProcessChecker(int id)
        {
            ByID = true;
            this.processID = id;
            this.thread = new Thread(runner);
            this.thread.IsBackground = true;
            this.thread.Name = "ProcessChecker";
        }
        public ProcessChecker(String name)
        {
            ByID = false;
            this.ProcName = name;
            this.thread = new Thread(runner);
            this.thread.IsBackground = true;
            this.thread.Name = "ProcessChecker";
        }
        private void PerformExit()
        {
            if (OnProcessExited == null)
            {
                Application.Exit();
            }
            else
            {
                OnProcessExited(this, new ProcessCheckerOnExitArg(this.processID));
            }
        }
        void runner()
        {
            while (runFlag)
            {
                System.Threading.Thread.Sleep(1500);//每1.5秒檢查一次               
                try
                {
                    if (ByID)
                    {
                        Process process = Process.GetProcessById(processID);
                        if (process == null || process.HasExited)
                        {
                            runFlag = false;
                            PerformExit();
                        }
                    }
                    else
                    {
                        Process[] loadScreenProcesses = Process.GetProcessesByName(ProcName);
                        if (loadScreenProcesses.Length == 0)
                        {
                            PerformExit();
                        }
                        else
                        {
                            if (loadScreenProcesses[0].HasExited)
                            {
                                PerformExit();
                            }
                        }
                    }
                }
                catch (Exception ee)
                {
                    Console.Error.WriteLine(ee.ToString());
                    runFlag = false;
                    if (OnProcessExited == null)
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        OnProcessExited(this, new ProcessCheckerOnExitArg(this.processID));
                    }
                }
            }
        }
        public void start()
        {
            runFlag = true;
            this.thread.Start();
        }
    }
}
