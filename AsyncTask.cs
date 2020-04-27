/*
* Copyright (c) 2019-2020 [Open Source Developer, Sunneo].
* All rights reserved.
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the [Open Source Developer, Sunneo] nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE [Open Source Developer, Sunneo] AND CONTRIBUTORS "AS IS" AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE [Open Source Developer, Sunneo] AND CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities
{
    public class AsyncTask
    {
        /// <summary>
        /// Test Program
        /// </summary>
        /// <param name="argv"></param>
        public static void Test(String[] argv)
        {
            AsyncTask goodjob = new AsyncTask(new Action(() => { Console.WriteLine("Main Job"); }));
            goodjob.AddAfterFinishJob(new Action(() => { Console.WriteLine("Additional Job"); }));
            goodjob.Start(false);
            goodjob.FlushJob();
            Console.WriteLine("Good Job-Available={0};IsFault={1}", goodjob.IsAvailable, goodjob.IsFault);
            AsyncTask badjob = new AsyncTask(new Action(() => { Console.WriteLine("Main Job2"); throw new Exception("No thing"); }));
            badjob.AddAfterFinishJob(new Action(() => { Console.WriteLine("Additional Job2"); }));
            badjob.Start(true);
            badjob.FlushJob();
            Console.WriteLine("Bad Job-Available={0};IsFault={1}", goodjob.IsAvailable, goodjob.IsFault);
        }
        public class AfterFinishJobArgs
        {
            public Control Ctrl;
            public object Action;
            public CallerInfoClazz callerInfo = new CallerInfoClazz();
        }
        public Dictionary<String, object> DynamicFields = new Dictionary<string, object>();
        public Dictionary<String, object> Args = new Dictionary<string, object>();
        object AfterFinishJobLocker = new object();
        private LinkedList<AfterFinishJobArgs> AfterFinishJob = new LinkedList<AfterFinishJobArgs>();
        private object ownedJob = null;
        private bool fromJobConstructor = false;
        public volatile bool DisposeAfterFinish = false;
        public class CallerInfoClazz
        {
            public String MemberName;
            public String SourceCode;
            public int Line;
        }
        public CallerInfoClazz CallerInfo = new CallerInfoClazz();
        public bool IsAlive
        {
            get
            {
                if (bgthread == null)
                {
                    return false;
                }
                try
                {
                    return bgthread.IsAlive;
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee.ToString());
                    return false;
                }
            }
        }
        private bool ControlInvoker(Control ctrl, object job, CallerInfoClazz info = null)
        {
            if (job != null)
            {
                try
                {
                    if (this.CallerInfo != null)
                    {
                        this.CallerInfo = info;
                    }
                    if (job is Action)
                    {
                        Action actionObj = (Action)job;
                        EventHandler Invoker = null;
                        Invoker = new EventHandler((object sender, EventArgs args) =>
                        {
                            if (sender is Control)
                            {
                                (sender as Control).HandleCreated -= Invoker;
                            }
                            ctrl.Invoke(actionObj);
                        });
                        if (ctrl != null)
                        {
                            if (ctrl.IsHandleCreated && !ctrl.IsDisposed)
                            {
                                ctrl.Invoke(actionObj);
                            }
                            else if (!ctrl.IsHandleCreated)
                            {
                                ctrl.HandleCreated += Invoker;
                            }
                        }
                        else
                            actionObj();
                    }
                    else if (job is Action<AsyncTask>)
                    {
                        Action<AsyncTask> asyncJob = (Action<AsyncTask>)job;
                        EventHandler Invoker = null;
                        Invoker = new EventHandler((object sender, EventArgs args) =>
                        {
                            if (sender is Control)
                            {
                                (sender as Control).HandleCreated -= Invoker;
                            }
                            ctrl.Invoke(asyncJob, this);
                        });
                        if (ctrl != null)
                        {
                            if (ctrl.IsHandleCreated && !ctrl.IsDisposed)
                            {
                                ctrl.Invoke(asyncJob, this);
                            }
                            else if (!ctrl.IsHandleCreated)
                            {
                                ctrl.HandleCreated += Invoker;
                            }
                        }
                        else
                            asyncJob(this);
                    }
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee.ToString());
                }
                return true;
            }
            return false;
        }

        private bool PollAndRunJobAfterFinishJob()
        {
            object job = null;
            Control ctrl = null;
            CallerInfoClazz callerInfo = null;
            lock (AfterFinishJobLocker)
            {
                if (AfterFinishJob.Count > 0)
                {
                    job = AfterFinishJob.First.Value.Action;
                    ctrl = AfterFinishJob.First.Value.Ctrl;
                    callerInfo = AfterFinishJob.First.Value.callerInfo;
                    AfterFinishJob.RemoveFirst();
                }
            }

            return ControlInvoker(ctrl, job, callerInfo);
        }
        private void runner()
        {
            try
            {
                IsFault = false;
                IsAvailable = false;
                if (fromJobConstructor)
                {
                    if (ownedJob != null)
                    {
                        if (ownedJob is Action)
                        {
                            Action actionObj = (Action)ownedJob;
                            actionObj();
                        }
                        else if (ownedJob is Action<AsyncTask>)
                        {
                            Action<AsyncTask> asyncJob = (Action<AsyncTask>)ownedJob;
                            asyncJob(this);
                        }
                        else if (ownedJob is Action<Dictionary<String, object>>)
                        {
                            Action<Dictionary<String, object>> asyncJob = (Action<Dictionary<String, object>>)ownedJob;
                            asyncJob(this.DynamicFields);
                        }
                        ownedJob = null;
                        fromJobConstructor = false;
                    }
                }
                else
                {
                    Run();
                }
                IsAvailable = true;
                while (PollAndRunJobAfterFinishJob()) ;
                if (DisposeAfterFinish)
                {
                    RunningDisposeAfterFinish = true;
                    this.Dispose();
                }
            }
            catch (Exception ee)
            {
                IsFault = true;
                FaultReason = ee;
            }
        }
        volatile bool RunningDisposeAfterFinish = false;
        private void jobFlusher()
        {
            while (PollAndRunJobAfterFinishJob()) ;
        }
        private bool AsyncFlushJob()
        {
            lock (AfterFinishJobLocker)
            {
                if (bgthread != null && bgthread.IsAlive) return false; // do not rerun 
                bgthread = new Thread(() => { jobFlusher(); bgthread = null; });
                if (mApartment != ApartmentState.Unknown)
                {
                    bgthread.SetApartmentState(mApartment);
                }
                if (!String.IsNullOrEmpty(mName))
                {
                    bgthread.Name = mName;
                }
                else
                {
                    bgthread.Name = "AsyncThread";
                }
                bgthread.IsBackground = true;
                bgthread.Start();
                return true;
            }
        }
        public bool TimedWait(int millis)
        {
            if (bgthread != null && bgthread.IsAlive)
            {
                return bgthread.Join(millis);
            }
            return true;
        }
        public void Wait()
        {
            if (bgthread != null && bgthread.IsAlive)
            {
                bgthread.Join();
            }
        }
        public bool Join(int timeMills = -1)
        {
            if (bgthread == null || !bgthread.IsAlive) return true;
            if (timeMills > 0)
            {
                return bgthread.Join(timeMills);
            }
            else
            {
                bgthread.Join();
                return true;
            }
        }
        private volatile Thread bgthread;
        private bool StartAsync()
        {
            lock (AfterFinishJobLocker)
            {
                if (bgthread != null && bgthread.IsAlive) return false; // do not rerun 
                bgthread = new Thread(() =>
                {
                    try
                    {
                        runner();
                    }
                    catch (ThreadAbortException aborted)
                    {
                        IsCancelled = true;
                        Thread.ResetAbort();
                    }
                });
                if (mApartment != ApartmentState.Unknown)
                {
                    bgthread.SetApartmentState(mApartment);
                }
                if (!String.IsNullOrEmpty(mName))
                {
                    bgthread.Name = mName;
                }
                else
                {
                    bgthread.Name = "AsyncThread";
                }
                bgthread.IsBackground = true;
                bgthread.Start();
                return true;
            }
        }

        #region Main Job to Override
        //=============================================================
        protected virtual void Run()
        {

        }
        //============================================================
        #endregion

        #region Constructors
        private String mName = "";
        public AsyncTask()
        {
        }
        System.Threading.ApartmentState mApartment = ApartmentState.Unknown;
        public void SetApartment(System.Threading.ApartmentState state)
        {
            mApartment = state;
        }
        public String GetName()
        {
            return mName;
        }
        public void SetName(String name)
        {
            mName = name;
        }
        public AsyncTask(Action j)
        {
            ownedJob = j;
            fromJobConstructor = true;
        }
        public AsyncTask(Action<AsyncTask> j)
        {
            ownedJob = j;
            fromJobConstructor = true;
        }
        public AsyncTask(Action<Dictionary<String, object>> j)
        {
            ownedJob = j;
            fromJobConstructor = true;
        }
        #endregion

        #region Public Attributes
        public Exception FaultReason = null;
        public volatile bool IsAvailable = false;
        public volatile bool IsFault = false;
        #endregion

        #region public methods
        /// <summary>
        /// start running
        /// </summary>
        /// <param name="blocking">when blocking is true, caller would block until job finished</param>
        public void Start(bool blocking = false)
        {
            IsCancelled = false;
            if (blocking)
            {
                runner();
            }
            else
            {
                StartAsync();
            }
        }
        /// <summary>
        /// wait until job and its additional job finish
        /// </summary>
        /// <param name="blocking">when blocking is true, caller would block until job finished</param>
        /// <returns></returns>
        public bool FlushJob(bool blocking = true)
        {
            if (blocking)
            {
                jobFlusher();
                return true;
            }
            else
            {
                return AsyncFlushJob();
            }
        }
        public volatile bool IsCancelled = false;
        public void Cancel()
        {
            IsCancelled = true;
        }
        /// <summary>
        /// it async thread was running, terminate it.
        /// </summary>
        public void StopAsync(bool avoidKillSelf = false)
        {
            if (bgthread != null && bgthread.IsAlive)
            {
                bool canKill = true;
                if (avoidKillSelf)
                {
                    canKill = (bgthread != Thread.CurrentThread);
                }
                if (canKill)
                {
                    try
                    {
                        if (bgthread != null) bgthread.Interrupt();
                        if (bgthread != null) bgthread.Abort();
                    }
                    finally
                    {
                        bgthread = null;
                    }
                }

            }
        }
        public int QueueLength()
        {
            lock (AfterFinishJobLocker)
            {
                return AfterFinishJob.Count;
            }
        }
        /// <summary>
        /// add additional job into running thread
        /// </summary>
        /// <param name="l">an action job which contains statements for running</param>
        public void AddAfterFinishJob(Action l,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            lock (AfterFinishJobLocker)
            {
                CallerInfoClazz info = new CallerInfoClazz() { Line = sourceLineNumber, MemberName = memberName, SourceCode = sourceFilePath };
                AfterFinishJob.AddLast(new AfterFinishJobArgs() { Action = l, callerInfo = info });
            }
        }
        /// <summary>
        /// add additional job into running thread
        /// </summary>
        /// <param name="l">an action job which contains statements for running</param>
        public void AddAfterFinishJob(Control c, Action l,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            lock (AfterFinishJobLocker)
            {
                CallerInfoClazz info = new CallerInfoClazz() { Line = sourceLineNumber, MemberName = memberName, SourceCode = sourceFilePath };
                AfterFinishJob.AddLast(new AfterFinishJobArgs() { Ctrl = c, Action = l, callerInfo = info });
            }
        }
        /// <summary>
        /// add additional job into running thread
        /// </summary>
        /// <param name="l">an action job which contains statements for running</param>
        public void AddAfterFinishJob(Action<AsyncTask> l,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            lock (AfterFinishJobLocker)
            {
                CallerInfoClazz info = new CallerInfoClazz() { Line = sourceLineNumber, MemberName = memberName, SourceCode = sourceFilePath };
                AfterFinishJob.AddLast(new AfterFinishJobArgs() { Action = l, callerInfo = info });
            }
        }
        /// <summary>
        /// add additional job into running thread
        /// </summary>
        /// <param name="l">an action job which contains statements for running</param>
        public void AddAfterFinishJob(Control c, Action<AsyncTask> l,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            lock (AfterFinishJobLocker)
            {
                CallerInfoClazz info = new CallerInfoClazz() { Line = sourceLineNumber, MemberName = memberName, SourceCode = sourceFilePath };
                AfterFinishJob.AddLast(new AfterFinishJobArgs() { Ctrl = c, Action = l, callerInfo = info });
            }
        }
        #endregion



        public void Dispose()
        {
            if (!RunningDisposeAfterFinish)
            {
                StopAsync();
            }
            ClearJob();
            IsFault = false;
            ownedJob = null;
            CallerInfo = null;
        }
        public void ClearJob()
        {
            lock (AfterFinishJobLocker)
            {
                AfterFinishJob.Clear();
            }
        }
        public class TaskContinuable
        {
            internal AsyncTask CurrentTask;
            private static void ControlInvoke(Control ctrl, Action<AsyncTask> callback, AsyncTask task)
            {
                if (ctrl != null)
                {
                    EventHandler Invoker = null;
                    Invoker = (object sender, EventArgs e) =>
                    {
                        ctrl.HandleCreated -= Invoker;
                        callback(task);
                    };
                    if (ctrl.IsHandleCreated)
                    {
                        ctrl.Invoke(new Action(() =>
                        {
                            callback(task);
                        }));
                    }
                    else
                    {
                        ctrl.HandleCreated += Invoker;
                    }
                }
            }
            private static void ControlInvoke(Control ctrl, Action callback)
            {
                if (ctrl != null)
                {
                    EventHandler Invoker = null;
                    Invoker = (object sender, EventArgs e) =>
                    {
                        ctrl.HandleCreated -= Invoker;
                        callback();
                    };
                    if (ctrl.IsHandleCreated)
                    {
                        ctrl.Invoke(new Action(() =>
                        {
                            callback();
                        }));
                    }
                    else
                    {
                        ctrl.HandleCreated += Invoker;
                    }
                }
            }
            public TaskContinuable Then(Action<AsyncTask> task,
              [CallerMemberName] string memberName = "",
              [CallerFilePath] string sourceFilePath = "",
              [CallerLineNumber] int sourceLineNumber = 0)
            {
                if (CurrentTask != null && !CurrentTask.IsDisposed)
                {
                    CurrentTask.AddAfterFinishJob(task, memberName, sourceFilePath, sourceLineNumber);
                    CurrentTask.FlushJob(false);
                }
                else
                {
                    task(CurrentTask);
                }
                return this;
            }
            public TaskContinuable Then(Action task,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
            {
                if (CurrentTask != null && !CurrentTask.IsDisposed)
                {
                    CurrentTask.AddAfterFinishJob(task, memberName, sourceFilePath, sourceLineNumber);
                    CurrentTask.FlushJob(false);
                }
                else
                {
                    task();
                }
                return this;
            }
            public TaskContinuable Then(Control ctrl, Action<AsyncTask> task,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
            {
                if (CurrentTask != null && !CurrentTask.IsDisposed)
                {
                    CurrentTask.AddAfterFinishJob(ctrl, task, memberName, sourceFilePath, sourceLineNumber);
                    CurrentTask.FlushJob(false);
                }
                else
                {
                    ControlInvoke(ctrl, task, CurrentTask);
                }
                return this;
            }
            public TaskContinuable Then(Control ctrl, Action task,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
            {
                if (CurrentTask != null && !CurrentTask.IsDisposed)
                {
                    CurrentTask.AddAfterFinishJob(ctrl, task, memberName, sourceFilePath, sourceLineNumber);
                    CurrentTask.FlushJob(false);
                }
                else
                {
                    ControlInvoke(ctrl, task);
                }
                return this;
            }
        }
        public static TaskContinuable QueueWorkingItem(Action action,
                [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            AsyncTask task = new AsyncTask(action);
            task.CallerInfo.MemberName = memberName;
            task.CallerInfo.SourceCode = sourceFilePath;
            task.CallerInfo.Line = sourceLineNumber;
            TaskContinuable continueItem = new TaskContinuable();
            task.DisposeAfterFinish = true;
            continueItem.CurrentTask = task;
            task.SetName("Async-QueueWorkingItem");
            task.Start(false);
            return continueItem;
        }
        volatile bool IsDisposed = false;
        ~AsyncTask()
        {
            if (!IsDisposed)
            {
                Dispose();
            }
        }
    }
}
