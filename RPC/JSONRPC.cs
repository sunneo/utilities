using Utilities.Coroutine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.RPC
{
    public class JSONRPCError
    {
        public String Message;
        public Exception error;
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
        void Start();
        void Stop();
        bool IsAlive { get; }
    }
    

    public class JSONRPC
    {
        TextReader Reader;
        TextWriter Writer;
        public event EventHandler<JSONRPCError> OnError;
        internal class ClientImpl : IJsonRpcClient
        {
            volatile bool mInvoking = false;
            object channelLocker = new object();
            JSONRPC mRPC;
            public JSONRPCMessage Invoke(JSONRPCMessage msg)
            {
                lock (channelLocker)
                {
                    mInvoking = true;
                    mRPC.Send(msg);
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
            IEnumerator Runner()
            {
                IsRunning = true;
                if (OnHandleRPC != null)
                {
                    while (IsRunning)
                    {
                        JsonRpcServerHandleEventArgs args = new JsonRpcServerHandleEventArgs();
                        if (mRPC.TryGet(out args.Input))
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
                        catch(Exception ee)
                        {

                        }
                        yield return true;
                    }
                }
                IsRunning = false;
                yield break;
            }
            public void Start()
            {
                if (Cor == null)
                {
                    Cor = new Coroutine.Coroutine(50, this.Host);
                }
                Cor.QueueWorkingItem(Runner());
                IsRunning = true;
            }
            public void Stop()
            {
                IsRunning = false;
                Cor.Dispose();
                Host.Dispose();
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

        public JSONRPC(TextReader reader, TextWriter writer)
        {
            this.Reader = reader;
            this.Writer = writer;
        }
        public bool TryGet<T>(out T output) where T : class
        {
            String content = Reader.ReadLine();
            if (String.IsNullOrEmpty(content))
            {
                output = null;
                if (OnError != null)
                {
                    OnError(this, new JSONRPCError() { Message = "Get Empty String" });
                }
                return false;
            }
            try
            {
                output = Utility.JSON.Deserialize<T>(content);
            }
            catch(Exception ee)
            {
                output = null;
                if(OnError != null)
                {
                    OnError(this, new JSONRPCError() { error = ee, Message=ee.Message });
                }
            }
            return output != null;
        }
        public void Send(JSONRPCMessage msg)
        {
            Writer.WriteLine(Utility.JSON.Serialize(msg));
            Writer.Flush();
        }
        public void Send(String name, params object[] args)
        {
            JSONRPCMessage msg = new JSONRPCMessage(name, args);
            Send(msg);
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
                this.args.AddRange(args);
            }
        }
        public int GetInt(int idx)
        {
            if (idx < args.Count)
            {
                return (int)(Int64)args[idx];
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
