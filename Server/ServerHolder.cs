using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Utilities.Server
{
    public class ServerHolder:IDisposable
    {
        public const int CLIENT_SEND_TIMEOUT = 100; 
        public volatile bool AutoClose = true;
        public event EventHandler<OnHandleConnectionEventArgs> OnHandleConnection;
        public event EventHandler<OnHandleConnectionEventArgs> OnServerRemoved;
        public event EventHandler OnServerStopped;
        public DynamicAttributes Attributes = new DynamicAttributes();
        Locker mLocker = new Locker();
        List<MediaConnectionInstance> mServers = new List<MediaConnectionInstance>();
        private class ReadOnlyMediaServerList :  IList<MediaConnectionInstance>
        {
            List<MediaConnectionInstance> list;
            public ReadOnlyMediaServerList(List<MediaConnectionInstance> list)
            {
                this.list = list;
            }
            public MediaConnectionInstance this[int index]
            {
                get { return list[index]; }
                set { }
            }

            public int Count
            {
                get { return list.Count; }
            }

            public IEnumerator<MediaConnectionInstance> GetEnumerator()
            {
                return list.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return list.GetEnumerator();
            }

            public int IndexOf(MediaConnectionInstance item)
            {
                return list.IndexOf(item);
            }

            public void Insert(int index, MediaConnectionInstance item)
            {
                
            }

            public void RemoveAt(int index)
            {
                
            }

            public void Add(MediaConnectionInstance item)
            {
                
            }

            public void Clear()
            {
                
            }

            public bool Contains(MediaConnectionInstance item)
            {
                return list.Contains(item);
            }

            public void CopyTo(MediaConnectionInstance[] array, int arrayIndex)
            {
                list.CopyTo(array, arrayIndex);
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(MediaConnectionInstance item)
            {
                return false;
            }
        }
        ReadOnlyMediaServerList mReadOnlyMediaServerList = null;
        public IList<MediaConnectionInstance> Servers
        {
            get
            {
                if (mReadOnlyMediaServerList == null)
                {
                    mReadOnlyMediaServerList = new ReadOnlyMediaServerList(mServers);
                }
                return mReadOnlyMediaServerList;
            }
        }
        Socket ServerSck;
        
        public void Registry(MediaConnectionInstance m)
        {
            mLocker.Synchronized(() =>
            {
                if (mServers.IndexOf(m) == -1)
                {
                    mServers.Add(m);
                }
            });
            
        }
        private void HandleConnection(MediaConnectionInstance sck)
        {
            bool autoClose = AutoClose;
            sck.Disposed += sck_Disposed;
            AsyncTask task = new AsyncTask(() => {
                if (OnHandleConnection != null)
                {
                    OnHandleConnectionEventArgs args = new OnHandleConnectionEventArgs(this, sck);
                    OnHandleConnection(this, args);
                }
            });
            if (autoClose)
            {
                task.AddAfterFinishJob(() =>
                {
                    try
                    {
                        //Console.WriteLine("Disconnect {0} (PID:{1}) (Second Connection?{2})", sck.IPAddress, sck.Attributes.GetAttributeInt("PID"), sck.Attributes.GetAttributeBool("IsSecondConnection"));
                        if (sck.RawConnection != null)
                        {
                            sck.RawConnection.Close();
                            sck.RawConnection.Dispose();
                        }
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine(ee.ToString());
                    }
                    mLocker.Synchronized(() =>
                    {
                        mServers.Remove(sck);
                    });
                    if (OnServerRemoved != null)
                    {
                        OnServerRemoved(this, new OnHandleConnectionEventArgs(this, sck));
                    }
                });
            }
            task.Start(false);
        }

        void sck_Disposed(object sender, EventArgs e)
        {
            try
            {
                if (sender != null)
                {
                    MediaConnectionInstance sck = (MediaConnectionInstance)sender;
                    mLocker.Synchronized(() =>
                    {
                        mServers.Remove(sck);
                    });
                    if (OnServerRemoved != null)
                    {
                        OnServerRemoved(this, new OnHandleConnectionEventArgs(this, sck));
                    }
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            
        }
        volatile bool bServerRun = false;
        public void Start(int Port)
        {
            try
            {
                
                ServerSck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ServerSck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                IPEndPoint ip = new IPEndPoint(IPAddress.Parse("0.0.0.0"), Port);
                ServerSck.Bind(ip);
                ServerSck.Listen(256);
                bServerRun = true;
                while (bServerRun)
                {
                    Socket sck = ServerSck.Accept();
                    if (sck != null)
                    {
                        MediaConnectionInstance server = new MediaConnectionInstance(sck);
                        HandleConnection(server);
                    }
                }
            }
            
            catch (ThreadAbortException ee)
            {
                
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
            finally
            {
                if (ServerSck != null)
                {
                    ServerSck.Close();
                    ServerSck.Dispose();
                    ServerSck = null;
                }
            }
        }
        AsyncTask AsyncServer;
        public void StartAsync(int Port, bool restartOnFail = false)
        {
            AsyncServer = new AsyncTask(() => {
                Start(Port);
            });
            AsyncServer.AddAfterFinishJob(() => {
                this.AsyncServer = null;
            });
           
            AsyncServer.SetName("ServerHolder:" + Port.ToString());
            AsyncServer.Start(false);
        }

        public void Stop()
        {
            try
            {
                bServerRun = false;
                if (AsyncServer != null)
                {
                    AsyncServer.StopAsync();
                    AsyncServer = null;
                }
                if (ServerSck != null)
                {
                    ServerSck.Close();
                    ServerSck.Dispose();
                    ServerSck = null;   
                }
                if(OnServerStopped != null)
                {
                    OnServerStopped(this, EventArgs.Empty);
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
