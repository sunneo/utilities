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
using Utilities.Coroutine.Waiter.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Coroutine.Waiter.Server
{
    public class WaiterHolder<T> where T:IWaiter
    {
        object WaiterLocker = new object();
        public LinkedList<T> WaiterList = new LinkedList<T>();
        public void AddWaiter(T w)
        {
            lock (WaiterLocker)
            {
                WaiterList.AddLast(w);
            }
        }
        public void ClearWaiter()
        {
            lock (WaiterLocker)
            {
                WaiterList.Clear();
            }
        }
        public T RemoveWaiter(T given = null, object givenType = null)
        {
            T w = null;
            lock (WaiterLocker)
            {
                if (WaiterList.Count > 0)
                {
                    if (given == null)
                    {
                        if (givenType == null)
                        {
                            w = WaiterList.First.Value;
                            WaiterList.RemoveFirst();
                        }
                        else
                        {
                            for (var node = WaiterList.First; node != null; node = node.Next)
                            {
                                if (node.Value.CanRemove(givenType))
                                {
                                    w = WaiterList.First.Value;
                                    WaiterList.Remove(node);
                                    break;
                                }
                            }
                            if (w == null)
                            {
                                w = WaiterList.First.Value;
                                WaiterList.RemoveFirst();
                            }
                        }
                    }
                    else
                    {
                        WaiterList.Remove(given);
                        w = given;
                    }
                }
            }
            return w;
        }
        public T NotifyAndRemove(object result, int _type = 0)
        {
            T waiter = RemoveWaiter(null, _type);
            if(waiter!=null) waiter.Notify(result);
            return waiter;
        }
        public T NotifyAndRemove(object result, object dummy)
        {
            T waiter = RemoveWaiter(null, dummy);
            if (waiter != null) waiter.Notify(result);
            return waiter;
        }
    }
    public class WaiterHolder:WaiterHolder<IWaiter>
    {

    }
}
