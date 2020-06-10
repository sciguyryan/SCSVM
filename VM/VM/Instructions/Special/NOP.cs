using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions.Special
{
    internal class NOP
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[] { };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[] { };

        public override OpCode OpCode => 
            OpCode.NOP;

        public override string AsmName => "nop";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            return false;
        }

        public override string ToString(InstructionData aData)
        {
            // nop
            return $"{AsmName}";
        }
    }
}
