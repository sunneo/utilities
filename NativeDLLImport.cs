using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{

    public class NativeDLLImport:IDisposable
    {
        [DllImport("kernel32.dll")]
        internal static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        internal static extern bool FreeLibrary(IntPtr hModule);
        IntPtr Library;
        public String FilePath;

        public NativeDLLImport(String FilePath)
        {
            this.FilePath = FilePath;
            Init();
        }

        private void Init()
        {
            if (String.IsNullOrEmpty(FilePath) || !File.Exists(FilePath)) return;
            this.Library = LoadLibrary(FilePath);
        }

        /// <summary>
        /// Try Get Function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="FunctionName"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public bool TryGetFunction<T>(String FunctionName,out T ret) where T:class
        {
            ret = default(T);
            if (this.Library == IntPtr.Zero) return false;
            IntPtr ptr = GetProcAddress(this.Library, FunctionName);
            if(ptr == IntPtr.Zero) return false;
            ret = Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
            return true;
        }

        public void Dispose()
        {
            if (Library != IntPtr.Zero)
            {
                FreeLibrary(Library);
                Library = IntPtr.Zero;
            }
        }

    }

}
