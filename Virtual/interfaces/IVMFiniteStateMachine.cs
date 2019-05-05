using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Virtual.interfaces
{
    public interface IVMFiniteStateMachine
    {
        bool Terminated { get; set; }
        event EventHandler Finished;
        Dictionary<String, int> LabelMap { get; }
        void AddInstruction(Action<FSMContext> ins);
        void AddInstruction(String label, Action<FSMContext> ins);
        void AddIF(Func<FSMContext, bool> testCondition, Action<FSMContext> _then, Action<FSMContext> _else);
        void AddLabel(String label);
        void AddGoto(int ip);
        void AddGoto(String label);
        void AddWhile(Func<FSMContext, bool> testCondition, Action<FSMContext> _body);
        void AddSetStatus(String status);
        void AddIfStatus(String status, Action<FSMContext> _body);
        IInstructionFactory InstructionFactory{get;}
        bool Advance();
    }
}
