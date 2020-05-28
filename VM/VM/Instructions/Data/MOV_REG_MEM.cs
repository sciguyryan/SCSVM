using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class MOV_REG_MEM
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[]
            { 
                typeof(Registers),
                typeof(int)
            };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[]
            {
                InsArgTypes.Register,
                InsArgTypes.LiteralPointer,
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode => 
            OpCode.MOV_REG_MEM;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            // We do not care if this write
            // is within an executable region or not.
            aCpu.Vm.Memory
                .SetInt((int)aData[1],
                          aCpu.Registers[(Registers)aData[0]],
                          GetSecurityContext(),
                          false);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg = (Registers)aData[0];
            var memoryAddr = (int)aData[1];

            // mov R1, &ADDRESS
            return (OutputLiteralsAsHex) ?
                $"{AsmName} {fromReg}, &0x{memoryAddr:X}" :
                $"{AsmName} {fromReg}, &{memoryAddr}";
        }
    }
}
