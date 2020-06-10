using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions.Branching
{
    internal class JLE_LIT
        : Instruction
    {
        public override Type[] ArgumentTypes =>
            new Type[]
            {
                typeof(int),
                typeof(int)
            };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[]
            {
                InsArgTypes.LiteralInteger,
                InsArgTypes.LiteralPointer,
            };

        public override OpCode OpCode =>
            OpCode.JLE_LIT;

        public override string AsmName => "jle";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            if ((int)aData[0] <= aCpu.Registers[Registers.AC])
            {
                aCpu.Registers[Registers.IP] = (int)aData[1];
            }

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var literal = (int)aData[0];
            var address = (int)aData[1];

            // jle $LITERAL, &ADDRESS
            return (OutputLiteralsAsHex) ?
                $"{AsmName} $0x{literal:X}, &0x{address:X}" :
                $"{AsmName} ${literal}, &{address}";
        }
    }
}
