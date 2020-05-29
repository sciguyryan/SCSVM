using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions.Special
{
    internal class HLT
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[] { };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[] { };

        public override OpCode OpCode => 
            OpCode.HLT;

        public override string AsmName => "hlt";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            // Returning true here will instruct the virtual machine
            // to suspend execution.
            return true;
        }

        public override string ToString(InstructionData aData)
        {
            // hlt
            return $"{AsmName}";
        }
    }
}
