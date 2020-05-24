using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class MUL_LIT_REG
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[]
            {
                typeof(int),
                typeof(Registers)
            };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[]
            {
                InsArgTypes.LiteralInteger,
                InsArgTypes.Register,
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode
            => OpCode.MUL_LIT_REG;

        public override string AsmName => "mul";

        public override bool Execute(InstructionData aData, CPU aCpu)
        {

            // Intentionally do the calculation as a long
            // so that we can check for an overflow.
            // At least one of these must be cast to a long
            // in order for this to work as expected.
            long result =
                (long)((int)aData[0]) * 
                aCpu.Registers[(Registers)aData[1]];

            // Perform the cast as an unchecked calculation.
            // Simply disregard the MSBs and take LSBs.
            aCpu.Registers[Registers.AC] = 
                unchecked((int)result);

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
            var literal = (int)aData[0];
            var fromReg = (Registers)aData[1];

            // mul $LITERAL, R1
            return (OutputLiteralsAsHex) ?
                $"{AsmName} $0x{literal:X}, {fromReg}" :
                $"{AsmName} ${literal}, {fromReg}";
        }
    }
}
