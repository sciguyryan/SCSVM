using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class MOV_REG_MEM : Instruction
    {
        public override Type[] ArgumentTypes => 
            new[] { typeof(Registers), typeof(int) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, null };

        public override OpCode OpCode => 
            OpCode.MOV_REG_MEM;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            var bytes = 
                BitConverter.GetBytes(cpu.Registers[(Registers)data[0]]);
            cpu.VM.Memory
                .SetValueRange((int)data[1], bytes,
                               GetSecurityContext());

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var fromReg = (Registers)data[0];
            var memoryAddr = (int)data[1];

            // mov R1, $MEMORY ADDR
            return $"{AsmName} {fromReg}, ${memoryAddr:X}";
        }
    }
}