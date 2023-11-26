using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Utilities.IpcCli
{

    public interface IIpcCli
    {/**
     * Get sub Interfaces
     * ex: SupervyseIDE.GetInterfaces()
     *     ["project","downloader"...]
     * @return
     */
        String[] Interfaces();
        /**
         * return method names
         * it is better to rename overloading with different signature.
         * @return
         */
        String[] Methods();
        /**
         * Get interface by given interfaceName
         * @param interfaceName
         * @return
         */
        IIpcCli Get(String interfaceName);
        /**
         * call method
         * name can be nested by backslash i.e.  downloader/git/downloadSourceCode
         * 
         * or indirectly acquire interface
         * SupervyseIDE.Get("downloader").Get("git").Invoke("downloadSourceCode",URL)
         * 
         * It is better to implement each function synchronuously
         * IDE should wait until method finished.
         * @param name method name
         * @param params
         * @return
         */
        String Invoke(String name, params String[] parms);

        bool IsNull();

        bool IsError();
        /**
         * whether this interface should be activated after logged in
         * @return
         */
        bool RequireLogin();
        String ReturnResult(String message);
        String ReturnAsyncJobStarted();
    }
    public class BaseIpcCli : IIpcCli
    {
        public static IIpcCli Nullable(IIpcCli tarGet)
        {
            return new NullIpcCli(tarGet);
        }
        public static IIpcCli Error(String req)
        {
            return Error("", req);
        }
        public static IIpcCli Error(String prevReq, String req)
        {
            return new ErrorIpcCli(prevReq, req);
        }
        public static IIpcCli Error(String prevReq, String req, String msg)
        {
            return new ErrorIpcCli(prevReq, req, msg);
        }
        public virtual IIpcCli Get(string interfaceName)
        {
            return null;
        }

        public virtual string[] Interfaces()
        {
            return new string[] { };
        }

        public virtual string Invoke(string name, params string[] parms)
        {
            return "";
        }

        public virtual bool IsError()
        {
            return false;
        }

        public virtual bool IsNull()
        {
            return false;
        }

        public virtual string[] Methods()
        {
            return new string[] { };
        }

        public virtual bool RequireLogin()
        {
            return false;
        }

        public virtual string ReturnAsyncJobStarted()
        {
            return "";
        }

        public virtual string ReturnResult(string message)
        {
            return "";
        }
    }

    class ErrorIpcCli : BaseIpcCli
    {
        String request;
        String prev;
        String msg;
        public ErrorIpcCli(String prev, String request, String msg)
        {
            this.prev = prev;
            this.request = request;
            this.msg = msg;
        }
        public ErrorIpcCli(String prev, String request) : this(prev, request, "")
        {

        }
        public ErrorIpcCli(String request)
        {
            this.request = request;
        }


        public override bool IsError()
        {
            return true;
        }

        public override bool RequireLogin()
        {
            return false;
        }
        protected String GetErrorMessage()
        {
            String ret = "";
            if (!String.IsNullOrEmpty(msg))
            {
                ret = ret + "\n" + msg;
            }
            else
            {
                ret = "[ERROR] Wrong Interface:" + request;
            }
            if (!String.IsNullOrEmpty(prev))
            {
                ret = ret + "\n" + "In Interface:" + prev + "\nPlease choose available interface by " + prev.TrimEnd('/') + "/@Interfaces";
            }
            return ret;
        }

        public override String[] Interfaces()
        {
            return Utility.List<String>(GetErrorMessage());
        }


        public override String[] Methods()
        {
            return Utility.List<String>(GetErrorMessage());
        }


        public override IIpcCli Get(String interfaceName)
        {
            return null;
        }


        public override String Invoke(String name, params String[] parms)
        {
            return GetErrorMessage();
        }

    }

    class NullIpcCli : BaseIpcCli
    {
        IIpcCli tarGet;


        public override bool IsNull()
        {
            return tarGet == null;
        }


        public override bool IsError()
        {
            if (tarGet != null)
            {
                return tarGet.IsError();
            }
            return false;
        }

        /**
         * dereference Nullable
         * resolve tarGet from NullIpcCli
         * @param tarGet
         * @return real tarGet, include null
         */
        public static IIpcCli GetRealTarGet(IIpcCli tarGet)
        {

            while (tarGet != null && (tarGet.GetType() == typeof(NullIpcCli)))
            {
                NullIpcCli nullable = (NullIpcCli)tarGet;
                tarGet = nullable.tarGet;
            }
            return tarGet;
        }
        public NullIpcCli(IIpcCli tarGet)
        {
            this.tarGet = GetRealTarGet(tarGet);
        }


        public override bool RequireLogin()
        {
            if (tarGet != null)
            {
                return tarGet.RequireLogin();
            }
            return false;
        }

        public override String[] Interfaces()
        {
            if (tarGet != null)
            {
                return tarGet.Interfaces();
            }
            return new String[0];
        }


        public override String[] Methods()
        {
            if (tarGet != null)
            {
                return tarGet.Methods();
            }
            return new String[0];
        }


        public override IIpcCli Get(String interfaceName)
        {
            IIpcCli ret = this;
            if (tarGet != null)
            {
                ret = tarGet.Get(interfaceName);
            }
            if (ret == null)
            {
                ret = this;
            }
            return ret;
        }


        public override String Invoke(String name, params String[] parms)
        {
            if (tarGet != null)
            {
                return tarGet.Invoke(name, parms);
            }
            return null;
        }

    }
}
