using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class JLT_LIT
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
            OpCode.JLT_LIT;

        public override string AsmName => "jlt";

        public override bool CanBindToLabel(int aArgumentID)
        {
            return (aArgumentID == 1);
        }

        public override bool Execute(InstructionData aData, CPU aCpu)
        {
            if ((int)aData[0] < aCpu.Registers[Registers.AC])
            {
                // Offset the address by current base size of the memory.
                // This is the bound of the memory outside of the 
                // executable memory region (e.g. main memory and stack).
                aCpu.Registers[Registers.IP] =
                    aCpu.VM.Memory.BaseMemorySize + (int)aData[1];
            }

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var literal = (int)aData[0];
            var address = (int)aData[1];

            // jlt $LITERAL, $ADDRESS
            return $"{AsmName} ${literal:X}, ${address:X}";
        }
    }
}
