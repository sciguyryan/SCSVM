using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions.Special
{
    internal class SUBROUTINE
        : Instruction
    {
        public override Type[] ArgumentTypes =>
            new Type[]
            {
                typeof(int)
            };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[]
            {
                InsArgTypes.LiteralInteger
            };

        public override OpCode OpCode =>
            OpCode.SUBROUTINE;

        public override string AsmName => "";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            return false;
        }

        public override string ToString(InstructionData aData)
        {
            // SUB[ID]:
            return $"SUB{(int)aData[0]}:";
        }
    }
}
