using System;
using System.Collections.Generic;
using VMCore.VM;
using VMCore.VM.Core;

namespace VMCore.Assembler
{
    /// <summary>
    /// Quick and dirty instruction class for use with the quick compiler.
    /// WARNING:
    /// Do not use outside of this purpose as it contains
    /// none of the validation present in the other classes.
    /// </summary>
    public class QuickInstruction
    {
        public OpCode Op { get; private set; }

        public object[] Args { get; private set; }

        public AsmLabel Label { get; private set; }

        public QuickInstruction(OpCode opCode, object[] args = null, AsmLabel label = null)
        {
            Op = opCode;
            Args = args;
            Label = label;
        }

        public override string ToString()
        {
            var opIns = new InstructionData
            {
                OpCode = Op
            };

            for (var i = 0; i < Args.Length; i++)
            {
                var arg = new AsmInstructionArg
                {
                    Value = Args[i]
                };

                opIns.Args.Add(arg);
            }

            return ReflectionUtils.InstructionCache[Op].ToString(opIns);
        }
    }
}
