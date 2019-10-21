using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Var<T>
    {
        T m_Value = default(T);
        public event EventHandler<T> ValueChanged;
        public bool HasValue
        {
            get;
            private set;
        }
        public bool Changed
        {
            get;
            private set;
        }
        protected virtual void NotifyValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, Value);
            }
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
                m_Value = value;
                Changed = true;
                HasValue = true;
                NotifyValueChanged();
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
