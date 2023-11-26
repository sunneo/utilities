using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Utilities.IpcCli
{
    public class IpcCliRoot : BaseIpcCli
    {
        static IpcCliRoot root;
        BaseIpcCliServer currentSourceServer;

        public static IpcCliRoot GetDefault()
        {
            if (root == null)
            {
                root = new IpcCliRoot();
            }
            return root;
        }
        public virtual void SetCurrentIpcServer(BaseIpcCliServer server)
        {
            this.currentSourceServer = server;
        }
        public virtual BaseIpcCliServer GetCurrentIpcServer()
        {
            return this.currentSourceServer;
        }
        protected Dictionary<String, IIpcCli> interfaceMap = new Dictionary<String, IIpcCli>();
        /**
         * register sub Interfaces
         * @param name
         * @param cli
         */
        public virtual void RegisterIIpcCli(String name, IIpcCli cli)
        {
            interfaceMap[name] = cli;
        }
        public IpcCliRoot()
        {

        }



        public override bool RequireLogin()
        {
            return false;
        }

        public override String[] Methods()
        {
            return new String[0];
        }

        public override String[] Interfaces()
        {
            return interfaceMap.Keys.ToArray();
        }

        public override IIpcCli Get(String interfaceName)
        {
            if (interfaceMap.ContainsKey(interfaceName))
            {
                IIpcCli ret0 = interfaceMap[interfaceName];
                return ret0;
            }
            IEnumerable<String> interfaceLayer = Utility.Tokenize(interfaceName, "/");
            IIpcCli curr = this;
            StringBuilder strbPrev = new StringBuilder();
            foreach (String name in interfaceLayer)
            {

                IIpcCli next = null;
                if (curr == this)
                {
                    // it's root
                    if (String.IsNullOrEmpty(name)) continue;
                    if (!interfaceMap.ContainsKey(name))
                    {
                        // wrong interface
                        next = BaseIpcCli.Error(strbPrev.ToString(), name);
                    }
                    else
                    {
                        next = interfaceMap[name];
                        // record previous one
                        strbPrev.Append(name + "/");
                    }
                }
                else
                {
                    next = curr.Get(name);
                    if (next == null || next.IsNull())
                    {
                        next = BaseIpcCli.Error(strbPrev.ToString(), name);
                    }
                    else
                    {
                        strbPrev.Append(name + "/");
                    }
                }
                if (next != curr && next != null && !next.IsNull())
                {
                    curr = next;
                }
            }
            IIpcCli ret = BaseIpcCli.Nullable(curr);
            return ret;
        }

        public override String Invoke(String name, params String[] parms)
        {
            IIpcCli curr = this;
            String fncName = name;
            if (fncName.IndexOf("/") > -1)
            {
                fncName = Utility.Tokenize(name, "/").LastOrDefault().Trim();
                String interfaceName = name.Substring(0, name.LastIndexOf(fncName));
                curr = Get(interfaceName.Trim('/'));
            }
            else
            {
                fncName = name;
            }
            if (curr != null && !curr.IsNull() && curr is NullIpcCli)
            {
                curr = NullIpcCli.GetRealTarGet(curr);
            }
            if (!String.IsNullOrEmpty(fncName) && fncName.StartsWith("@"))
            {
                switch (fncName)
                {
                    case "@methods":
                        return "Methods:\n" + String.Join("\n", curr.Methods());
                    case "@interfaces":
                        {
                            Var<IIpcCli> currTarGet = new Var<IIpcCli>(curr);
                            return "Interfaces:\n" + String.Join("\n", Delegates.ForAll(curr.Interfaces()).Filter((x) =>
                            {
                                IIpcCli res = currTarGet.Value.Get(x);
                                if (res.IsError()) return false;
                                return true;
                            }));
                        }

                }
            }
            else
            {
                if (curr != this)
                {
                    return curr.Invoke(fncName, parms);
                }
            }
            // root has no other method
            return ReturnResult("OK");
        }

    }
}
