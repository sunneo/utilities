using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Utilities.IpcCli
{
    public class IpcCliFileServerExample : BaseIpcCliServer
    {
        FileCommunicator fileCommunicator;
        public IpcCliFileServerExample()
        {
            String dir = "IPCCLI";
            String indir = Path.Combine(dir, "IN");
            String outdir = Path.Combine(dir, "OUT");
            fileCommunicator = new FileCommunicator();
            fileCommunicator.SetOutputFolder(outdir);
            fileCommunicator.SetInputFolder(indir);
            fileCommunicator.OnTextInputedWithPath += (s, e) => OnMessage(e.Key);
        }

        public override void SendReply(String reply)
        {
            fileCommunicator.Write(reply);
        }

        public override void Start()
        {
            fileCommunicator.Start();
        }

        public override void Stop()
        {
            fileCommunicator.Stop();
        }

        public override void OnMessage(String msg)
        {
            base.OnMessage(msg);
        }

    }
}
