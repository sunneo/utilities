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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Coroutine.Waiter.Server;
using Utilities;
using Utilities.Coroutine.Waiter.Client;

namespace Utilities
{

    /// <summary>
    /// file based communication
    /// so application can communicates across processes without 
    /// necessary to establish sockets and namedpipe
    /// 
    /// sometimes file communication is much more stable than 
    /// network/pipe
    /// </summary>
    public class FileCommunicator : IDisposable
    {
        public static String GetWriteFileName()
        {
            return Process.GetCurrentProcess().Id.ToString() + "_t" + Thread.CurrentThread.ManagedThreadId.ToString() + ".out";
        }
        public class FileNameWaiter : IWaiter
        {
            public String ExpectedFileName;

            public override bool CanRemove(object value)
            {
                if (value is String)
                {
                    String sval = (String)value;
                    return (Path.GetFileName(sval).Equals(ExpectedFileName));
                }
                return base.CanRemove(value);
            }
        }
        #region Fields
        int mProcessId = -1;
        volatile bool mDisposed = false;
        FileSystemWatcher watcher = null;
        volatile bool mTextMode = true;
        String mOutputFolder = "";
        #endregion
        public WaiterHolder<FileNameWaiter> Waiter = new WaiterHolder<FileNameWaiter>();
        /// <summary>
        /// on text content inputed
        /// </summary>
        public event EventHandler<String> OnTextInputed;
        public event EventHandler<KeyValuePair<String, String>> OnTextInputedWithPath;
        /// <summary>
        /// on binary inputed
        /// (TextMode=false)
        /// </summary>
        public event EventHandler<Stream> OnBinaryDataInputed;
        Locker locker = new Locker();

        /// <summary>
        /// text mode
        /// false => binary mode
        /// when mode is text mode, events are triggered through OnTextInputed
        /// as TextMode=false, it will output by OnBinaryDataInputed
        /// </summary>
        public bool TextMode
        {
            get
            {
                return mTextMode;
            }
            set
            {
                mTextMode = value;
            }
        }
        public bool IsDisposed
        {
            get
            {
                return mDisposed;
            }
            private set
            {
                mDisposed = value;
            }
        }
        public int ProcessId
        {
            get
            {
                if (mProcessId == -1)
                {
                    mProcessId = Process.GetCurrentProcess().Id;
                }
                return mProcessId;
            }
        }

        /// <summary>
        /// additional output targets
        /// when path was given, it will output to additional targets as well
        /// </summary>
        public List<String> AdditionalOutput = new List<string>();
        #region implementations
        protected virtual void OnText(String fullPath)
        {
            if (OnTextInputed != null || OnTextInputedWithPath != null || Waiter.WaiterList.Count > 0)
            {
                for (int i = 0; i < 10; ++i)
                {
                    try
                    {
                        String txt = File.ReadAllText(fullPath);
                        if (OnTextInputedWithPath != null)
                        {
                            if (OnTextInputedWithPath != null)
                            {
                                OnTextInputedWithPath(this, new KeyValuePair<String, String>(txt, fullPath));
                                Waiter.NotifyAndRemove(txt, fullPath);
                            }
                        }
                        else
                        {
                            if (OnTextInputed != null)
                            {
                                OnTextInputed(this, txt);
                                Waiter.NotifyAndRemove(txt);
                            }
                        }

                        break;
                    }
                    catch (Exception ee)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
        }
        protected virtual void OnBinary(String fullPath)
        {
            if (OnBinaryDataInputed != null)
            {
                try
                {
                    using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        OnBinaryDataInputed(this, fs);
                    }
                }
                catch (Exception ee)
                {

                }
            }
        }
        public void WriteTo(String txt, String givenFolder = "", String filename = "")
        {
            if (String.IsNullOrEmpty(givenFolder))
            {
                givenFolder = OutputFolder;
            }
            if (String.IsNullOrEmpty(filename))
            {
                filename = GetWriteFileName();
            }
            if (String.IsNullOrEmpty(givenFolder)) return;
            try
            {
                if (!Directory.Exists(givenFolder))
                {
                    Directory.CreateDirectory(givenFolder);
                }

                File.WriteAllText(Path.Combine(givenFolder, filename), txt);
                for (int i = 0; i < AdditionalOutput.Count; ++i)
                {
                    File.WriteAllText(AdditionalOutput[i], txt);
                }
            }
            catch (Exception ee)
            {
            }
        }
        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                try
                {
                    if (File.Exists(e.FullPath) && File.GetAttributes(e.FullPath) != FileAttributes.Directory)
                    {
                        bool doIt = true;
                        if(!String.IsNullOrEmpty(inputFileName) && !e.FullPath.Equals(inputFileName))
                        {
                            doIt = false;
                        }
                        if (doIt)
                        {
                            if (mTextMode)
                            {
                                OnText(e.FullPath);
                            }
                            else
                            {
                                OnBinary(e.FullPath);
                            }
                            File.Delete(e.FullPath);
                        }
                    }
                }
                catch (Exception ee)
                {

                }
            }
        }
        String mInputFolder;
        String inputFileName;
        public void SetInputFolder(String mFolder, bool isFile=false)
        {
           
            inputFileName = "";
            if(isFile || File.Exists(mFolder) && !File.GetAttributes(mFolder).HasFlag(FileAttributes.Directory))
            {
                inputFileName = mFolder;
                mFolder = Path.GetDirectoryName(mFolder);
            }
            mInputFolder = mFolder;
            if (!Directory.Exists(mInputFolder))
            {
                Directory.CreateDirectory(mInputFolder);
            }
        }

