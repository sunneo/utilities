using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Utilities
{
    public class Utility
    {
        public class JSON
        {
            public static T Deserialize<T>(String data)
            {
                try
                {
                    using (JsonTextReader txtReader = new JsonTextReader(new StringReader(data)))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        return serializer.Deserialize<T>(txtReader);
                    }
                }
                catch (Exception ex)
                {
                    return default(T);
                }
            }
            public static String Serialize(object o)
            {
                return Serialize(o, false);
            }
            public static String Serialize(object o, bool formatted)
            {
                StringBuilder strb = new StringBuilder();
                using (TextWriter writer = new StringWriter(strb))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    if(formatted)
                    {
                        serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                    }
                    serializer.Serialize(writer, o);
                }
                return strb.ToString();
            }
        }
        public static StreamReader SharedStreamReader(String filepath, Encoding _encoding)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader freader = new StreamReader(fs, _encoding, true);
            return freader;
        }
        public static StreamReader SharedStreamReader(String filepath)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader freader = new StreamReader(fs, Encoding.Default, true);
            return freader;
        }
        /// Get the leftmost n characters of a string, return empty string if error.
        public static String Left(String str, int n)
        {
            if (n < 0)
            {
                Tracer.D("Unreasonable Parameter: " + n); //$NON-NLS-1$
                return str;
            }

            try
            {
                int strLen = str.Length;
                if (n > strLen)
                {
                    n = strLen;
                }

                return str.Substring(0, n);
            }
            catch (Exception e)
            {
                return "";
            }
        }
        public static List<String> Tokenize(String str, String delimiter)
        {
            if (String.IsNullOrEmpty(str))
            {
                return new List<String>();
            }
            List<String> tokens;
            tokens = new List<String>();
            int len = str.Length;
            int idx = 0;
            String val = str;
            int delimLen = delimiter.Length;
            while (idx < len)
            {
                int tok = val.IndexOf(delimiter);
                if (tok < 0)
                {
                    tokens.Add(val);
                    break;
                }
                String left = Left(val, tok);
                tokens.Add(left);
                idx = tok + delimLen;
                val = val.Substring(tok + delimLen);
            }
            return tokens;
        }
        public static T[] List<T>(params T[] parms)
        {
            return parms;
        }
        public static StreamReader SharedUTF8StreamReader(String filepath)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader freader = new StreamReader(fs, Encoding.UTF8, true);
            return freader;
        }
        static LRUDictionary<Type, XmlSerializer> mTypePool;
        static LRUDictionary<Type, XmlSerializer> TypePool
        {
            get
            {
                if (mTypePool == null)
                {
                    mTypePool = new LRUDictionary<Type, XmlSerializer>(128);
                }
                return mTypePool;
            }
        }
        public static XmlSerializer GetTypeSerializer(Type type, params Type[] extraTypes)
        {
            XmlSerializer serializer = TypePool.Get(type);
            if (serializer == null)
            {
                serializer = new XmlSerializer(type, extraTypes);
                //serializer = XmlSerializer.FromTypes(new Type[] { type })[0];
                TypePool.Put(type, serializer);
            }
            return serializer;
        }
        public static Process RunCommandLine(String file,String[] args)
        {
            Process process = new Process();
            ProcessStartInfo info = process.StartInfo;
            info.CreateNoWindow = true;
            info.FileName = file;
            if(args != null && args.Length> 0)
            {
                info.Arguments = String.Join(" ", args);
            }
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.WorkingDirectory = Environment.CurrentDirectory;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardInput = true;
            info.RedirectStandardError = true;
            process.EnableRaisingEvents = true;
            process.Start();
            process.ErrorDataReceived += (s, e) =>
            {
                Console.Error.WriteLine("[RunCommandLine][file={0}]:{1}", file, e.Data);
            };
            process.BeginErrorReadLine();
            return process;
        }
        public static int RunCommandLine(String file, String[] args, out String stdout, out String stderr)
        {
            StringBuilder strbStdout = new StringBuilder();
            StringBuilder strbError = new StringBuilder();
            Process process = RunCommandLine(file, args);
            
            process.OutputDataReceived += (s, e) =>
            {
                strbStdout.AppendLine(e.Data);
            };
            process.ErrorDataReceived += (s, e) =>
            {
                strbError.AppendLine(e.Data);
            };
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            stdout = strbStdout.ToString();
            stderr = strbError.ToString();
            return process.ExitCode;
        }
        public static string Serialize(object o,params Type[] extraTypes)
        {
            try
            {
                if (o != null)
                {
                    XmlSerializer ser = GetTypeSerializer(o.GetType(), extraTypes);
                    StringBuilder sb = new StringBuilder();
                    StringWriter writer = new StringWriter(sb);
                    ser.Serialize(writer, o);
                    return sb.ToString();
                }
                
            }
            catch (Exception ee)
            {

            }
            return "";

        }

        public static T Deserialize<T>(string s,params Type[] extra)
        {
            if (string.IsNullOrEmpty(s))
                return default(T);

            try
            {
                XmlSerializer ser = GetTypeSerializer(typeof(T),extra);
                using (var fileReader = new FileStream(s, FileMode.Open, FileAccess.Read))
                using(var sr = new StreamReader(fileReader))
                {
                    object obj = ser.Deserialize(sr);

                    return (T)obj;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return default(T);
            }
        }
        public static IEnumerable<String> EnumeratorStringToLines(String stringContent)
        {
            int length = stringContent.Length;
            StringBuilder strb = new StringBuilder();
            for (int i = 0; i < length; ++i)
            {
                char c = stringContent[i];
                if (c == '\n')
                {
                    yield return strb.ToString();
                    strb.Clear();
                    continue;
                }
                else if (c == '\r')
                {
                    if (i + 1 < length && stringContent[i + 1] == '\n')
                    {
                        yield return strb.ToString();
                        strb.Clear();
                        i += 1;
                        continue;
                    }
                }
                strb.Append(c);
            }
            if (strb.Length > 0)
            {
                yield return strb.ToString();
            }
            yield break;
        }
        public static IEnumerable<T> ForeachTokenize<T>(String str, String[] tokenizer, Func<String, Utilities.Var<T>, bool> parser)
        {
            foreach (String token in str.Split(tokenizer, StringSplitOptions.RemoveEmptyEntries))
            {
                Utilities.Var<T> value = new Utilities.Var<T>();
                if (!parser(token, value)) continue;
                if (!value.HasValue) continue;
                yield return value.Value;
            }
            yield break;
        }
        public static bool TryParse(String s, Utilities.Var<int> var)
        {
            int value = 0;
            if (int.TryParse(s, out value))
            {
                var.Value = value;
                return true;
            }
            return false;
        }
        public static bool TryParse(String s, Utilities.Var<bool> var)
        {
            bool value = false;
            if (bool.TryParse(s, out value))
            {
                var.Value = value;
                return true;
            }
            return false;
        }
        public static bool TryParse(String s, Utilities.Var<float> var)
        {
            float value = 0;
            if (float.TryParse(s, out value))
            {
                var.Value = value;
                return true;
            }
            return false;
        }
        public static bool TryParse(String s, Utilities.Var<long> var)
        {
            long value = 0;
            if (long.TryParse(s, out value))
            {
                var.Value = value;
                return true;
            }
            return false;
        }
        public static bool TryParse(String s, Utilities.Var<double> var)
        {
            double value = 0;
            if (double.TryParse(s, out value))
            {
                var.Value = value;
                return true;
            }
            return false;
        }
        public class LooperTask
        {
            // time period in seconds
            public int Period;
            Locker locker = new Locker();
            Locker lockerRunner = new Locker();
            public LooperTask(int period)
            {
                this.Period = period;
            }
            Coroutine.Cancellable cancellable = new Coroutine.Cancellable();
            public SortedDictionary<DateTime, LinkedList<Action>> tasks = new SortedDictionary<DateTime, LinkedList<Action>>();
            private static DateTime GetDateTime(int hour, int min, int sec)
            {
                return GetDateTime(hour, min, sec, false);
            }
            /// <summary>
            /// create datetime
            /// </summary>
            /// <param name="hour"></param>
            /// <param name="min"></param>
            /// <param name="sec"></param>
            /// <param name="toNextDay">Point to next day if assigned time has exceeded now</param>
            /// <returns></returns>
            private static DateTime GetDateTime(int hour,int min,int sec, bool toNextDay)
            {
                DateTime now = DateTime.Now;
                DateTime dt = new DateTime(now.Year, now.Month, now.Day, hour, min, sec);
                if (toNextDay)
                {
                    if (dt < now)
                    {
                        dt = dt.AddDays(1);
                    }
                }
                return dt;
            }
            public LinkedListNode<Action> RegisterJob(Action action,int hour,int min,int sec)
            {
                DateTime dt = GetDateTime(hour, min, sec, true);
                
                LinkedList<Action> actions = new LinkedList<Action>();
                LinkedListNode<Action> ret = null;
                using (var token = locker.Lock())
                {
                    if (tasks.ContainsKey(dt))
                    {
                        actions = tasks[dt];
                    }
                    else
                    {
                        tasks[dt] = actions;
                    }
                    ret = actions.AddLast(action);
                }
                return ret;
            }
            public void Cancel(LinkedListNode<Action> action)
            {
                if (action == null) return;
                using (var token = locker.Lock())
                {
                    action.List.Remove(action);
                }
            }
            public void Cancel(int hour,int min,int sec)
            {
                DateTime dt = GetDateTime(hour, min, sec);
                using (var token = locker.Lock())
                {
                    if (tasks.ContainsKey(dt))
                    {
                        tasks[dt].Clear();
                    }
                    tasks.Remove(dt);
                    DateTime next = dt.AddDays(1);
                    if (tasks.ContainsKey(next))
                    {
                        tasks[next].Clear();
                    }
                    tasks.Remove(next);
                }
            }
            public void Cancel()
            {
                using (var token = locker.Lock())
                {
                    tasks.Clear();
                }
            }
            LinkedList<Action> actionsScheduleToRun = new LinkedList<Action>();
            protected virtual void Runner()
            {
                while (!cancellable.CancellationPending)
                {
                    LinkedList<Action> actionList = new LinkedList<Action>();
                    // only lock this section
                    // make list be re-new
                    using (var runnerLockerCtx = this.lockerRunner.Lock())
                    {
                        actionList = actionsScheduleToRun;
                        actionsScheduleToRun = new LinkedList<Action>();
                    }
                    if (actionList.Count != 0)
                    {
                        int count = actionList.Count;
                        Dictionary<LinkedListNode<Action>, LinkedListNode<Action>> visited = new Dictionary<LinkedListNode<Action>, LinkedListNode<Action>>();
                        LinkedListNode<Action> actionNode = actionList.First;
                        
                        int hit = 0;
                        while (actionNode != null)
                        {
                            if (visited.ContainsKey(actionNode))
                            {
                                if(hit < count)
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            ++hit;
                            if (actionNode.Value != null)
                            {
                                try
                                {
                                    actionNode.Value();
                                }
                                catch (Exception ee)
                                {
                                    Tracer.D(ee.ToString());
                                }
                            }
                            visited[actionNode] = actionNode;
                            LinkedListNode<Action> next = actionNode.Next;
                            actionNode = next;
                        }
                        actionList.Clear();
                    }
                    Thread.Sleep(Period*1000);
                }
            }
            protected virtual void Scheduler()
            {
                Coroutine.Cancellable cancellable = this.cancellable;
                DateTime previouseTrigger = DateTime.Now;
                while(!cancellable.CancellationPending)
                {
                    try
                    {
                        DateTime now = DateTime.Now;
                        using (var token = locker.Lock())
                        {
                            List<DateTime> keyList = tasks.Keys.ToList();
                            for(int i=0; i<keyList.Count; ++i) 
                            {
                                DateTime dtBeforeNow = keyList[i];
                                if (dtBeforeNow > now) break;
                                LinkedList<Action> list = tasks[dtBeforeNow];
                                // just lock to transfer jobs(only action) into running queue
                                using(var runnerLockerCtx = this.lockerRunner.Lock())
                                {
                                    LinkedListNode<Action> actionNode = list.First;
                                    while (actionNode != null)
                                    {
                                        if (actionNode.Value != null)
                                        {
                                            actionsScheduleToRun.AddLast(actionNode.Value);
                                        }
                                        LinkedListNode<Action> next = actionNode.Next;
                                        actionNode = next;
                                    }
                                }
                                // remove from now
                                tasks.Remove(dtBeforeNow);
                                DateTime nextPos = dtBeforeNow.AddDays(1);
                                // add to next day
                                tasks[nextPos] = list;
                            }
                        }
                        Thread.Sleep(Period * 1000);
                    }
                    catch(Exception ee)
                    {
                        Tracer.D(ee.ToString());
                    }
                }
            }
            public void Start()
            {
                Stop();
                AsyncTask.QueueWorkingItem(Runner);
                AsyncTask.QueueWorkingItem(Scheduler);
            }
            public void Stop()
            {
                if (cancellable != null)
                {
                    cancellable.Cancel();
                }
                cancellable = new Coroutine.Cancellable();
            }
        }
    }
}
