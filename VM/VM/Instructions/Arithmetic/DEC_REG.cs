using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions.Arithmetic
{
    internal class DEC_REG 
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
                InsArgTypes.Register
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null
            };

        public override OpCode OpCode => 
            OpCode.DEC_REG;

        public override string AsmName => "dec";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            --aCpu.Registers[(Registers)aData[0]];

            // We do not need to update the CPU flags
            // here as the result is not going
            // into the accumulator register.

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg = (Registers)aData[0];

            // dec R1
            return $"{AsmName} {fromReg}";
        }
    }
}
