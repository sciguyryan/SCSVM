using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions.Data
{
    internal class PSH_LIT
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
                InsArgTypes.LiteralInteger,
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
            };

        public override OpCode OpCode => 
            OpCode.PSH_LIT;

        public override string AsmName => "push";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            aCpu.Vm.Memory.StackPushInt((int)aData[0]);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var literal = (int)aData[0];

            // push $LITERAL
            return (OutputLiteralsAsHex) ?
                $"{AsmName} $0x{literal:X}" :
                $"{AsmName} ${literal}";
        }
    }
}
