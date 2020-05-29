using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions.Logic
{
    internal class NOT
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
                null
            };

        public override OpCode OpCode => 
            OpCode.NOT;

        public override string AsmName => "not";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            var result = 
                ~aCpu.Registers[(Registers)aData[0]];

            aCpu.Registers[Registers.AC] = result;

            // Update the CPU flags based on the result of
            // the calculation just performed.
            // We do not need to check for an overflow here as it 
            // is not possible for a NOT operation on two
            // of the same type to ever overflow. No new bits are
            // added and no casts are performed.

            base.UpdateCalculationFlags(aCpu, result);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var register = (Registers)aData[0];

            // not R1
            return $"{AsmName} {register}";
        }
    }
}
