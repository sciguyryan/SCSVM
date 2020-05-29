using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions.Arithmetic
{
    internal class MOD_REG_LIT
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[]
            {
                typeof(Registers),
                typeof(int)
            };
        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[]
            {
                InsArgTypes.Register,
                InsArgTypes.LiteralInteger
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode => 
            OpCode.MOD_REG_LIT;

        public override string AsmName => "mod";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            // TODO - modulo is very slow, so if we can find any fast
            // paths here, we should probably do that.
            var result = 
                (long)((int)aData[1]) % 
                aCpu.Registers[(Registers)aData[0]];

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
            var fromReg = (Registers)aData[0];
            var literal = (int)aData[1];

            // mod R1, $LITERAL
            return (OutputLiteralsAsHex) ?
                $"{AsmName} {fromReg}, $0x{literal:X}" :
                $"{AsmName} {fromReg}, ${literal}";
        }
    }
}