        public void SetOutputFolder(String mFolder)
        {
            mOutputFolder = mFolder;
            if (!Directory.Exists(mOutputFolder))
            {
                Directory.CreateDirectory(mOutputFolder);
            }
        }
        public Func<String> InputFolderDelegator;
        /// <summary>
        /// input folder provider
        /// </summary>
        /// <returns></returns>
        public virtual String GetInputFolder()
        {
            if (!String.IsNullOrEmpty(mInputFolder)) return mInputFolder;
            String folder = Environment.CurrentDirectory;
            if (InputFolderDelegator != null)
            {
                folder = InputFolderDelegator();
            }
            String path = Path.Combine(folder, ProcessId.ToString() + ".in");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
        #endregion
        /// <summary>
        /// output desination
        /// set empty to mute its output
        /// </summary>
        public String OutputFolder
        {
            get
            {
                if (String.IsNullOrEmpty(mOutputFolder))
                {
                    String path = Path.Combine(Environment.CurrentDirectory, ProcessId.ToString() + ".out");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    mOutputFolder = path;
                }
                return mOutputFolder;
            }
            set
            {
                mOutputFolder = value;
            }
        }

        #region Write functions
        public void Write(String value)
        {
            WriteTo(value);
        }
        public void Write(bool value)
        {
            WriteTo(value.ToString());
        }
        public void Write(int value)
        {
            WriteTo(value.ToString());
        }
        public void Write(double value)
        {
            WriteTo(value.ToString());
        }
        public void Write(String fmt, object param)
        {
            WriteTo(String.Format(fmt, param));
        }
        public void Write(String fmt, object param1, object param2)
        {
            WriteTo(String.Format(fmt, param1, param2));
        }
        public void Write(String fmt, params object[] param)
        {
            WriteTo(String.Format(fmt, param));
        }
        #endregion


        public void Start()
        {
            if (watcher == null)
            {
                watcher = new FileSystemWatcher(GetInputFolder());
                watcher.BeginInit();
                watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.LastWrite;
                watcher.Changed += watcher_Changed;
                watcher.Created += watcher_Changed;
                watcher.EnableRaisingEvents = true;
                watcher.EndInit();
            }
        }


        public void Stop()
        {
            if (watcher != null)
            {
                watcher.Dispose();
                watcher = null;
            }
        }
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            try
            {
                Stop();
            }
            catch (Exception ee)
            {

            }
        }
        public FileCommunicator()
        {

        }
        public void PipeOutputTo(FileCommunicator that)
        {
            this.OutputFolder = that.GetInputFolder();
        }
    }
}
