using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Utilities.Coroutine;

namespace Utilities
{
    public class ProcessMonitor
    {
        public ProcessStatusMeasure ProcessMeasure = new ProcessStatusMeasure();
        private Process process;
        private Assembly assembly;
        private readonly int pollingFrequency;
        private readonly int mProbeTimes;
        private readonly int stackTraceIterations;
        private Thread mMainThread;
        public volatile bool IsEnd = false;
        public ProcessMonitor(Thread mainThread, Process process, Assembly assembly, int pollingFrequency = 1000, int probeTimes = 4, int stackTraceIterations = 10)
        {
            this.mMainThread = mainThread;
            this.process = process;
            this.assembly = assembly;
            this.pollingFrequency = pollingFrequency;
            this.mProbeTimes = probeTimes;
            this.stackTraceIterations = stackTraceIterations;
        }
        public void Stop()
        {
            IsEnd = true;
        }
        private void TracerBody()
        {
            IsEnd = false;
            while (!IsEnd)
            {
                try
                {
                    
                    for (int i = 0; i < mProbeTimes; ++i)
                    {
                        ProcessMeasure.Step();
                        Thread.Sleep(pollingFrequency);
                    }
                    process = Process.GetCurrentProcess();
                    if (process.HasExited)
                    {
                        process.Dispose();
                        process = null;
                        return;
                    }
                    if (process.Responding)
                    {
                        process.Dispose();
                        continue;
                    }

                    var pid = process.Id;
                    {
                        StringBuilder mainThreadTrace = new StringBuilder();
                        var trace = GetStackTrace(mMainThread);
                        if (trace == null)
                        {
                            trace = GetStackTrace(mMainThread);
                        }
                        if (trace != null)
                        {
                            mainThreadTrace.AppendLine("--------------------------------------");
                            mainThreadTrace.AppendLine("MainThread");
                            foreach (var f in trace.GetFrames())
                            {
                                if (IsEnd) return;
                                var method = f.GetMethod();
                                String declType = "";
                                if (method.DeclaringType != null)
                                {
                                    declType = method.DeclaringType.FullName;
                                }
                                mainThreadTrace.AppendLine("  " + declType + "." + method.Name + "(" + f.GetFileName() + ":" + f.GetFileLineNumber() + ")");
                            }
                            mainThreadTrace.AppendLine();
                            Tracer.D(mainThreadTrace.ToString());
                        }

                    }
                    using (var dataTarget = DataTarget.AttachToProcess(pid, 5000, AttachFlag.Passive))
                    {
                        ClrInfo runtimeInfo = dataTarget.ClrVersions[0];
                        var runtime = runtimeInfo.CreateRuntime();

                        StringBuilder strb = new StringBuilder();
                        Dictionary<String, String> moduleMap = new Dictionary<string, string>();
                        foreach (var t in runtime.Modules)
                        {
                            if (IsEnd) return;
                            if (t.IsFile)
                            {
                                String name = Path.GetFileNameWithoutExtension(t.FileName);
                                moduleMap[name] = t.FileName;
                            }
                        }

                        foreach (var t in runtime.Threads)
                        {
                            if (IsEnd) return;
                            if (t.ManagedThreadId == Thread.CurrentThread.ManagedThreadId) continue;
                            if (!t.IsAlive) continue;

                            strb.AppendLine("--------------------------------------");
                            strb.AppendLine("Thread " + t.ManagedThreadId);
                            int count = 0;
                            foreach (var f in t.EnumerateStackTrace())
                            {
                                if (IsEnd) return;
                                strb.AppendLine(" " + f.DisplayString + "+" + f.InstructionPointer);
                                if (f.Method != null)
                                {
                                    String moduleName = f.ModuleName;

                                    Module module = assembly.GetModule(f.ModuleName);
                                    String pdbName = "";
                                    if (module != null)
                                    {
                                        pdbName = Path.Combine(Path.GetDirectoryName(module.Assembly.Location), Path.GetFileNameWithoutExtension(module.Assembly.Location)) + ".pdb";
                                    }
                                    else
                                    {
                                        if (moduleMap.ContainsKey(moduleName))
                                        {
                                            pdbName = Path.Combine(Path.GetDirectoryName(moduleMap[moduleName]), Path.GetFileNameWithoutExtension(moduleMap[moduleName])) + ".pdb";
                                        }
                                    }
                                    if (!string.IsNullOrEmpty(pdbName) && File.Exists(pdbName))
                                    {
                                        try
                                        {
                                            var sourceFileNameAndLine = ReadClrSourceFileNameAndLine(pdbName, f.Method, f.InstructionPointer);
                                            strb.AppendLine(String.Format("  >({0}- line {1})", sourceFileNameAndLine.Item1, sourceFileNameAndLine.Item2));
                                        }
                                        catch (Exception ee)
                                        {

                                        }
                                    }
                                }
                                ++count;
                                if (count >= stackTraceIterations)
                                {
                                    break;
                                }
                            }


                        }
                        strb.AppendLine();
                        Tracer.D(strb.ToString());
                        if (process != null)
                        {
                            process.Dispose();
                            process = null;
                        }
                    }

                }
                catch (Exception ee)
                {
                    Tracer.D(ee.ToString());
                }
            }
          
        }
        public void Run()
        {
            AsyncTask task = new AsyncTask(() =>
            {
                TracerBody();
            });
            task.Start(false);
        }
        /// <summary>
        /// Reads the CLR source file name, line number and displacement.
        /// </summary>
        /// <param name="pdbName">The name of pdb.</param>
        /// <param name="method">The method.</param>
        /// <param name="address">The address.</param>
        internal static Tuple<string, uint, ulong> ReadClrSourceFileNameAndLine(String pdbName, Microsoft.Diagnostics.Runtime.ClrMethod method, ulong address)
        {
            Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbReader pdbReader = new Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbReader(pdbName);
            Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbFunction function = pdbReader.GetFunctionFromToken(method.MetadataToken);
            uint ilOffset = FindIlOffset(method, address);

            ulong distance = ulong.MaxValue;
            string sourceFileName = "";
            uint sourceFileLine = uint.MaxValue;

            if (function != null && function.SequencePoints != null)
            {
                foreach (Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSequencePointCollection sequenceCollection in function.SequencePoints)
                {
                    foreach (Microsoft.Diagnostics.Runtime.Utilities.Pdb.PdbSequencePoint point in sequenceCollection.Lines)
                    {
                        if (point.Offset <= ilOffset)
                        {
                            ulong dist = ilOffset - point.Offset;

                            if (dist < distance)
                            {
                                sourceFileName = sequenceCollection.File.Name;
                                sourceFileLine = point.LineBegin;
                                distance = dist;
                            }
                        }
                    }
                }

                return Tuple.Create(sourceFileName, sourceFileLine, distance);
            }
            else
            {
                return Tuple.Create("", (uint)0, (ulong)0);
            }
        }
        /// <summary>
        /// Finds the IL offset for the specified frame.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="instructionPointer">The instruction pointer.</param>
        internal static uint FindIlOffset(Microsoft.Diagnostics.Runtime.ClrMethod method, ulong instructionPointer)
        {
            ulong ip = instructionPointer;
            uint last = uint.MaxValue;

            foreach (var item in method.ILOffsetMap)
            {
                if (item.StartAddress > ip)
                    return last;
                if (ip <= item.EndAddress)
                    return (uint)item.ILOffset;
                last = (uint)item.ILOffset;
            }

            return last;
        }

#pragma warning disable 0618
        private StackTrace GetStackTrace(Thread targetThread)
        {
            StackTrace stackTrace = null;
            var ready = new ManualResetEventSlim();

            new Thread(() =>
            {
                // Backstop to release thread in case of deadlock:
                ready.Set();
                Thread.Sleep(200);
                try { targetThread.Resume(); }
                catch { }
            }).Start();

            ready.Wait();
            targetThread.Suspend();
            try { stackTrace = new StackTrace(targetThread, true); }
            catch { /* Deadlock */ }
            finally
            {
                try { targetThread.Resume(); }
                catch { stackTrace = null;  /* Deadlock */  }
            }

            return stackTrace;
        }
#pragma warning restore 0618
    }
}
