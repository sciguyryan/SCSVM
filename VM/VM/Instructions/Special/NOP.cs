using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class NOP : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[] { };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null };

        public override OpCode OpCode => 
            OpCode.NOP;

        public override string AsmName => "nop";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            return false;
        }

        public override string ToString(InstructionData data)
        {
            // nop
            return $"{AsmName}";
        }
    }
}