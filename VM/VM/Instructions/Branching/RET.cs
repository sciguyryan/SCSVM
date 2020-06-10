using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions.Branching
{
    internal class RET
        : Instruction
    {
        public override Type[] ArgumentTypes =>
            new Type[] { };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[] { };

        public override OpCode OpCode =>
            OpCode.RET;

        public override string AsmName => "ret";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            aCpu.PopState();

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            // ret
            return $"{AsmName}";
        }
    }
}