using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions.Arithmetic
{
    internal class MOD_LIT_REG
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
                InsArgTypes.Register
            };

        public override OpCode OpCode => 
            OpCode.MOD_LIT_REG;

        public override string AsmName => "mod";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            // TODO - modulo is very slow, so if we can find any fast
            // paths here, we should probably do that.
            var result = 
                (long)aCpu.Registers[(Registers)aData[1]] % 
                (int)aData[0];

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

            // mod $LITERAL, R1
            return (OutputLiteralsAsHex) ? 
                    $"{AsmName} $0x{literal:X}, {fromReg}" :
                    $"{AsmName} ${literal}, {fromReg}";
        }
    }
}
