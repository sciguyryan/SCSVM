using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class MOV_LIT_MEM : Instruction
    {
        public override Type[] ArgumentTypes =>
            new [] { typeof(int), typeof(int) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, null };

        public override OpCode OpCode => 
            OpCode.MOV_LIT_MEM;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            var bytes =
                BitConverter.GetBytes((int)data[0]);
            cpu.VM.Memory
                .SetValueRange((int)data[1], bytes,
                               GetSecurityContext());

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var literal = (int)data[0];
            var toAddr = (int)data[1];

            // mov $LITERAL, [$MEMORY ADDR]
            return $"{AsmName} ${literal:X}, [${toAddr:X}]";
        }
    }
}