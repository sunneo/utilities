using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.Server
{
    public class NamedPipeServer
    {
        public String Name;
        private System.Threading.Thread Task;
        public NamedPipeServer(String name)
        {
            this.Name = name;
        }
        public event EventHandler<Tuple<StreamReader, StreamWriter>> Connected;
        public event EventHandler Started;
        public event EventHandler Stopped;
        public event EventHandler<Exception> ErrorOccurred;
        System.Threading.CancellationTokenSource CancellationTokenSource = new System.Threading.CancellationTokenSource();
        public bool IsAlive
        {
            get
            {
                return (Task != null && Task.IsAlive);
            }
        }
        NamedPipeServerStream NewServer()
        {
            PipeSecurity ps = new PipeSecurity();
            ps.AddAccessRule(new PipeAccessRule("Users", PipeAccessRights.ReadWrite | PipeAccessRights.CreateNewInstance, AccessControlType.Allow));
            ps.AddAccessRule(new PipeAccessRule("CREATOR OWNER", PipeAccessRights.FullControl, AccessControlType.Allow));
            ps.AddAccessRule(new PipeAccessRule("SYSTEM", PipeAccessRights.FullControl, AccessControlType.Allow));
            // Create the real pipe and monitor it indefinitely
            NamedPipeServerStream ret = new NamedPipeServerStream(Name, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.None,16384,16384, ps);
            return ret;
        }
        private void TaskLoop()
        {
            NamedPipeServerStream server = null;
            try
            {
                server = NewServer();
                if (Started != null)
                {
                    Started(this, EventArgs.Empty);
                }
                while (true)
                {
                    if (CancellationTokenSource.IsCancellationRequested)
                    {
                        server.Dispose();
                        server = null;
                        break;
                    }
                    server.WaitForConnection();

                    using (StreamReader reader = new StreamReader(server, Encoding.UTF8, true, 1024, true))
                    using (StreamWriter writer = new StreamWriter(server, Encoding.UTF8, 1024, true))
                    {
                        try
                        {
                            while (true)
                            {
                                if (Connected != null)
                                {
                                    Connected(this, new Tuple<StreamReader, StreamWriter>(reader, writer));
                                }
                                if (!server.IsConnected)
                                {
                                    server.Dispose();
                                    server = null;
                                    break;
                                }
                                server.WaitForPipeDrain();
                            }
                        }
                        catch (IOException ee)
                        {
                            if (server != null)
                            {
                                server.Dispose();
                                server = null;
                            }
                            Console.WriteLine(ee.ToString());
                        }
                    }
                    if (CancellationTokenSource.IsCancellationRequested)
                    {
                        if (server != null)
                        {
                            server.Dispose();
                            server = null;
                        }
                        break;
                    }
                    server = NewServer();
                }
                if (Stopped != null)
                {
                    Stopped(this, EventArgs.Empty);
                }
            }
            catch (ThreadAbortException ee)
            {
                
            }
            catch (Exception ee)
            {
                if (ErrorOccurred != null) 
                {
                    ErrorOccurred(this, ee);
                }
            }
        }
        public void Start()
        {
            if (Task != null)
            {
                Stop();
                Task = null;
            }
            Task = new System.Threading.Thread(new System.Threading.ThreadStart(TaskLoop));
            Task.IsBackground = true;
            Task.Name = "NamedPipeServer";
            Task.Start();
        }
        public void Stop()
        {
            try
            {
                if (CancellationTokenSource != null)
                {
                    CancellationTokenSource.Cancel();
                }
                // Wait for graceful shutdown instead of using Thread.Abort()
                if (Task != null && Task.IsAlive)
                {
                    if (!Task.Join(TimeSpan.FromSeconds(5)))
                    {
                        // Thread didn't stop gracefully, but we've done what we can
                        Console.WriteLine("NamedPipeServer thread did not stop within timeout");
                    }
                    Task = null;
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
        }
    }
}
