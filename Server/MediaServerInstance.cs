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
        public MediaConnectionInstance(Socket sck)
        {
            this.RawConnection = sck;
            this.IPAddress = (sck.RemoteEndPoint as System.Net.IPEndPoint).Address.ToString();
            NetworkStream mStream = new NetworkStream(sck,true);
            BufferedStream writerBuffer = new BufferedStream(mStream);
            BufferedStream readerBuffer = new BufferedStream(mStream);
            this.Writer = new BinaryWriter(writerBuffer);
            this.Reader = new BinaryReader(readerBuffer);
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
