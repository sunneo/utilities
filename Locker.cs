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

namespace Utilities
{

    public class Locker
    {
        object lockerObj = new object();
        object RawLocker = new object();
        public volatile Thread OwnerThread;
        bool useLock = true;
        volatile bool Entered = false;
        public DisposableWrapper Lock()
        {
            DisposableWrapper ret = new DisposableWrapper(() =>
            {
                Monitor.Exit(lockerObj);
            });
            Monitor.Enter(lockerObj);
            return ret;
        }
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
        public void SynchronizedWithEvent(Action action, Action beforeEnter, Action enter, Action leave)
        {
            try
            {
                if (beforeEnter != null)
                {
                    beforeEnter();
                }
                PerformLock();
                if (enter != null)
                {
                    enter();
                }
                if (action != null)
                {
                    action();
                }
            }
            finally
            {
                PerformUnlock();
                if (leave != null)
                {
                    leave();
                }
            }
        }

        public T SynchronizedWithEvent<T>(Func<T> action, Action beforeEnter, Action enter, Action leave)
        {
            T ret = default(T);
            bool locked = false;
            try
            {
                if (beforeEnter != null)
                {
                    beforeEnter();
                }
                PerformLock();
                if (enter != null)
                {
                    enter();
                }
                locked = true;
                ret = action();
                PerformUnlock();
                if (leave != null)
                {
                    leave();
                }
                locked = false;
                return ret;
            }
            finally
            {
                PerformUnlock();
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
