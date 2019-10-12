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
        public bool CancellationPending
        {
            get;
            private set;
        }
        public void Cancel()
        {
            CancellationPending = true;
        }
    }
}
