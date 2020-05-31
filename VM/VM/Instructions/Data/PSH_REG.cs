using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions.Data
{
    internal class PSH_REG
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
                InsArgTypes.Register,
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
            };

        public override OpCode OpCode => 
            OpCode.PSH_REG;

        public override string AsmName => "push";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            aCpu.Vm.Memory
                .StackPushInt(aCpu.Registers[(Registers)aData[0]]);

            // Update the stack pointer register
            // to reflect the new stack position.
            aCpu.Registers[Registers.SP] -= sizeof(int);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            // push R1
            return $"{AsmName} {(Registers)aData[0]}";
        }
    }
}
