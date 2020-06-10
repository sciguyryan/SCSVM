using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions.Arithmetic
{
    internal class ADD_LIT_REG
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
            OpCode.ADD_LIT_REG;

        public override string AsmName => "add";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            // Intentionally do the calculation as a long
            // so that we can check for an overflow.
            // At least one of these must be cast to a long
            // in order for this to work as expected.
            var result =
                (long)(int)aData[0] + 
                aCpu.Registers[(Registers)aData[1]];

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
            var literal = (int)aData[0];
            var fromReg = (Registers)aData[1];

            // add $LITERAL, R1
            return (OutputLiteralsAsHex) ? 
                $"{AsmName} $0x{literal:X}, {fromReg}" :
                $"{AsmName} ${literal}, {fromReg}";
        }
    }
}
