using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
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

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode => 
            OpCode.BIT;

        public override string AsmName => "bit";

        public override bool Execute(InstructionData aData, CPU aCpu)
        {
            var val =
                (int)aCpu.Registers[(Registers)aData[1]];

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
            return $"{AsmName} ${bit:X}, {fromReg}";
        }
    }
}