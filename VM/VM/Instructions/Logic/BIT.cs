using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;

namespace VMCore.VM.Instructions.Logic
{
    internal class BIT
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

        public override OpCode OpCode => 
            OpCode.BIT;

        public override string AsmName => "bit";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            var val =
                aCpu.Registers[(Registers)aData[1]];

            var bitSet =
                Utils.IsBitSet(val, (int)aData[0]) ? 1 : 0;

            aCpu.SetResultFlagPair(bitSet);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var bit = (int)aData[0];
            var fromReg = (Registers)aData[1];

            // bit $LITERAL, R1
            return (OutputLiteralsAsHex) ?
                $"{AsmName} $0x{bit:X}, {fromReg}" :
                $"{AsmName} ${bit}, {fromReg}";
        }
    }
}
