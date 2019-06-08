/*
* Copyright (c) 2019-2020 [Open Source Developer, Sunneo].
* All rights reserved.
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the [Open Source Developer, Sunneo] nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE [Open Source Developer, Sunneo] AND CONTRIBUTORS "AS IS" AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE [Open Source Developer, Sunneo] AND CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Virtual.interfaces;

namespace Utilities.Virtual
{
    /// <summary>
    /// VMlike finite machine implementation
    /// it can also be achieved by IEnumerator in C#.
    /// </summary>
    public class VMFiniteStateMachine : IVMFiniteStateMachine
    {
        long IFCount = 0;
        FSMContext m_FsmContext = new FSMContext();
        List<IR> Instructions = new List<IR>();
        private bool m_Terminated = false;
        Dictionary<String, int> mLabelMap = new Dictionary<string, int>();

        public event EventHandler Finished;

        internal class VMInstructionFactory:IInstructionFactory
        {
            public VMFiniteStateMachine Parent;
            public VMInstructionFactory(VMFiniteStateMachine mParent)
            {
                this.Parent = mParent;
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
                    if (Parent.LabelMap.ContainsKey(label))
                    {
                        fsm.IP = Parent.LabelMap[label];
                    }
                };
            }

            public Action<FSMContext> NOP
            {
                get
                {
                    return new Action<FSMContext>((fsm) => { });
                }
            }
        }

        public bool Terminated
        {
            get { return m_Terminated;  }
            set { m_Terminated = value; }
        }

        public Dictionary<String, int> LabelMap
        {
            get
            {
                return mLabelMap;
            }           
        }
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
                if ((bool)this.m_FsmContext.GeneralReg[IFLabel])
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
            this.AddInstruction(label, this.InstructionFactory.NOP);
        }

        public void AddGoto(int ip)
        {
            if (ip < Instructions.Count)
            {
                this.AddInstruction(mInstructionFactory.GOTO(ip));
            }
        }
        public void AddGoto(String label)
        {
            this.AddInstruction(mInstructionFactory.GOTO(label));
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
                if (!(bool)this.m_FsmContext.GeneralReg[TESTLabel])
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
            this.AddIF(new Func<FSMContext, bool>((fsm) => { return fsm.STATUS.Equals(status); }), _body, this.InstructionFactory.NOP);
        }

        private void LaunchIR(IR ir)
        {
            ir.Finished = false;
            ir.Instruction(this.m_FsmContext);
            ir.Finished = true;
        }
        public bool Advance()
        {
            if (Terminated) return false;
            if (m_FsmContext.IP < Instructions.Count)
            {
                int ip = m_FsmContext.IP;
                var action = Instructions[ip];
                // move to next
                ++m_FsmContext.IP;
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

        IInstructionFactory mInstructionFactory;
        public IInstructionFactory InstructionFactory
        {
            get 
            {
                if (mInstructionFactory == null)
                {
                    mInstructionFactory = new VMInstructionFactory(this);
                }
                return mInstructionFactory;
            }
        }
    }
}
