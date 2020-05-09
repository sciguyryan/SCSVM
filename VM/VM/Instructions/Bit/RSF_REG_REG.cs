using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class RSF_REG_REG : Instruction
    {
        public override Type[] ArgumentTypes => 
            new [] { typeof(Registers), typeof(Registers) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, null };
        public override OpCode OpCode => 
            OpCode.RSF_REG_REG;

        public override string AsmName => "rsf";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            cpu.Registers[(Registers)data[0]] >>= 
                cpu.Registers[(Registers)data[1]];

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var fromReg1 = (Registers)data[0];
            var fromReg2 = (Registers)data[1];

            // rsf R1, R2
            return $"{AsmName} {fromReg1}, {fromReg2}";
        }
    }
}