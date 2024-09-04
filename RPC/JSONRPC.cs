using Utilities.Coroutine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace Utilities.RPC
{
    public class JSONRPCError
    {
        public String Message;
        public Exception error;
    }
    public interface ITracer
    {
        void Log(String fmt, params object[] param);
    }
    public class NullTracer : ITracer
    {
        public void Log(string fmt, params object[] param)
        {
            
        }
    }
    public class StdErrorTracer : ITracer
    {
        public void Log(string fmt, params object[] param)
        {
            Console.Error.WriteLine(fmt, param);
        }
    }

    public interface IJsonRpcClient
    {
        bool IsInvoking { get; }
        JSONRPCMessage Invoke(JSONRPCMessage msg);
        JSONRPCMessage<T> TypedInvoke<T>(JSONRPCMessage msg) where T : class;
        object Invoke(String name, params object[] args);
    }
    public class JsonRpcServerHandleEventArgs : EventArgs
    {
        public JSONRPCMessage Input;
        public JSONRPCMessage Output;
    }
    public interface IJsonRpcServer
    {
        event EventHandler<JsonRpcServerHandleEventArgs> OnHandleRPC;
        event EventHandler OnRequestHandled;
        event EventHandler<JSONRPCError> OnError;
        GenericDataSet Properties { get; }
        void Start(bool thread=false);
        void Stop(bool thread=false);
        bool IsAlive { get; }

        void DoEvent();
    }
    

    public class JSONRPC
    {
        TextReader Reader;
        TextWriter Writer;
        public event EventHandler<JSONRPCError> OnError;
        public const bool DoLog = false;
        protected static ITracer _tracer;
        public static ITracer Tracer
        {
            get
            {
                if (_tracer == null)
                {
                    if (DoLog)
                    {
                        _tracer = new StdErrorTracer();
                    }
                    else
                    {
                        _tracer = new NullTracer();
                    }
                }
                return _tracer;
            }
        }
        internal class ClientImpl : IJsonRpcClient
        {
            volatile bool mInvoking = false;
            object channelLocker = new object();
            JSONRPC mRPC;
            public JSONRPCMessage Invoke(JSONRPCMessage msg)
            {
                lock (channelLocker)
                {
                    bool retry = false;
                    do
                    {
                        retry = false;
                        mInvoking = true;
                        if (mRPC.Send(msg))
                        {
                            JSONRPCMessage ret = null;
                            int timeout = msg.timeoutMillis;
                            bool isTimeout = false;
                            if (mRPC.TryGet(true, timeout, out isTimeout, out ret))
                            {
                                mInvoking = false;
                                return ret;
                            }
                            if (timeout > 0 && isTimeout)
                            {
                                retry = true;
                            }
                        }
                        mInvoking = false;
                    } while (retry);
                    
                }
                return null;
            }
            public object Invoke(string name, params object[] args)
            {
                if (mRPC == null)
                {
                    return null;
                }
                lock (channelLocker)
                {
                    mInvoking = true;
                    mRPC.Send(name, args);
                    JSONRPCMessage ret = null;
                    if (mRPC.TryGet(out ret))
                    {
                        mInvoking = false;
                        return ret;
                    }
                    mInvoking = false;
                }
                return null;
            }
            public ClientImpl(JSONRPC rpc)
            {
                mRPC = rpc;
            }
            public ClientImpl(TextReader reader, TextWriter writer)
            {
                mRPC = new JSONRPC(reader, writer);
            }

            public bool IsInvoking
            {
                get { return mInvoking; }
            }


            public JSONRPCMessage<T> TypedInvoke<T>(JSONRPCMessage msg) where T : class
            {
                if (mRPC == null)
                {
                    return null;
                }
                lock (channelLocker)
                {
                    mInvoking = true;
                    mRPC.Send(msg);
                    JSONRPCMessage<T> ret = null;
                    if (mRPC.TryGet(out ret))
                    {
                        mInvoking = false;
                        return ret;
                    }
                    mInvoking = false;
                }
                return null;
            }
        }
        internal class ServerImpl : IJsonRpcServer
        {
            JSONRPC mRPC;
            CoroutineHost Host = new CoroutineHost(100);
            Coroutine.Coroutine Cor;
            volatile bool IsRunning = false;
            GenericDataSet mProperties = new GenericDataSet();
            public GenericDataSet Properties { get => mProperties; }
            public event EventHandler<JSONRPCError> OnError;
            public bool IsAlive => IsRunning;
            public event EventHandler OnRequestHandled;

            public event EventHandler<JsonRpcServerHandleEventArgs> OnHandleRPC;

            public ServerImpl(JSONRPC rpc)
            {
                this.mRPC = rpc;
            }
            public ServerImpl(TextReader reader, TextWriter writer)
            {
                mRPC = new JSONRPC(reader, writer);
            }
            public void DoEvent()
            {
                JsonRpcServerHandleEventArgs args = new JsonRpcServerHandleEventArgs();
                if (mRPC.TryGet(out args.Input, OnError))
                {
                    OnHandleRPC(this, args);
                    mRPC.Send(args.Output);
                }
                try
                {
                    if (OnRequestHandled != null)
                    {
                        OnRequestHandled(this, EventArgs.Empty);
                    }
                }
                catch (Exception ee)
                {
                    Console.Error.WriteLine(ee.ToString());
                }
            }
            public void ThreadRunner()
            {
                IsRunning = true;
                if (OnHandleRPC != null)
                {
                    while (IsRunning)
                    {
                        DoEvent();
                    }
                }
                IsRunning = false;
            }
            IEnumerator Runner()
            {
                IsRunning = true;
                if (OnHandleRPC != null)
                {
                    while (IsRunning)
                    {
                        DoEvent();
                        yield return true;
                    }
                }
                IsRunning = false;
                yield break;
            }
            public void Start(bool thread = false)
            {
                if(thread)
                {
                    StartThread();
                    return;
                }
                if (Cor == null)
                {
                    Cor = new Coroutine.Coroutine(50, this.Host);
                }
                Cor.QueueWorkingItem(Runner());
                IsRunning = true;
            }
            protected Thread th;
            public void StartThread()
            {
                if (th == null || !th.IsAlive)
                {
                    th = new Thread(ThreadRunner);
                }
                th.Start();
                IsRunning = true;
            }
            public void Stop(bool thread = false)
            {
                if (thread)
                {
                    StopThread();
                    return;
                }
                IsRunning = false;
                Cor.Dispose();
                Host.Dispose();
            }
            public void StopThread()
            {
                IsRunning = false;
                try
                {
                    if (this.th != null)
                    {
                        this.th.Interrupt();
                        this.th.Abort();
                    }
                }
                catch(Exception ee)
                {
                    Console.Error.WriteLine(ee.ToString());
                }
            }
        }
        public IJsonRpcClient Client()
        {
            return new ClientImpl(this);
        }
        public IJsonRpcServer Server()
        {
            return new ServerImpl(this);
        }
        public void Dispose()
        {
            if (readerCancellable != null)
            {
                readerCancellable.Cancel();
            }
        }
        volatile Coroutine.Cancellable readerCancellable = new Cancellable();
        public JSONRPC(TextReader reader, TextWriter writer)
        {
            this.Reader = reader;
            this.Writer = writer;
        }
        public bool TryGet<T>(out T output, EventHandler<JSONRPCError> OnErrorHandler = null) where T : class
        {
            bool dummyTimeout = false;
            return TryGet(false, -1,out dummyTimeout, out output, OnErrorHandler);
        }
        public bool TryGet<T>(bool untilParsed,int timeoutMillis, out bool isTimeout,out T output, EventHandler<JSONRPCError> OnErrorHandler=null) where T : class
        {
            bool retry = true;
            if (!untilParsed)
            {
                retry = false;
            }
            do
            {
                //String content = Reader.ReadLine();
                String content = "";
                
                if (timeoutMillis > 0)
                {
                    Var<String> varString = new Var<string>("");
                    Var<bool> done = new Var<bool>(false);
                    Coroutine.Cancellable readerCancellable = this.readerCancellable;
                    Thread th = new Thread(() =>
                    {
                        Tracer.Log("[JSONRPC] Wait for String");
                        varString.Value = Reader.ReadLine();
                        Tracer.Log("[JSONRPC] Wait for String, Get {0}", varString.Value);
                        done.Value = true;
                    });
                    th.Start();
                    DateTime dt = DateTime.Now;
                    while (!readerCancellable.CancellationPending && !done.Value)
                    {
                        if (timeoutMillis > 0)
                        {
                            if (DateTime.Now.Subtract(dt).TotalMilliseconds >= timeoutMillis)
                            {
                                isTimeout = true;
                                output = null;
                                return false;
                            }
                        }
                        Thread.Sleep(16);
                    }
                    content = varString.Value;
                }
                else
                {
                    content = Reader.ReadLine();
                }
                
                if (String.IsNullOrEmpty(content))
                {
                    output = null;
                    JSONRPCError err = new JSONRPCError() { Message = "Get Empty String" };
                    if (OnError != null)
                    {
                        OnError(this, err);
                    }
                    if (OnErrorHandler != null)
                    {
                        OnErrorHandler(this, err);
                    }
                    isTimeout = false;
                    return false;
                }
                try
                {
                    output = Utility.JSON.Deserialize<T>(content);
                }
                catch (Exception ee)
                {
                    output = null;
                    JSONRPCError err = new JSONRPCError() { error = ee, Message = ee.Message };
                    if (OnError != null)
                    {
                        OnError(this, err);
                    }
                    if (OnErrorHandler != null)
                    {
                        OnErrorHandler(this, err);
                    }
                }
                if (untilParsed)
                {
                    if(output != null)
                    {
                        retry = false;
                    }
                }
            } while (retry);
            isTimeout = false;
            return output != null;
        }
        public bool Send(JSONRPCMessage msg)
        {
            String strmsg = Utility.JSON.Serialize(msg);
            if (readerCancellable.CancellationPending)
            {
                Tracer.Log("[JSONRPC] When Sending {0}, but Cancelled", strmsg);
                return false;
            }
            try
            {
                Tracer.Log("[JSONRPC] Sending {0}", strmsg);
                Writer.WriteLine(strmsg);
                Writer.Flush();
                Tracer.Log("[JSONRPC] Sent {0}", strmsg);
            }
            catch(Exception ee)
            {
                Tracer.Log("[JSONRPC] Send {0} failed", strmsg);
                Console.Error.WriteLine(ee.ToString());
                return false;
            }
            return true;
        }
        public bool Send(String name, params object[] args)
        {
            JSONRPCMessage msg = new JSONRPCMessage(name, args);
            return Send(msg);
        }
        public JSONRPCMessage Invoke(String name, params object[] args)
        {
            JSONRPCMessage ret = null;
            if (TryGet<JSONRPCMessage>(out ret))
            {
                return ret;
            }
            return new JSONRPCMessage();
        }
    }
    public class Arguments
    {
        public List<object> args = new List<object>();
        public void Assign(params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                this.args = new List<object>();
                this.args.AddRange(args);
            }
        }
        public long GetLong(int idx)
        {
            if (idx < args.Count)
            {
                return (long)(Int64)args[idx];
            }
            return -1;
        }
        public int GetInt(int idx)
        {
            if (idx < args.Count)
            {
                return (int)(Int64)args[idx];
            }
            return -1;
        }
        public double GetDouble(int idx)
        {
            if (idx < args.Count)
            {
                return (double)args[idx];
            }
            return -1;
        }
        public bool TryGetArrayArg<T>(int idx, out T[] outArray)
        {
            if (idx < args.Count)
            {
                if (args[idx] is JArray)
                {
                    JArray array = args[idx] as JArray;
                    T[] ret = array.Values<T>().ToArray();
                    outArray = ret;
                    return true;
                }
            }
            outArray = new T[0];
            return false;
        }
    }
    public class JSONRPCMessage : JSONRPCMessage<Object>
    {
        public JSONRPCMessage()
        {

        }
        public JSONRPCMessage(String method, params object[] args)
            : base(method, args)
        {
        }
    }
    public class JSONRPCMessage<T> where T : class
    {
        public bool isTimeout = false;
        public int timeoutMillis = -1;
        public String jsonrpc = "2.0";
        public String id = "1";
        public String method = "";
        public Arguments Params = new Arguments();
        public T Result = null;
        public bool TryGetArrayArg<T>(int idx, out T[] outArray)
        {
            return Params.TryGetArrayArg<T>(idx, out outArray);
        }
        public bool TryGetArrayResult<T>(out T[] outArray)
        {
            if (Result is JArray)
            {
                JArray array = Result as JArray;
                T[] ret = array.Values<T>().ToArray();
                outArray = ret;
                return true;
            }
            outArray = new T[0];
            return false;
        }
        public JSONRPCMessage()
        {

        }
        public JSONRPCMessage(String method, params object[] args)
        {
            this.method = method;
            this.Params.Assign(args);
        }
    }
}
