/*
* Copyright (c) 2019-2020 [Open Source Developer, Sunneo].
* All rights reserved.
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the [Open Source Developer, Sunneo] nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE [Open Source Developer, Sunneo] AND CONTRIBUTORS "AS IS" AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE [Open Source Developer, Sunneo] AND CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{


    /**
	 *  Usage:
	 *  In C
	 *  <pre>
	 *  extern "C" __declspec(dllexport) int sum(double a, double b)
	 *  {
	 *      return a+b;   
	 *  }
	 *  extern "C" __declspec(dllexport) int sum(double a, double b)
	 *  {
	 *      return sqrt(v);
	 *  }
	 *  </pre>
     *  extern "C"  __declspec(dllexport) double __stdcall dlsqrt(double v);
	 *  In C#
	 *  <pre>
	 *  
	 *  static class Program
     *   {
     *      [UnmanagedFunctionPointer(CallingConvention.Cdecl)]    delegate int sumfnc(double a, double b);
     *      [UnmanagedFunctionPointer(CallingConvention.StdCall)]  delegate double dlsqrtfnc(double v);
     *      public static void Main(String[] argv)
     *      {
     *          NativeDLLImport dlimport = new NativeDLLImport("DLLTest.dll");
     *          sumfnc sum = null;
     *          dlsqrtfnc dlsqrt = null;
     *          dlimport.TryGetFunction("sum", out sum);
     *          dlimport.TryGetFunction("_dlsqrt@8", out dlsqrt);
     *          Console.WriteLine("call native sum({0},{1})={2}", 1, 2, sum(1, 2));
     *          Console.WriteLine("call native dlsqrt({0})={1}", 100, dlsqrt(100));
     *          Console.ReadKey();
     *      }
     *  }
	 *  <pre>
	 */
    public class NativeDLLImport:IDisposable
    {
        bool HasLoad = false;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32.dll")]
        internal static extern int FormatMessage(int flag, ref IntPtr source, int msgid, int langid, ref string buf, int size, ref IntPtr args);

        public static string GetSysErrMsg(int errCode)
        {
            IntPtr tempptr = IntPtr.Zero;
            string msg = null;
            FormatMessage(0x1300, ref tempptr, errCode, 0, ref msg, 255, ref tempptr);
            return msg;
        }

        IntPtr Library;
        public String FilePath;

        public event EventHandler<String> OnError;


        public NativeDLLImport(String FilePath)
        {
            this.FilePath = FilePath;
        }

        private void Init()
        {
            if (!HasLoad)
            {
                HasLoad = true;
                if (String.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
                {
                    try
                    {
                        if (OnError != null)
                        {
                            OnError(this, "Load Library `" + this.FilePath + "` Failed... No Such File");
                        }
                    }
                    catch (Exception ee)
                    {
                        Tracer.D(ee.ToString());
                    }               
                    return;
                }
                this.Library = LoadLibrary(FilePath);
                if (this.Library == IntPtr.Zero)
                {
                    try
                    {
                        if (OnError != null)
                        {
                            OnError(this, GetSysErrMsg(Marshal.GetLastWin32Error()));
                        }
                    }
                    catch (Exception ee)
                    {
                        Tracer.D(ee.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Try Get Function
        /// this trick can make compiler deriving data type of delegate 
        /// </summary>
        /// <typeparam name="T">function type</typeparam>
        /// <param name="FunctionName">function name</param>
        /// <param name="ret">boolean value represent whether operation success</param>
        /// <returns></returns>
        public bool TryGetFunction<T>(String FunctionName,out T ret) where T:class
        {
            ret = default(T);
            Init();
            if (this.Library == IntPtr.Zero)
            {
                try
                {
                    if (OnError != null)
                    {
                        OnError(this, "Unable To GetProcAddress for `" + FunctionName + " ... DLL Load Failed`");
                    }
                }
                catch (Exception ee)
                {
                    Tracer.D(ee.ToString());
                }
                
                return false;
            }
            IntPtr ptr = GetProcAddress(this.Library, FunctionName);
            if (ptr == IntPtr.Zero)
            {
                try
                {
                    if (OnError != null)
                    {
                        OnError(this, "Unable To GetProcAddress for `" + FunctionName + "`..." + GetSysErrMsg(Marshal.GetLastWin32Error()));
                    }
                }
                catch (Exception ee)
                {
                    Tracer.D(ee.ToString());
                }
                return false;
            }
            ret = Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
            if (ret == null)
            {
                try
                {
                    if (OnError != null)
                    {
                        OnError(this, "Unable To GetProcAddress for " + FunctionName + " ...Type Not Match");
                    }
                }
                catch (Exception ee)
                {
                    Tracer.D(ee.ToString());
                }
                
            }
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
