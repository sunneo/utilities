using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Utilities.IpcCli
{
    public class BaseIpcCliServer
    {
        static BaseIpcCliServer instance;
        public static BaseIpcCliServer GetDefault()
        {
            if (instance == null)
            {
                instance = new IpcCliFileServerExample();
            }
            return instance;
        }
        public static void SetDefault(BaseIpcCliServer target)
        {
            instance = target;
        }
        public BaseIpcCliServer()
        {
            SetDefault(this);
        }
        public virtual void Start()
        {

        }
        public virtual void Stop()
        {

        }
        /**
         * reply execute result to client
         * @param reply
         */
        public virtual void SendReply(String reply)
        {

        }
        public class IpcRPCRequest
        {
            public String method;
            public String[] args;
            protected List<String> GetTokensByTokenizer(String plain)
            {
                return Utility.Tokenize(plain, " ");
            }

            /**
             * support spaces in arguments surrounded by quote op.
             * i.e. 
             * <pre>
             * echo -e "bmcExample/showComboDialog 1 2 3 4 \"hello world\" \"'string with quote'\" '\"string with double quotes\"'" > ~/.local/Insyde/SupervyseIDE_OPF/process-context/IPCCLI/IN/1.txt; while [ $(ls ~/.local/Insyde/SupervyseIDE_OPF/process-context/IPCCLI/OUT/ | wc -l) -eq "0" ] ; do sleep 1; done; for f in `ls ~/.local/Insyde/SupervyseIDE_OPF/process-context/IPCCLI/OUT/`; do cat ~/.local/Insyde/SupervyseIDE_OPF/process-context/IPCCLI/OUT/$f; rm ~/.local/Insyde/SupervyseIDE_OPF/process-context/IPCCLI/OUT/$f; done
             * 
             *  the list will be: 
             * 
             * 1
             * 2
             * 3
             * 4
             * hello world
             * 'string with quote'
             * "string with double quotes"
             * 
             *  
             * </pre>
             * @param plain
             * @return
             */
            public static List<String> GetTokensByParsing(String plain)
            {
                return GetTokensByParsing(plain, (ch)=>Char.IsWhiteSpace(ch));
            }
            public static List<String> GetTokensByParsing(String plain, char splitter)
            {
                return GetTokensByParsing(plain, (ch)=>ch == splitter);
            }
            public static List<String> GetTokensByParsing(String plain, Func<char, bool> condition)
            {
                List<String> tokens = new List<String>();
                try
                {
                    char[] chars = plain.ToArray();
                    int idx = 0;
                    StringBuilder strb = new StringBuilder();
                    char pairCh = (char)0;
                    while (idx < chars.Length)
                    {
                        char ch = chars[idx];
                        if (pairCh > 0)
                        {
                            if (ch == pairCh && chars[idx] != '\\')
                            {
                                pairCh = (char)0;
                                tokens.Add(strb.ToString());
                                strb = new StringBuilder();
                                ++idx;
                                continue;
                            }
                        }
                        else
                        {
                            if (condition.Invoke(ch))
                            {
                                if (strb.Length > 0)
                                {
                                    // output buffer
                                    tokens.Add(strb.ToString());
                                    strb = new StringBuilder();
                                }
                                ++idx;
                                continue;
                            }
                            if (ch == '\'' || ch == '"')
                            {
                                pairCh = ch;
                                ++idx;
                                continue;
                            }
                        }
                        strb.Append(ch);
                        ++idx;
                    }
                    if (strb.Length > 0)
                    {
                        // output buffer
                        tokens.Add(strb.ToString());
                        strb = new StringBuilder();
                    }
                }
                catch (Exception ee)
                {

                }
                return tokens;
            }
            protected IpcRPCRequest Parse(String plain)
            {
                IpcRPCRequest ret = this;
                try
                {
                    List<String> tokens = GetTokensByParsing(plain);
                    String[] args = new String[0];
                    if (tokens.Count > 1)
                    {
                        List<String> arglist = tokens.Skip(1).ToList();
                        args = Delegates.ForAll(arglist).ToArray();
                        ret.args = args;
                        ret.method = tokens.FirstOrDefault();
                    }
                    else
                    {
                        ret.method = tokens.FirstOrDefault();
                    }
                }
                catch (Exception ee)
                {

                }
                return ret;
            }
            public static IpcRPCRequest FromString(String plain)
            {
                IpcRPCRequest ret = new IpcRPCRequest();
                return ret.Parse(plain);
            }
        }
        /**
         * on get message
         * just an example
         * XXX it is better to wrap request into a JSON format like
         * { method:"", args:[] }
         * @param msg
         */
        public virtual void OnMessage(String msg)
        {
            String plain = msg;
            //        try {
            //           plain = new String(Base64.getDecoder().decode(msg));
            //        }catch(Exception ee) {
            //            
            //        }
            // surround try-catch 
            // to prevent command crash service
            IpcCliRoot.GetDefault().SetCurrentIpcServer(this);
            try
            {
                IpcRPCRequest req = IpcRPCRequest.FromString(plain);
                String reply = "";
                if (req.args != null && req.args.Length > 0)
                {
                    reply = IpcCliRoot.GetDefault().Invoke(req.method, req.args);
                }
                else
                {
                    reply = IpcCliRoot.GetDefault().Invoke(req.method);
                }
                if (reply == null)
                {
                    reply = "OK";
                }
                SendReply(reply);
            }
            catch (Exception ee)
            {
                SendReply("Error \n" + ee.ToString());
            }



        }
    }
}
