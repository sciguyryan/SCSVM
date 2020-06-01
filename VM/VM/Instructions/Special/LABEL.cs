using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions.Special
{
    internal class LABEL
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[]
            {
                typeof(string),
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
            };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[]
            {
                InsArgTypes.String
            };

        public override OpCode OpCode => 
            OpCode.LABEL;

        public override string AsmName => "label";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            // This should never be executed.
            return false;
        }

        public override string ToString(InstructionData aData)
        {
            // label $NAME
            return $"{AsmName} {(string)aData[0]}";
        }
    }
}
