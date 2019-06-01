using Utilities.Waiter.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Waiter.Server
{
    public class WaiterHolder
    {
        object WaiterLocker = new object();
        public LinkedList<IWaiter> WaiterList = new LinkedList<IWaiter>();
        public void AddWaiter(IWaiter w)
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
        public IWaiter RemoveWaiter(IWaiter given = null, int givenType = 0)
        {
            IWaiter w = null;
            lock (WaiterLocker)
            {
                if (WaiterList.Count > 0)
                {
                    if (given == null)
                    {
                        if (givenType == 0)
                        {
                            w = WaiterList.First.Value;
                            WaiterList.RemoveFirst();
                        }
                        else
                        {
                            for (var node = WaiterList.First; node != null; node = node.Next)
                            {
                                if (node.Value.WaitType == givenType)
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
        public IWaiter NotifyAndRemove(object result, int _type=0)
        {
            IWaiter waiter = RemoveWaiter(null, _type);
            if (waiter != null)
            {
                waiter.DummyResult = result;
                waiter.HasReply = true;
            }
            return waiter;
        }
    }
}
