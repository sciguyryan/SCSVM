using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class MOV_MEM_REG : Instruction
    {
        public override Type[] ArgumentTypes => 
            new[] { typeof(int), typeof(Registers) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, null };

        public override OpCode OpCode => 
            OpCode.MOV_MEM_REG;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            cpu.Registers[(Registers)data[1]] = 
                cpu.VM.Memory
                .GetValueAsType<int>((int)data[0],
                                     GetSecurityContext());

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var memoryAddr = (int)data[0];
            var toReg = (Registers)data[1];

            // mov [$MEMORY ADDR], R1
            return $"{AsmName} [${memoryAddr:X}], {toReg}";
        }
    }
}