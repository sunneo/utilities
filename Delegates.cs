using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Delegates
    {
        public interface IEnumerableEx<T> : IEnumerable<T>
        {
            IEnumerableEx<T> Filter(Func<T, bool> fnc);
            IEnumerableEx<T2> Translate<T2>(Func<T, T2> translate);
        }
        public interface IEnumeratorEx<T> : IEnumerator<T>
        {
            IEnumeratorEx<T> Filter(Func<T, bool> fnc);
            IEnumeratorEx<T2> Translate<T2>(Func<T, T2> translate);
        }
        public static IEnumerableEx<T> ForAll<T>(IEnumerable<T> iter)
        {
            return new IEnumerableExImpl<T>(iter);
        }
        public static IEnumerableEx<T> ForAll<T>(IEnumerator<T> iter)
        {
            return new IEnumerableExImpl<T>(new IEnumerableEnumerator<T>(iter));
        }
    }
    /// <summary>
    /// translate iterator to iterable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class IEnumerableEnumerator<T>:IEnumerable<T>
    {
        IEnumerator<T> instance;
        public IEnumerableEnumerator(IEnumerator<T> that)
        {
            instance = that;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return instance;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return instance;
        }
    }
    class IEnumerableExImpl<T> : Delegates.IEnumerableEx<T>
    {
        IEnumerable<T> instance;
        public IEnumerableExImpl(IEnumerable<T> that)
        {
            this.instance = that;
        }
        protected virtual IEnumerable<T> FilterImpl(Func<T, bool> fnc)
        {
            IEnumerator<T> iter= instance.GetEnumerator();
            if(fnc == null)
            {
                fnc = (x) => true;
            }
            while(iter != null && iter.MoveNext())
            {
                T val = iter.Current;
                if(fnc(val))
                {
                    yield return val;
                }
            }
            yield break;
        }
        protected virtual IEnumerable<T2> TranslateImpl<T2>(Func<T, T2> fnc)
        {
            IEnumerator<T> iter = instance.GetEnumerator();
            if (fnc == null)
            {
                fnc = (T x) => default(T2);
            }
            while (iter != null && iter.MoveNext())
            {
                T val = iter.Current;
                T2 ret = fnc(val);
                yield return ret;
            }
            yield break;
        }
        public Delegates.IEnumerableEx<T2> Translate<T2>(Func<T, T2> translate)
        {
            return new IEnumerableExImpl<T2>(TranslateImpl(translate));
        }
        public Delegates.IEnumerableEx<T> Filter(Func<T, bool> fnc)
        {
            return new IEnumerableExImpl<T>(FilterImpl(fnc));
        }
        
        public IEnumerator<T> GetEnumerator()
        {
            return instance.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return instance.GetEnumerator();
        }
    }

    class IEnumeratorExImpl<T> : Delegates.IEnumeratorEx<T>
    {
        IEnumerator<T> instance;

        public T Current => throw new NotImplementedException();

        object IEnumerator.Current => throw new NotImplementedException();

        public IEnumeratorExImpl(IEnumerator<T> that)
        {
            this.instance = that;
        }
        protected virtual IEnumerator<T> FilterImpl(Func<T, bool> fnc)
        {
            IEnumerator<T> iter = instance;
            if (fnc == null)
            {
                fnc = (x) => true;
            }
            while (iter!=null && iter.MoveNext())
            {
                T val = iter.Current;
                if (fnc(val))
                {
                    yield return val;
                }
            }
            yield break;
        }
        protected virtual IEnumerator<T2> TranslateImpl<T2>(Func<T, T2> fnc)
        {
            IEnumerator<T> iter = instance;
            if (fnc == null)
            {
                fnc = (T x) => default(T2);
            }
            while (iter != null && iter.MoveNext())
            {
                T val = iter.Current;
                T2 ret = fnc(val);
                yield return ret;
            }
            yield break;
        }
        public Delegates.IEnumeratorEx<T2> Translate<T2>(Func<T, T2> translate)
        {
            return new IEnumeratorExImpl<T2>(TranslateImpl(translate));
        }
        public Delegates.IEnumeratorEx<T> Filter(Func<T, bool> fnc)
        {
            return new IEnumeratorExImpl<T>(FilterImpl(fnc));
        }

        public void Dispose()
        {
            if (instance == null)
            {
                return;
            }
            instance.Dispose();
        }

        public bool MoveNext()
        {
            if(instance == null)
            {
                return false;
            }
            return instance.MoveNext();
        }

        public void Reset()
        {
            if(instance == null)
            {
                return;
            }
            instance.Reset();
        }
    }
}
