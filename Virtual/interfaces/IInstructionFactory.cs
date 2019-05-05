using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Virtual.interfaces
{
    public interface IInstructionFactory
    {
        Action<FSMContext> GOTO(int ip);
        Action<FSMContext> GOTO(String label);
        Action<FSMContext> NOP { get; }
    }
}
