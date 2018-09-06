using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;

namespace Utilities
{
    public class Tracer
    {
        static object Locker = new object();

        public static void D(String fmt)
        {
            lock (Locker)
            {
                String nowDate = DateTime.Now.ToString("yyyyMMdd");
                String now = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]";
                String path = Path.Combine("報告匯集", nowDate);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                String file = Path.Combine(path, "XQOP錯誤報告.txt");
                File.AppendAllText(file, now + fmt + Environment.NewLine);
            }
        }
    }
}
