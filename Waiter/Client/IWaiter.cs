using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.Waiter.Client
{
    public class IWaiter
    {
        public int WaitType = 0;
        private object Waiter = new Object();
        public volatile bool HasReply = false;
        public object DummyResult = null;
        public void Wait()
        {
            while (!HasReply)
            {
                Thread.Sleep(100);
                Thread.Yield();
                System.Windows.Forms.Application.DoEvents();
            }
        }
    }
    public class MessageWaiter<T> : IWaiter
    {
        public T Result;
    }
}
