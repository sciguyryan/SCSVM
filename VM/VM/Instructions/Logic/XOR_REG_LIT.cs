using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class XOR_REG_LIT
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
                InsArgTypes.LiteralInteger,
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode => 
            OpCode.XOR_REG_LIT;

        public override string AsmName => "xor";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            var result = 
                aCpu.Registers[(Registers)aData[0]] ^ 
                (int)aData[1];

            aCpu.Registers[Registers.AC] = result;

            // Update the Cpu flags based on the result of
            // the calculation just performed.
            // We do not need to check for an overflow here as it 
            // is not possible for a XOR operation on two
            // of the same type to ever overflow. No new bits are
            // added and no casts are performed.
            base.UpdateCalculationFlags(aCpu, result);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg = (Registers)aData[0];
            var literal = (int)aData[1];

            // xor R1, $LITERAL
            return (OutputLiteralsAsHex) ? 
                $"{AsmName} {fromReg}, $0x{literal:X}" :
                $"{AsmName} {fromReg}, ${literal}";
        }
    }
}
