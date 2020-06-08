using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions.Branching
{
    internal class CAL_LIT
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
                InsArgTypes.LiteralPointer
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null
            };

        public override OpCode OpCode =>
            OpCode.CAL_LIT;

        public override string AsmName => "call";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            aCpu.PushState();

            aCpu.Registers[Registers.IP] = (int)aData[0];

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var address = (int)aData[0];

            // call &$LITERAL
            return (OutputLiteralsAsHex) ?
                $"{AsmName} &$0x{address:X}" :
                $"{AsmName} &${address:X}";
        }
    }
}
