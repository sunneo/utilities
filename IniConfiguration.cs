using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class IniConfiguration<T>
    {

        public T Data;
        String FileName;
        FileSystemWatcher mWatcher = null;
        private IniConfiguration(String filename, bool alwaysUpdate)
        {
            this.FileName = filename;
            if (alwaysUpdate)
            {
                String extension = Path.GetExtension(filename);
                mWatcher = new FileSystemWatcher();
                mWatcher.BeginInit();
                mWatcher.Path = Path.GetDirectoryName(filename);
                mWatcher.InternalBufferSize = 64;
                mWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime;
                if (!String.IsNullOrEmpty(extension))
                {
                    mWatcher.Filter = "*"+extension;
                }
                mWatcher.EnableRaisingEvents = true;
                mWatcher.Changed += mFileChangedHandler;
                mWatcher.Created += mFileChangedHandler;
                mWatcher.EndInit();
            }
        }

        void mFileChangedHandler(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(FileName))
                {
                    if (e.Name.Equals(Path.GetFileName(FileName)))
                    {
                        this.Data = IniReader.Deserialize<T>(FileName);
                    }
                }
            }
            catch (Exception ee)
            {

            }
        }
        public static IniConfiguration<T> FromFile(String filename,bool alwaysUpdate=false)
        {
            IniConfiguration<T> ret = new IniConfiguration<T>(filename, alwaysUpdate);
            ret.Data = IniReader.Deserialize<T>(filename);
            return ret;
        }
    }
}
