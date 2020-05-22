using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class JLE_REG
        : Instruction
    {
        public override Type[] ArgumentTypes =>
            new Type[]
            {
                typeof(Registers),
                typeof(int)
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode =>
            OpCode.JLE_REG;

        public override string AsmName => "jle";

        public override bool CanBindToLabel(int aArgumentID)
        {
            return (aArgumentID == 1);
        }

        public override bool Execute(InstructionData aData, CPU aCpu)
        {
            if (aCpu.Registers[(Registers)aData[0]] <=
                aCpu.Registers[Registers.AC])
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
            var fromReg = (Registers)aData[0];
            var address = (int)aData[1];

            // jle R1, &ADDRESS
            return $"{AsmName} {fromReg}, &{address:X}";
        }
    }
}
