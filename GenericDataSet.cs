using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class GenericDataSet : SequentialDictionary<String, Object>
    {
        public GenericDataSet()
        {

        }
        public static GenericDataSet FromKeyValues(params Object[] objects)
        {
            return new GenericDataSet(objects);
        }
        public GenericDataSet(params Object[] objects)
        {
            LoadKeyValues(objects);
        }
        public virtual GenericDataSet LoadKeyValues(params Object[] objects)
        {
            for (int i = 0; i < objects.Length; i += 2)
            {
                try
                {
                    object key = objects[i];
                    object val = objects[i + 1];
                    if (!(key is String))
                    {
                        Set((String)key, val);
                    }
                }
                catch (Exception ee)
                {

                }
            }
            return this;
        }

        public void Set(String key,Object val)
        {
            this[key] = val;
        }
        public T Get<T>(String key)
        {
            if (!ContainsKey(key)) return default(T);
            return (T)this[key];
        }
        public bool IsEmpty()
        {
            return Count == 0;
        }
    }
}
