using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions.Data
{
    internal class MOV_LIT_MEM
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
            OpCode.MOV_LIT_MEM;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            // We do not care if this writes to executable
            // memory.
            aCpu.Vm.Memory
                .SetInt((int)aData[1],
                          (int)aData[0],
                          GetSecurityContext(),
                          false);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var literal = (int)aData[0];
            var toAddr = (int)aData[1];

            // mov $LITERAL, &ADDRESS
            return (OutputLiteralsAsHex) ?
                $"{AsmName} $0x{literal:X}, &0x{toAddr:X}" :
                $"{AsmName} ${literal}, &{toAddr}";
        }
    }
}
