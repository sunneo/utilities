using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Virtual
{
    public class IR
    {
        public System.Action<FSMContext> Instruction;
        public bool Finished;
        public IR(System.Action<FSMContext> ins)
        {
            this.Instruction = ins;
        }
        public static implicit operator IR(System.Action<FSMContext> ins)
        {
            return new IR(ins);
        }
        public static implicit operator System.Action<FSMContext>(IR ins)
        {
            return ins.Instruction;
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
