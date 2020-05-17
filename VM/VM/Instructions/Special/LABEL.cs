using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
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

        public override OpCode OpCode => 
            OpCode.LABEL;

        public override string AsmName => "label";

        public override bool Execute(InstructionData aData, CPU aCpu)
        {
            // This should never be executed.
            return false;
        }

        public override string ToString(InstructionData aData)
        {
            // This should never be executed.
            var name = (string)aData[0];

            // label $ID NAME
            return $"{AsmName} {name}";
        }
    }
}
