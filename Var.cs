using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class BaseVar
    {
        public class ChainActionArgs
        {
            public BaseVar pthis;
            public BaseVar[] others;
        }
        public event EventHandler BaseValueChanged;
        public bool HasValue
        {
            get;
            internal set;
        }
        public bool Changed
        {
            get;
            internal set;
        }

        public BaseVar Chain(BaseVar pthis, Action<ChainActionArgs> action, params BaseVar[] others)
        {
            EventHandler handler = new EventHandler((s, e) =>
            {
                if (others.Length > 0)
                {
                    bool hasValue = true;
                    foreach (BaseVar other in others)
                    {
                        if (!other.HasValue)
                        {
                            hasValue = false;
                            break;
                        }
                    }
                    if (hasValue && action != null)
                    {
                        ChainActionArgs args = new ChainActionArgs();
                        args.pthis = pthis;
                        args.others = others;
                        action(args);
                    }
                }
            });
            foreach (BaseVar other in others)
            {
                other.BaseValueChanged += handler;
            }
            return pthis;
        } 
        protected virtual void NotifyBaseValueChanged()
        {
            if (BaseValueChanged != null)
            {
                BaseValueChanged(this, EventArgs.Empty);
            }
        }
    }
    public class Var<T>:BaseVar
    {
        T m_Value = default(T);
        public event EventHandler<T> ValueChanged;
        public bool ValueChangeTriggerOnlyOnChange = false;

        protected virtual void NotifyValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, m_Value);
            }
            NotifyBaseValueChanged();
        }
       
        public T Value
        {
            get
            {
                Changed = false;
                return m_Value;
            }
            set
            {
                T origVal = m_Value;
                m_Value = value;
                Changed = true;
                HasValue = true;
                if (ValueChangeTriggerOnlyOnChange)
                {
                    if (!m_Value.Equals(origVal))
                    {
                        NotifyValueChanged();
                    }
                }
                else
                {
                    NotifyValueChanged();
                }
            }
        }
        public Var()
        {
        }
        public Var(T val)
        {
            this.Value = val;
            HasValue = true;
        }
        public static implicit operator Var<T>(T t)
        {
            return new Var<T>(t);
        }
        public static implicit operator T(Var<T> t)
        {
            return t.Value;
        }
        public override string ToString()
        {
            return Value.ToString();
        }
       
    }
}
