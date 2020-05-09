using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class BIT : Instruction
    {
        public override Type[] ArgumentTypes => 
            new [] { typeof(int), typeof(Registers) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, null };

        public override OpCode OpCode => 
            OpCode.BIT;

        public override string AsmName => "bit";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            var val =
                (int)cpu.Registers[(Registers)data[1]];

            var bitSet =
                Utils.IsBitSet(val, (int)data[0]) ? 1 : 0;

            cpu.SetResultFlagPair(bitSet);

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var bit = (int)data[0];
            var fromReg = (Registers)data[1];

            // bit $LITERAL, R1
            return $"{AsmName} ${bit:X}, {fromReg}";
        }
    }
}