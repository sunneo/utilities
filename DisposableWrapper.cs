using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class DisposableWrapper : IDisposable
    {
        private event EventHandler Disposed;
        public volatile bool IsDisposed = false;
        public void Dispose()
        {
            if (!IsDisposed)
            {
                try
                {
                    if (Disposed != null)
                    {
                        Disposed(this, EventArgs.Empty);
                        // Clear event handlers to prevent memory leaks
                        Disposed = null;
                    }
                }
                catch (Exception ee)
                {

                }
                IsDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
        public DisposableWrapper(Action action)
        {
            if (action != null)
            {
                Disposed += new EventHandler((sender, args) =>
                {
                    action();
                });
            }
        }
        ~DisposableWrapper()
        {
            Dispose();
        }
    }
}
