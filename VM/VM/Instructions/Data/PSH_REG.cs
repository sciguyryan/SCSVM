using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

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

        public override OpCode OpCode => 
            OpCode.PSH_REG;

        public override string AsmName => "push";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            aCpu.Vm.Memory
                .StackPushInt(aCpu.Registers[(Registers)aData[0]]);

            // Update the stack pointer register to reflect
            // the new stack position.
            // This must be done with a security-level context.
            aCpu.Registers[(Registers.SP, SecurityContext.System)] =
                aCpu.Vm.Memory.StackPointer;

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            // push R1
            return $"{AsmName} {(Registers)aData[0]}";
        }
    }
}
