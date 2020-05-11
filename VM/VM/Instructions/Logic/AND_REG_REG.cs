using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class AND_REG_REG
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[]
            {
                typeof(Registers),
                typeof(Registers)
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode => 
            OpCode.AND_REG_REG;

        public override string AsmName => "and";

        public override bool Execute(InstructionData aData, CPU aCpu)
        {
            var result = 
                aCpu.Registers[(Registers)aData[0]] & 
                aCpu.Registers[(Registers)aData[1]];

            aCpu.Registers[Registers.AC] = result;

            // Update the CPU flags based on the result of
            // the calculation just performed.
            // We do not need to check for an overflow here as it 
            // is not possible for an AND operation on two
            // of the same type to ever overflow. No new bits are
            // added and no casts are performed.
            base.UpdateCalculationFlags(aCpu, result);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg1 = (Registers)aData[0];
            var fromReg2 = (Registers)aData[1];

            // and R1, R2
            return $"{AsmName} {fromReg1}, {fromReg2}";
        }
    }
}
