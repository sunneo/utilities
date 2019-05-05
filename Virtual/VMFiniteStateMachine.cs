using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Virtual
{
    public class VMFiniteStateMachine
    {
        public long IFCount = 0;
        FSMContext fsmContext = new FSMContext();
        public bool Terminated = false;
        public event EventHandler Finished;
        public class FSMContext
        {
            public int IP;
            public String STATUS;
            public Dictionary<String, object> GeneralReg = new Dictionary<string, object>();
        }

        public Dictionary<String, int> LabelMap = new Dictionary<string, int>();
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
        }
        List<IR> Instructions = new List<IR>();
        public void AddInstruction(Action<FSMContext> ins)
        {
            Instructions.Add(ins);
        }
        public void AddInstruction(String label, Action<FSMContext> ins)
        {
            LabelMap[label] = Instructions.Count;
            AddInstruction(ins);
        }
        public void AddIF(Func<FSMContext, bool> testCondition, Action<FSMContext> _then, Action<FSMContext> _else)
        {
            long ifCounter = IFCount++;
            String IFLabel = "IF" + ifCounter.ToString();
            String THENLabel = IFLabel + "THEN";
            String ELSELabel = IFLabel + "ELSE";
            // test
            // goto
            //  then
            //  else

            Action<FSMContext> test = (fsm) =>
            {
                fsm.GeneralReg[IFLabel] = testCondition(fsm);
            };
            Action<FSMContext> gotoResult = (fsm) =>
            {
                if ((bool)this.fsmContext.GeneralReg[IFLabel])
                {
                    fsm.IP = LabelMap[THENLabel];
                }
                else
                {
                    fsm.IP = LabelMap[ELSELabel];
                }
            };
            AddInstruction(test);
            AddInstruction(gotoResult);
            AddInstruction(THENLabel, _then);
            AddInstruction(ELSELabel, _else);
        }
        public void AddLabel(String label)
        {
            this.AddInstruction(label, NOP);
        }
        public Action<FSMContext> GOTO(int ip)
        {
            return (fsm) =>
            {
                fsm.IP = ip;
            };
        }
        public Action<FSMContext> GOTO(String label)
        {
            return (fsm) =>
            {
                if (LabelMap.ContainsKey(label))
                {
                    fsm.IP = LabelMap[label];
                }
            };
        }
        public void AddGoto(int ip)
        {
            if (ip < Instructions.Count)
            {
                this.AddInstruction((fsm) =>
                {
                    fsm.IP = ip;
                });
            }
        }
        public void AddGoto(String label)
        {
            if (LabelMap.ContainsKey(label))
            {
                AddGoto(LabelMap[label]);
            }
        }
        public void AddWhile(Func<FSMContext, bool> testCondition, Action<FSMContext> _body)
        {
            long ifCounter = IFCount++;
            String TESTLabel = "WHILE" + ifCounter.ToString();
            String TESTFailLabel = "WHILE" + ifCounter.ToString() + "_Fail";
            // test
            // goto
            //  then
            //  else

            Action<FSMContext> test = (fsm) =>
            {
                fsm.GeneralReg[TESTLabel] = testCondition(fsm);
            };
            Action<FSMContext> gotoOutResult = (fsm) =>
            {
                if (!(bool)this.fsmContext.GeneralReg[TESTLabel])
                {
                    fsm.IP = LabelMap[TESTFailLabel];
                }
            };
            Action<FSMContext> gotoBack = (fsm) =>
            {
                fsm.IP = LabelMap[TESTLabel];
            };
            Action<FSMContext> onFail = (fsm) =>
            {
                // nop
            };
            AddInstruction(TESTLabel, test);
            AddInstruction(gotoOutResult);
            AddInstruction(_body);
            AddInstruction(gotoBack);
            AddInstruction(TESTFailLabel, onFail);
        }
        public void AddSetStatus(String status)
        {
            AddInstruction(new Action<FSMContext>((fsm) =>
            {
                fsm.STATUS = status;
            }));
        }
        public void AddIfStatus(String status, Action<FSMContext> _body)
        {
            this.AddIF(new Func<FSMContext, bool>((fsm) => { return fsm.STATUS.Equals(status); }), _body, NOP);
        }
        public Action<FSMContext> NOP
        {
            get
            {
                return new Action<FSMContext>((fsm) => { });
            }
        }
        private void LaunchIR(IR ir)
        {
            ir.Finished = false;
            ir.Instruction(this.fsmContext);
            ir.Finished = true;
        }
        public bool Advance()
        {
            if (Terminated) return false;
            if (fsmContext.IP < Instructions.Count)
            {
                int ip = fsmContext.IP;
                var action = Instructions[ip];
                // move to next
                ++fsmContext.IP;
                LaunchIR(action);
                return true;
            }
            else
            {
                Terminated = true;
                if (Finished != null)
                {
                    Finished(this, EventArgs.Empty);
                }
                return false;
            }
        }
    }
}
