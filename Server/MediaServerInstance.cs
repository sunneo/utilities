using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities.Server
{
    public class MediaConnectionInstance:IDisposable
    {
        Locked<bool> IsDisposed = false;
        public event EventHandler Disposed;
        public event EventHandler ConnectionClosed;
        public Socket RawConnection
        {
            get;
            private set;
        }
        public String IPAddress
        {
            get;
            private set;
        }

        public BinaryWriter Writer { get; private set; }
        public BinaryReader Reader { get; private set; }

        public Locker WriterLocker = new Locker();
        public Locker ReaderLocker = new Locker();

        public IDisposable BeginWriterLocker()
        {
            return WriterLocker.Lock();
        }
        public IDisposable BeginReaderLocker()
        {
            return ReaderLocker.Lock();
        }


        public DynamicAttributes Attributes = new DynamicAttributes();

        public String GetStringAttribute(String key)
        {
            return Attributes.GetAttributeString(key).ToString();
        }
        BufferedStream mBufferedStream;
        
        public MediaConnectionInstance(Socket sck)
        {
            this.RawConnection = sck;
            this.IPAddress = (sck.RemoteEndPoint as System.Net.IPEndPoint).Address.ToString();
            NetworkStream mStream = new NetworkStream(sck, true);
            // Use single BufferedStream to avoid double-disposal issue
            mBufferedStream = new BufferedStream(mStream);
            // Both reader and writer use leaveOpen=true so they don't dispose the BufferedStream
            // We'll dispose the BufferedStream explicitly in Dispose()
            this.Writer = new BinaryWriter(mBufferedStream, Encoding.UTF8, true);
            this.Reader = new BinaryReader(mBufferedStream, Encoding.UTF8, true);
        }
        public static MediaConnectionInstance New(String ip, int port)
        {
            try
            {
                Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sck.Connect(ip, port);
                return new MediaConnectionInstance(sck);
            }
            catch(Exception ee)
            {
                Console.WriteLine(ee.ToString());
                return null;
            }
        }



        public void Dispose()
        {
            if (IsDisposed) return;
            try
            {
                if (Writer != null)
                {
                    Writer.Dispose();
                    Writer = null;
                }
            }
            catch (Exception ee)
            {
                // Ignore exceptions during disposal to prevent further issues
                Console.WriteLine("Warning: Exception during Writer disposal: " + ee.Message);
            }
            try
            {
                if (Reader != null)
                {
                    Reader.Dispose();
                    Reader = null;
                }
            }
            catch (Exception ee)
            {
                // Ignore exceptions during disposal to prevent further issues
                Console.WriteLine("Warning: Exception during Reader disposal: " + ee.Message);
            }
            try
            {
                if (mBufferedStream != null)
                {
                    mBufferedStream.Dispose();
                    mBufferedStream = null;
                }
            }
            catch (Exception ee)
            {
                // Ignore exceptions during disposal to prevent further issues
                Console.WriteLine("Warning: Exception during BufferedStream disposal: " + ee.Message);
            }
            try
            {
                if (RawConnection != null)
                {
                    RawConnection.Dispose();
                    RawConnection = null;
                }
            }
            catch (Exception ee)
            {
                // Ignore exceptions during disposal to prevent further issues
                Console.WriteLine("Warning: Exception during Socket disposal: " + ee.Message);
            }
            IsDisposed = true;
            if (Disposed != null)
            {
                this.Disposed(this, EventArgs.Empty);
            }
        }
        ~MediaConnectionInstance()
        {
            Dispose();
        }
    }
}
