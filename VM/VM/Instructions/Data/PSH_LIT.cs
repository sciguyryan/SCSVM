using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

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

        public override OpCode OpCode => 
            OpCode.PSH_LIT;

        public override string AsmName => "push";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            aCpu.Vm.Memory.StackPushInt((int)aData[0]);

            // Update the stack pointer register to reflect
            // the new stack position.
            // This must be done with a security-level context.
            aCpu.Registers[(Registers.SP, SecurityContext.System)] =
                aCpu.Vm.Memory.StackPointer;

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
