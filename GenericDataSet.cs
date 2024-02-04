using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class GenericDataSet : SequentialDictionary<String, Object>
    {
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
