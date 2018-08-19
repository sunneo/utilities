using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities
{
    public class Locked<T>
    {
        Locker mLocker = new Locker();
        T mValue;
        public T Value
        {
            get
            {
                return mLocker.Synchronized(()=>
                {
                    return mValue;
                });
            }
            set
            {
                mLocker.Synchronized(() =>
                {
                    mValue = value;
                });
            }
        }
        public static implicit operator Locked<T>(T t)
        {
            return new Locked<T>(t);
        }
        public static implicit operator T(Locked<T> t)
        {
            return t.Value;
        }
        
        public Locked(T val)
        {
            Value = val;
        }
    }
}
