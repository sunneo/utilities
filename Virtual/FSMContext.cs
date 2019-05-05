using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Virtual
{
    public class FSMContext
    {
        public int IP;
        public String STATUS;
        public Dictionary<String, object> GeneralReg = new Dictionary<string, object>();
    }

}
