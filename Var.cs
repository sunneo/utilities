using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Var<T>
    {
        Locked<T> m_Value = new Locked<T>(default(T));
        public event EventHandler<T> ValueChanged;
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
                return m_Value.Value;
            }
            set
            {
                m_Value.Value = value;
                NotifyValueChanged();
            }
        }
        public Var()
        {
        }
        public Var(T val)
        {
            this.Value = val;
        }
        public static implicit operator Var<T>(T t)
        {
            return new Var<T>(t);
        }
        public static implicit operator T(Var<T> t)
        {
            return t.Value;
        }
        
       
    }
}
