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
        public class AfterFinishJobArgs
        {
            public Control Ctrl;
            public object Action;
        }
        public Dictionary<String, object> DynamicFields = new Dictionary<string, object>();
        
        object AfterFinishJobLocker = new object();
        private LinkedList<AfterFinishJobArgs> AfterFinishJob = new LinkedList<AfterFinishJobArgs>();
        private object ownedJob = null;
        private bool fromJobConstructor = false;
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
            }
        }
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
            if (bgthread != null && bgthread.IsAlive)
            {
                bgthread.Abort();
                bgthread = null;
            }
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
        #endregion
        public void Dispose()
        {
            StopAsync();
            ClearJob();
            StopAsync();
            IsFault = false;
        }
        public void ClearJob()
        {
            lock (AfterFinishJobLocker)
            {
                AfterFinishJob.Clear();
            }
        }
    }
}
