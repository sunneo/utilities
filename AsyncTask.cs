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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities
{
    public class AsyncTask : IDisposable
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

        #region PRIVATE CLASSES

        private class AfterFinishJobArgs
        {
            public Control Ctrl;
            public object Action;
        }
        #endregion

        #region PRIVATE FIELDS

        private LinkedList<AfterFinishJobArgs> AfterFinishJob = new LinkedList<AfterFinishJobArgs>();
        private object bgthreadLocker = new object();
        private object AfterFinishJobLocker = new object();
        private volatile Thread bgthread;
        private bool fromJobConstructor = false;
        private object ownedJob = null;
        private volatile bool IsApartmentSet = false;
        private ApartmentState apartment = ApartmentState.MTA;
        private String mName = "";
        #endregion
        #region PUBLIC FIELDS

        public Dictionary<String, object> DynamicFields = new Dictionary<string, object>();

        #endregion

        #region PRIVATE METHODS
        private bool ControlInvoker(Control ctrl, object job)
        {
            if (job != null)
            {
                try
                {
                    if (job is Action)
                    {
                        Action actionObj = (Action)job;
                        if (ctrl != null)
                        {
                            ctrl.Invoke(actionObj);
                        }
                        else
                        {
                            actionObj();
                        }

                    }
                    else if (job is Action<AsyncTask>)
                    {
                        Action<AsyncTask> asyncJob = (Action<AsyncTask>)job;
                        if (ctrl != null)
                        {
                            ctrl.Invoke(asyncJob, this);
                        }
                        else
                        {
                            asyncJob(this);
                        }

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
            lock (AfterFinishJobLocker)
            {
                if (AfterFinishJob.Count > 0)
                {
                    job = AfterFinishJob.First.Value.Action;
                    ctrl = AfterFinishJob.First.Value.Ctrl;
                    AfterFinishJob.RemoveFirst();
                }
            }

            return ControlInvoker(ctrl, job);
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
                    }
                }
                else
                {
                    Run();
                }
                IsAvailable = true;
                while (PollAndRunJobAfterFinishJob()) ;
            }
            catch (Exception ee)
            {
                IsFault = true;
                FaultReason = ee;
                System.Diagnostics.Debug.WriteLine(ee.ToString());
            }
        }
        private void jobFlusher()
        {
            while (PollAndRunJobAfterFinishJob()) ;
            lock (AfterFinishJobLocker)
            {
                if (AfterFinishJob.Count > 0)
                {
                    AfterFinishJob.Clear();
                }
            }
        }
        private bool AsyncFlushJob()
        {
            lock (AfterFinishJobLocker)
            {
                if (IsAlive) return false; // do not rerun 
                bgthread = new Thread(() => { jobFlusher(); bgthread = null; });
                if (!String.IsNullOrEmpty(mName))
                {
                    bgthread.Name = mName;
                }
                else
                {
                    bgthread.Name = "AsyncThread";
                }
                if (IsApartmentSet)
                {
                    bgthread.SetApartmentState(this.apartment);
                }
                bgthread.IsBackground = true;
                bgthread.Start();
                return true;
            }
        }
        private bool StartAsync()
        {
            lock (AfterFinishJobLocker)
            {
                if (IsAlive) return false; // do not rerun 
                bgthread = new Thread(() =>
                {
                    runner();
                });
                if (!String.IsNullOrEmpty(mName))
                {
                    bgthread.Name = mName;
                }
                else
                {
                    bgthread.Name = "AsyncThread";
                }
                if (IsApartmentSet)
                {
                    bgthread.SetApartmentState(this.apartment);
                }
                bgthread.IsBackground = true;
                bgthread.Start();
                return true;
            }
        }
        #endregion

    

    
        
        

        #region Main Job to Override
        //=============================================================
        protected virtual void Run()
        {

        }
        //============================================================
        #endregion
        
        #region Constructors
        
        
        public AsyncTask()
        {

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
        public bool IsAlive
        {
            get
            {
                lock (bgthreadLocker)
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
        }
        #endregion

        #region public methods
        /// <summary>
        /// start running
        /// </summary>
        /// <param name="blocking">when blocking is true, caller would block until job finished</param>
        public void Start(bool blocking = true)
        {
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

        /// <summary>
        /// it async thread was running, terminate it.
        /// </summary>
        public void StopAsync()
        {
            if (IsAlive)
            {
                bgthread.Abort();
            }
            bgthread = null;
        }
        /// <summary>
        /// add additional job into running thread
        /// </summary>
        /// <param name="l">an action job which contains statements for running</param>
        public void AddAfterFinishJob(Action l)
        {
            lock (AfterFinishJobLocker)
            {
                AfterFinishJob.AddLast(new AfterFinishJobArgs() { Action = l });
            }
        }
        /// <summary>
        /// add additional job into running thread
        /// </summary>
        /// <param name="l">an action job which contains statements for running</param>
        public void AddAfterFinishJob(Control c, Action l)
        {
            lock (AfterFinishJobLocker)
            {
                AfterFinishJob.AddLast(new AfterFinishJobArgs() { Ctrl = c, Action = l });
            }
        }
        /// <summary>
        /// add additional job into running thread
        /// </summary>
        /// <param name="l">an action job which contains statements for running</param>
        public void AddAfterFinishJob(Action<AsyncTask> l)
        {
            lock (AfterFinishJobLocker)
            {
                AfterFinishJob.AddLast(new AfterFinishJobArgs() { Action = l });
            }
        }
        /// <summary>
        /// add additional job into running thread
        /// </summary>
        /// <param name="l">an action job which contains statements for running</param>
        public void AddAfterFinishJob(Control c, Action<AsyncTask> l)
        {
            lock (AfterFinishJobLocker)
            {
                AfterFinishJob.AddLast(new AfterFinishJobArgs() { Ctrl = c, Action = l });
            }
        }
        
        public void Dispose()
        {
            ownedJob = null;
            ClearJob();
            StopAsync();
            IsFault = false;
            mName = null;
            try
            {
                DynamicFields = new Dictionary<string, object>();
            }
            catch (Exception ee)
            {

            }
        }
        public void ClearJob()
        {
            lock (AfterFinishJobLocker)
            {
                AfterFinishJob.Clear();
            }
        }

        public void SetApartment(ApartmentState apartment)
        {
            this.apartment = apartment;
            IsApartmentSet = true;
        }
        public bool TimedWait(int millis)
        {
            if (IsAlive)
            {
                return bgthread.Join(millis);
            }
            return true;
        }
        public void Wait()
        {
            if (IsAlive)
            {
                bgthread.Join();
            }
        }
        public bool Join(int timeMills = -1)
        {
            if (!IsAlive) return true;
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
        #endregion
    }
}
