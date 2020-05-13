using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class MOV_LIT_MEM
        : Instruction
    {
        public override Type[] ArgumentTypes =>
            new Type[]
            {
                typeof(int),
                typeof(int)
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode => 
            OpCode.MOV_LIT_MEM;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData aData, CPU aCpu)
        {
            var bytes =
                BitConverter.GetBytes((int)aData[0]);

            // We do not care if this writes to executable
            // memory.
            aCpu.VM.Memory
                .SetValueRange((int)aData[1],
                               bytes,
                               GetSecurityContext(),
                               false);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var literal = (int)aData[0];
            var toAddr = (int)aData[1];

            // mov $LITERAL, [$MEMORY ADDR]
            return $"{AsmName} ${literal:X}, [${toAddr:X}]";
        }
    }
}
