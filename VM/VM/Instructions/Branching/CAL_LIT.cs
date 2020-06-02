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

        public override bool CanBindToLabel(int aArgumentId)
        {
            return aArgumentId == 0;
        }

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            aCpu.PushState();

            // Offset the address by current base size of the memory.
            // This is the bound of the memory outside of the 
            // executable memory region (e.g. main memory and stack).
            aCpu.Registers[Registers.IP] =
                aCpu.Vm.Memory.BaseMemorySize + (int)aData[0];

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var literal = (int)aData[0];

            // call &$LITERAL
            return (OutputLiteralsAsHex) ?
                $"{AsmName} &$0x{literal:X}" :
                $"{AsmName} &${literal:X}";
        }
    }
}