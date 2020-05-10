using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class SUB_REG_LIT
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
            OpCode.SUB_REG_LIT;

        public override string AsmName => "sub";

        public override bool Execute(InstructionData aData, CPU aCpu)
        {
            // Intentionally do the calculation as a long
            // so that we can check for an overflow.
            // At least one of these must be cast to a long
            // in order for this to work as expected.
            long result =
                 (long)((int)aData[1]) - 
                 aCpu.Registers[(Registers)aData[0]];

            // Perform the cast as an unchecked calculation.
            // Simply disregard the MSBs and take LSBs.
            aCpu.Registers[Registers.AC] = unchecked((int)result);

            // Update the CPU flags based on the result of
            // the calculation just performed.
            // If the value is above the bounds for an
            // integer then the overflow flag will be set.
            base.UpdateCalculationFlags(aCpu, (int)result,
                                        (result > int.MaxValue ||
                                         result < int.MinValue));

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg = (Registers)aData[0];
            var literal = (int)aData[1];

            // sub R1, $LITERAL
            return $"{AsmName} {fromReg}, ${literal:X}";
        }
    }
}