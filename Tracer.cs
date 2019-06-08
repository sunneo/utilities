using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Utilities;

namespace Utilities
{
    public class Tracer
    {
        public static bool Enabled = true;
        public static String TracerPath = "Logs";
        static object Locker = new object();

        public static void D(String message, [CallerMemberName] string memberName = "",
              [CallerFilePath] string sourceFilePath = "",
              [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!Enabled) return;
            lock (Locker)
            {
                String nowDate = DateTime.Now.ToString("yyyyMMdd_HH")+"0000";
                String now = DateTime.Now.ToString("HH:mm:ss");
                String path = TracerPath;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                String file = Path.Combine(path, "Log_"+nowDate+".txt");
                File.AppendAllText(file, String.Format("{0} {1} ... {2} ({3} line {4})",now,message,memberName,sourceFilePath,sourceLineNumber)+Environment.NewLine);
            }
        }
    }
}
