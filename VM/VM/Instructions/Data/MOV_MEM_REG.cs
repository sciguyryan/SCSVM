using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class MOV_MEM_REG
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[]
            {
                typeof(int),
                typeof(Registers)
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode => 
            OpCode.MOV_MEM_REG;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData aData, CPU aCpu)
        {
            aCpu.Registers[(Registers)aData[1]] = 
                aCpu.VM.Memory
                .GetValueAsType<int>((int)aData[0], GetSecurityContext());

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var memoryAddr = (int)aData[0];
            var toReg = (Registers)aData[1];

            // mov [$MEMORY ADDR], R1
            return $"{AsmName} [${memoryAddr:X}], {toReg}";
        }
    }
}
