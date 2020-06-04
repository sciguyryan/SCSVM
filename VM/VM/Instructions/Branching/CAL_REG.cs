using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions.Branching
{
    internal class CAL_REG
        : Instruction
    {
        public override Type[] ArgumentTypes =>
            new Type[]
            {
                typeof(Registers)
            };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[]
            {
                InsArgTypes.RegisterPointer
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null
            };

        public override OpCode OpCode =>
            OpCode.CAL_REG;

        public override string AsmName => "call";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            aCpu.PushState();

            // Offset the address by current base size of the memory.
            // This is the bound of the memory outside of the 
            // executable memory region (e.g. main memory and stack).
            aCpu.Registers[Registers.IP] =
                aCpu.Vm.Memory.BaseMemorySize +
                aCpu.Registers[(Registers)aData[0]];

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var reg = (Registers)aData[0];

            // call &R1
            return $"{AsmName} &${reg}";
        }
    }
}
