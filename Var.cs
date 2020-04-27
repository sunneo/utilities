using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.OptionParser.OptionParse;

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
            set;
        }
        public bool Changed
        {
            get;
            internal set;
        }
        /// <summary>
        /// chain on one of member happends data change
        /// </summary>
        /// <param name="pthis"></param>
        /// <param name="action"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        public static BaseVar ChainOr(BaseVar pthis, Action<ChainActionArgs> action, params BaseVar[] others)
        {
            EventHandler handler = new EventHandler((s, e) =>
            {
                if (action != null)
                {
                    ChainActionArgs args = new ChainActionArgs();
                    args.pthis = pthis;
                    args.others = others;
                    action(args);
                }
            });
            foreach (BaseVar other in others)
            {
                other.BaseValueChanged += handler;
            }
            return pthis;
        }
        /// <summary>
        /// chain on one of member happends data change ,and all data are available
        /// </summary>
        /// <param name="pthis"></param>
        /// <param name="action"></param>
        /// <param name="others"></param>
        /// <returns></returns>
        public static BaseVar Chain(BaseVar pthis, Action<ChainActionArgs> action, params BaseVar[] others)
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

    /// <summary>
    /// a var data type which listen to value change
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Var<T>:BaseVar
    {
        T m_Value = default(T);
        public event EventHandler<T> ValueChanged;
        public bool ValueChangeTriggerOnlyOnChange = false;
        public Func<T> Getter;
        public Action<T> Setter;
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
                if (Getter != null)
                {
                    return Getter();
                }
                Changed = false;
                return m_Value;
            }
            set
            {
                T origVal = m_Value;
                m_Value = value;
                Changed = true;
                HasValue = true;
                if (Setter != null)
                {
                    try
                    {
                        Setter(value);
                    }
                    catch (Exception ee)
                    {

                    }
                }
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

    /// <summary>
    /// pipe a string to a target Var type
    /// target type should implement TryParse 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VarStringPipe<T> : Var<String>
    {
        public Var<String> Source = new Var<string>();
        public Var<T> Target;
        protected override void NotifyValueChanged()
        {
            base.NotifyValueChanged();
            T value;
            if (DynamicTryParse<T>.TryParse(this.Value, out value))
            {
                Target.Value = value;
            }
        }
        /// <summary>
        /// sync this string value to target
        /// </summary>
        /// <param name="target"></param>
        public VarStringPipe(Var<T> target)
        {
            this.Target = target;
        }
        /// <summary>
        /// sync this string value to target
        /// </summary>
        /// <param name="target"></param>
        public VarStringPipe()
        {
            this.Target = new Var<T>();
        }
        /// <summary>
        /// sync a string to this and transform to target
        /// </summary>
        /// <param name="source">string source</param>
        /// <param name="target">transform target</param>
        public VarStringPipe(Var<String> source,Var<T> target)
        {
            this.Source = source;
            if (this.Source != null)
            {
                this.Source.ValueChanged += Source_ValueChanged;
            }
            this.Target = target;
        }

        void Source_ValueChanged(object sender, string e)
        {
            this.Value = e;
        }
    }

    public class NamedVarFactory
    {
        public List<BaseVar> Vars = new List<BaseVar>();
        public NamedVar<T> NewInstance<T>(String name)
        {
            NamedVar<T> ret = new NamedVar<T>(name);
            Vars.Add(ret);
            return ret;
        }
        public NamedVar<T> NewInstance<T>(String name, T val)
        {
            NamedVar<T> ret = new NamedVar<T>(name,val);
            Vars.Add(ret);
            return ret;
        }
    }
    public class NamedVar<T> : Var<T>
    {
        public String Name;
        public NamedVar(String name, T val)
            :base(val)
        {
            this.Name = name;
        }
        public NamedVar(String name)
            :base()
        {
        }
    }
}
