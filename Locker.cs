﻿using System;
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
        object RawLocker = new object();
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
            if (useLock)
            {
                lock (RawLocker)
                {
                    if (action != null)
                    {
                        action();
                    }
                }
            }
            else
            {
                if (action != null)
                {
                    action();
                }
            }
            
        }
        public T Synchronized<T>(Func<T> action)
        {
            if (useLock)
            {
                lock (RawLocker)
                {
                    return action();
                }
            }
            else
            {
                return action();
            }
            
        }
        public Tout Synchronized<T1, Tout>(Func<T1, Tout> action, T1 param)
        {
            if (useLock)
            {
                lock (RawLocker)
                {
                    return action(param);
                }
            }
            else
            {
                return action(param);
            }
            
        }
        public Tout Synchronized<T1, T2, Tout>(Func<T1, T2, Tout> action, T1 param, T2 param2)
        {
            if (useLock)
            {
                lock (RawLocker)
                {
                   return action(param, param2);
                }
            }
            else
            {
               return action(param, param2);
            }
            
        }
        public Tout Synchronized<T1, T2, T3, Tout>(Func<T1, T2, T3, Tout> action, T1 param, T2 param2, T3 param3)
        {
            if (useLock)
            {
                lock (RawLocker)
                {
                    return action(param, param2, param3);
                }
            }
            else
            {
                return action(param, param2, param3);
            }

          
        }
        public Tout Synchronized<T1, T2, T3, T4, Tout>(Func<T1, T2, T3, T4, Tout> action, T1 param, T2 param2, T3 param3, T4 param4)
        {
            if (useLock)
            {
                lock (RawLocker)
                {
                    return action(param, param2, param3, param4);
                }
            }
            else
            {
                return action(param, param2, param3, param4);
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
