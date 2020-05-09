using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class LSF_REG_LIT : Instruction
    {
        public override Type[] ArgumentTypes => 
            new [] { typeof(Registers), typeof(int) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, null };

        public override OpCode OpCode => 
            OpCode.LSF_REG_LIT;

        public override string AsmName => "lsf";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            cpu.Registers[(Registers)data[0]] <<= 
                (int)data[1];

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var fromReg = (Registers)data[0];
            var literal = (int)data[1];

            // lsf R1, $LITERAL
            return $"{AsmName} {fromReg}, ${literal:X}";
        }
    }
}