using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class INC_REG : Instruction
    {
        public override Type[] ArgumentTypes => 
            new [] { typeof(Registers) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null };

        public override OpCode OpCode => 
            OpCode.INC_REG;

        public override string AsmName => "inc";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            ++cpu.Registers[(Registers)data[0]];

            // We do not need to update the CPU flags
            // here as the result is not going
            // into the accumulator register.

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var fromReg = (Registers)data[0];

            // inc R1
            return $"{AsmName} {fromReg}";
        }
    }
}