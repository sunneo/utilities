using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace Utilities.Coroutine
{
    public class CoroutineStatus
    {
        public enum Status
        {
            // start when job coming
            READY,
            // already run
            RUNNING,
            // require start to run
            STOP
        }
    }
    public interface ITimer : IDisposable
    {
        int Interval { get; set; }
        bool Enabled { get; set; }
        bool IsDisposed { get; }
        event EventHandler Tick;

    }
    public class ThreadTimer : ITimer
    {
        int mInterval;
        bool mEnabled = false;
        bool mIsDisposed = false;

        event EventHandler mTick;
        
        public System.Threading.Timer Timer;

        public int Interval
        {
            get
            {
                return mInterval;
            }
            set
            {
                mInterval = value;

                if (this.IsDisposed || this.Timer == null)
                    return;

                try
                {
                    if (!this.Enabled)
                        this.Timer.Change(System.Threading.Timeout.Infinite, value);
                    else
                        this.Timer.Change(0, value);
                }
                catch
                {
                }
            }
        }
        public bool Enabled
        {
            get
            {
                return mEnabled;
            }
            set
            {
                mEnabled = value;

                if (this.IsDisposed || this.Timer == null)
                    return;

                try
                {
                    if (!this.mEnabled)
                        this.Timer.Change(System.Threading.Timeout.Infinite, this.Interval);
                    else
                        this.Timer.Change(this.Interval, this.Interval);
                }
                catch
                {
                }
            }
        }
        public bool IsDisposed
        {
            get
            {
                return this.mIsDisposed;
            }
        }

        public event EventHandler Tick
        {
            add
            {
                // To avoid dock event twice
                if (this.mTick == null || !this.mTick.GetInvocationList().Contains(value))
                    this.mTick += value;
            }
            remove
            {
                if (this.mTick != null)
                    this.mTick -= value;
            }
        }

        public ThreadTimer(int interval = 50)
        {
            this.mInterval = interval;
            this.Timer = new System.Threading.Timer(
                new System.Threading.TimerCallback(this.CallBack),
                this,
                System.Threading.Timeout.Infinite, interval
                );
        }

        private void CallBack(object state)
        {
            if (!this.Enabled || this.IsDisposed) 
                return;

            this.mEnabled = false;

            this.NotifyTickEvent();
            
            this.mEnabled = true;
        }
        private void NotifyTickEvent()
        {
            if (this.IsDisposed || this.mTick == null)
                return;

            this.mTick(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (this.IsDisposed)
                return;

            this.Timer.Dispose();
        }
    }

    public class CoroutineHost : IDisposable
    {
        static CoroutineHost mInstance;
        public static void DisposeDefaultInstance()
        {
            try
            {
                if (mInstance != null)
                {
                    mInstance.Dispose();
                }
                mInstance = null;
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
        }
        public static CoroutineHost Instance
        {
            get
            {
                if (CoroutineHost.mInstance == null)
                    CoroutineHost.mInstance = new CoroutineHost(new WindowFormTimer());

                return CoroutineHost.mInstance;
            }
        }

        ITimer Timer;
        IEnumerator enumerator;
        LinkedList<Coroutine> Queue = new LinkedList<Coroutine>();
        
        bool CanDisposeTimer = false;
        bool IsDisposed = false;

        public CoroutineHost(int interval)
        {
            this.CanDisposeTimer = true;
            this.Timer = new ThreadTimer(interval);
            this.Timer.Tick += this.Timer_Tick;
        }
        CoroutineHost(ITimer _Timer)
        {
            this.Timer = _Timer;
            this.Timer.Interval = 50;
            this.Timer.Tick += this.Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (this.IsDisposed)
                return;

            this.Timer.Enabled = false;
            
            try
            {
                // Consume queue
                if (this.enumerator == null)
                    this.enumerator = this.RunTasks();

                // Pointer to next task, it will be start at next timer tick
                if (!this.enumerator.MoveNext())
                    this.enumerator = null;

                // If all tasks have done, stop the timer
                if (this.Queue.Count == 0)
                {
                    this.Timer.Enabled = false;
                    return;
                }
            }
            catch (Exception ee)
            {
                Tracer.D(ee.ToString());
            }

            this.Timer.Enabled = true;
        }

        private IEnumerator RunTasks()
        {
            if (this.Queue.Count == 0)
                yield break;

            LinkedListNode<Coroutine> node = this.Queue.First;
            while (node != null)
            {
                DateTime now = DateTime.Now;

                LinkedListNode<Coroutine> next = node.Next;

                bool launched = false;
                Coroutine current = node.Value;
                if (!current.IsDisposed)
                {
                    // Every time when launch coroutine queue, its interval must bigger the coroutine interval
                    bool isInsideInterval = current.LastTriggerTime.ToBinary() == 0 ?
                        true :
                        now.Subtract(current.LastTriggerTime).TotalMilliseconds >= current.Interval;

                    if (isInsideInterval)
                    {
                        current.LastTriggerTime = now;

                        bool nodeCanContinue = true;
                        try
                        {
                            nodeCanContinue = node.Value.RunTask();
                        }
                        catch (Exception ee)
                        {
                            nodeCanContinue = false;
                            Tracer.D(ee.ToString());
                        }

                        if(!nodeCanContinue)
                        {
                            try
                            {
                                // Remove disposed one
                                this.Queue.Remove(node);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    else
                    {
                        launched = true;
                    }
                }
                else
                {
                    try
                    {
                        // Remove disposed one
                        this.Queue.Remove(node);
                    }
                    catch (Exception)
                    {
                    }
                }

                node = next;
            }
        }

        public void QueueWorkingItem(Coroutine runner)
        {
            if (this.IsDisposed)
                return;

            bool enabled = this.Timer.Enabled;

            // Force stop the timer, and append task to the last
            this.Timer.Enabled = false;

            this.Queue.AddLast(runner);

            // Resume timer state
            this.Timer.Enabled = enabled;
        }
        public void Start()
        {
            if (this.IsDisposed)
                return;

            this.Timer.Enabled = true;
        }
        public void Stop()
        {
            if (this.IsDisposed)
                return;

            this.Timer.Enabled = false;
        }

        public void Dispose()
        {
            if (this.IsDisposed)
                return;

            this.IsDisposed = true;

            try
            {
                this.Queue.Clear();

                this.Timer.Enabled = false;
                if (this.CanDisposeTimer)
                    this.Timer.Dispose();
            }
            catch (Exception ee)
            {
                Tracer.D(ee.ToString());
            }
        }

        internal class WindowFormTimer : ITimer
        {
            System.Windows.Forms.Timer mTimer;

            bool mDisposed = false;

            public event EventHandler Tick;

            public int Interval
            {
                get
                {
                    return this.IsDisposed ? 0 : this.mTimer.Interval;
                }
                set
                {
                    if (this.IsDisposed)
                        return;

                    this.mTimer.Interval = value;
                }
            }
            public bool Enabled
            {
                get
                {
                    return this.IsDisposed ? false : this.mTimer.Enabled;
                }
                set
                {
                    if (this.IsDisposed)
                        return;

                    this.mTimer.Enabled = value;

                    if (value)
                        this.mTimer.Start();
                    else
                        this.mTimer.Stop();
                }
            }
            public bool IsDisposed
            {
                get
                {
                    return this.mDisposed;
                }
            }

            public WindowFormTimer()
            {
                this.mTimer = new System.Windows.Forms.Timer();
                this.mTimer.Interval = 50;
                this.mTimer.Tick += this.mTimer_Tick;
                this.mTimer.Disposed += this.mTimer_Disposed;
            }

            private void mTimer_Tick(object sender, EventArgs e)
            {
                if (this.IsDisposed)
                    return;

                if (this.Tick != null)
                    this.Tick(sender, e);
            }
            private void mTimer_Disposed(object sender, EventArgs e)
            {
                this.mDisposed = true;
            }

            public void Dispose()
            {
                if (this.IsDisposed)
                    return;

                this.mDisposed = true;

                this.mTimer.Dispose();
            }
        }
    }

    public class Coroutine : IDisposable
    {
        CoroutineHost mHost;
        CoroutineStatus.Status mStatus = CoroutineStatus.Status.STOP;

        IEnumerator enumerator;

        internal DateTime LastTriggerTime;
        internal LinkedList<Func<bool>> Queue = new LinkedList<Func<bool>>();

        public int Interval = 1;
        public bool Enabled = false;
        public bool IsDisposed = false;

        public Coroutine(int interval = 50, CoroutineHost host = null)
        {
            this.Interval = interval;

            this.mHost = (host == null) ? CoroutineHost.Instance : host;
        }

        internal IEnumerator TaskRunner()
        {
            while(this.Queue.Count > 0)
            {
                LinkedListNode<Func<bool>> node = this.Queue.First;
                while (node != null)
                {
                    LinkedListNode<Func<bool>> next = node.Next;

                    bool nodeCanContinue = true;
                    try
                    {
                        nodeCanContinue = node.Value();
                    }
                    catch (Exception ee)
                    {
                        nodeCanContinue = false;
                        Tracer.D(ee.ToString());
                    }

                    try
                    {
                        if (!nodeCanContinue)
                            this.Queue.Remove(node);
                    }
                    catch (Exception)
                    {
                    }

                    node = next;
                    yield return true;
                }
            }

            if (this.Queue.Count == 0)
                this.mStatus = CoroutineStatus.Status.READY;

            yield break;
        }

        public void QueueWorkingItem(IEnumerator runner)
        {
            if (this.IsDisposed)
                return;

            this.mHost.Stop();

            this.Enabled = false;
            
            this.Queue.AddLast(new Func<bool>(runner.MoveNext));

            if (this.mStatus != CoroutineStatus.Status.RUNNING)
            {
                this.mStatus = CoroutineStatus.Status.RUNNING;
                this.mHost.QueueWorkingItem(this);
            }

            this.Enabled = true;
            
            if (this.mHost != null)
                this.mHost.Start();
        }

        public void Start()
        {
            if (this.IsDisposed)
                return;

            this.mStatus = CoroutineStatus.Status.RUNNING;
            this.Enabled = true;

            if (this.mHost != null)
            {
                this.mHost.QueueWorkingItem(this);
                this.mHost.Start();
            }
        }
        public void Stop()
        {
            if (this.IsDisposed)
                return;

            this.mStatus = CoroutineStatus.Status.STOP;
            this.Enabled = false;
            this.Queue.Clear();
        }
        public void Pause()
        {
            if (this.IsDisposed)
                return;

            this.mStatus = CoroutineStatus.Status.STOP;
            this.Enabled = false;
        }
        public void Resume()
        {
            if (this.IsDisposed)
                return;

            this.mStatus = CoroutineStatus.Status.RUNNING;
            this.Enabled = true;
        }

        public bool RunTask()
        {
            if (this.IsDisposed)
                return false;
            
            if (this.mStatus == CoroutineStatus.Status.RUNNING)
            {
                // Consume queue
                if (this.enumerator == null)
                    this.enumerator = this.TaskRunner();

                // If there has not next task in queue, return false
                if (!this.enumerator.MoveNext())
                {
                    this.enumerator = null;
                    this.Enabled = false;
                    return false;
                }

                // Still has task
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            if (this.IsDisposed)
                return;

            this.IsDisposed = true;
            this.Enabled = false;

            try
            {
                this.mStatus = CoroutineStatus.Status.STOP;
                this.Queue.Clear();
            }
            catch (Exception ee)
            {
                Tracer.D(ee.ToString());
            }
        }
    }
}
