using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Server
{
    public class OnHandleConnectionEventArgs : EventArgs
    {
        public ServerHolder Server;
        public MediaConnectionInstance Current;
        public OnHandleConnectionEventArgs(ServerHolder server, MediaConnectionInstance current)
        {
            this.Server = server;
            this.Current = current;
        }
    }
}
