using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities
{

    public class Locker
    {
        object lockerObj = new object();
        public volatile Thread OwnerThread;
        bool useLock = true;
        volatile bool Entered = false;
        private void PerformLock(int timeout = -1)
        {
            try
            {
                if (useLock)
                {
                    if (timeout == -1)
                    {
                        if (OwnerThread != null && !OwnerThread.IsAlive)
                        {
                            ForceDispose();
                        }
                        if (OwnerThread != null)
                        {
                            if (OwnerThread == Thread.CurrentThread)
                            {
                                return;
                            }
                        }
                        Monitor.Enter(lockerObj);
                        Entered = true;
                        OwnerThread = Thread.CurrentThread;
                    }
                    else
                    {
                        if (Monitor.TryEnter(lockerObj, timeout))
                        {
                            Entered = true;
                            OwnerThread = Thread.CurrentThread;
                        }
                        else
                        {
                        }
                    }
                }
            }
            catch (Exception ee)
            {

            }
        }
        private void PerformUnlock()
        {
            try
            {
                if (useLock)
                {
                    if (Entered)
                    {
                        OwnerThread = null;
                        Monitor.Exit(lockerObj);
                        Entered = false;
                    }
                }
            }
            catch (Exception ee)
            {

            }
        }

        public void Synchronized(Action action)
        {
            try
            {
                PerformLock(assignedTimeout);
                if (action != null)
                {
                    action();
                }
            }
            catch (ThreadAbortException ee)
            {
                PerformUnlock();
                ForceDispose();
            }
            finally
            {
                PerformUnlock();
            }
        }
        public T Synchronized<T>(Func<T> action)
        {
            try
            {
                PerformLock(assignedTimeout);
                return action();
            }
            catch (ThreadAbortException ee)
            {
                PerformUnlock();
                ForceDispose();
                return default(T);
            }
            finally
            {
                PerformUnlock();
            }
        }
        public Tout Synchronized<T1, Tout>(Func<T1, Tout> action, T1 param)
        {
            try
            {
                PerformLock(assignedTimeout);
                return action(param);
            }
            catch (ThreadAbortException ee)
            {
                PerformUnlock();
                ForceDispose();
                return default(Tout);
            }
            finally
            {
                PerformUnlock();
            }
        }
        public Tout Synchronized<T1, T2, Tout>(Func<T1, T2, Tout> action, T1 param, T2 param2)
        {
            try
            {
                PerformLock(assignedTimeout);
                return action(param, param2);
            }
            catch (ThreadAbortException ee)
            {
                PerformUnlock();
                ForceDispose();
                return default(Tout);
            }
            finally
            {
                PerformUnlock();
            }
        }
        public Tout Synchronized<T1, T2, T3, Tout>(Func<T1, T2, T3, Tout> action, T1 param, T2 param2, T3 param3)
        {
            try
            {
                PerformLock(assignedTimeout);
                return action(param, param2, param3);
            }
            catch (ThreadAbortException ee)
            {
                PerformUnlock();
                ForceDispose();
                return default(Tout);
            }
            finally
            {
                PerformUnlock();
            }
        }
        public Tout Synchronized<T1, T2, T3, T4, Tout>(Func<T1, T2, T3, T4, Tout> action, T1 param, T2 param2, T3 param3, T4 param4)
        {
            try
            {
                PerformLock(assignedTimeout);
                return action(param, param2, param3, param4);
            }
            catch (ThreadAbortException ee)
            {
                PerformUnlock();
                ForceDispose();
                return default(Tout);
            }
            finally
            {
                PerformUnlock();
            }
        }
        int assignedTimeout = -1;
        public Locker(bool useLock = true, int timeout = -1)
        {
            this.useLock = useLock;
            this.assignedTimeout = timeout;
        }
        public void ForceDispose()
        {
            try
            {
                Monitor.PulseAll(lockerObj);
            }
            catch (Exception ee)
            {

            }

            try
            {
                Monitor.Exit(lockerObj);

            }
            catch (Exception ee)
            {

            }
            lockerObj = new object();


        }
    }
}
