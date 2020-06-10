using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions.Data
{
    internal class POP
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
            OpCode.POP;

        public override string AsmName => "pop";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            aCpu.Registers[(Registers)aData[0]] = 
                aCpu.Vm.Memory.StackPopInt();

            // Update the stack pointer register to reflect
            // the new stack position.
            // This must be done with a security-level context.
            aCpu.Registers[(Registers.SP, SecurityContext.System)] = 
                aCpu.Vm.Memory.StackPointer;

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var reg = (Registers)aData[0];

            // pop R1
            return $"{AsmName} {reg}";
        }
    }
}
