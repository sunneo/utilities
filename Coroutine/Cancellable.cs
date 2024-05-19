using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Interfaces;

namespace Utilities.Coroutine
{
    public class Cancellable : ICancellable
    {
        private static long uid = -1;
        public long ID = uid++;
        volatile bool mCancellationPending;
        public bool CancellationPending
        {
            get
            {
                return mCancellationPending;
            }
            private set
            {
                mCancellationPending = value;
            }
        }
        public void Cancel()
        {
            CancellationPending = true;
        }
    }
}
