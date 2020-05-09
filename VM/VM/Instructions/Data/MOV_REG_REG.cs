using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class MOV_REG_PTR_REG : Instruction
    {
        public override Type[] ArgumentTypes => 
            new[] { typeof(Registers), typeof(Registers) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, null };

        public override OpCode OpCode => 
            OpCode.MOV_REG_PTR_REG;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            cpu.Registers[(Registers)data[1]] = 
                cpu.VM.Memory
                .GetValueAsType<int>(cpu.Registers[(Registers)data[0]], 
                                     GetSecurityContext());

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var fromReg = (Registers)data[0];
            var toReg = (Registers)data[1];

            // mov *R1, R2
            return $"{AsmName} *{fromReg}, {toReg}";
        }
    }
}