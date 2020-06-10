using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions.Bit
{
    internal class LSF_REG_REG
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[]
            {
                typeof(Registers),
                typeof(Registers)
            };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[]
            {
                InsArgTypes.Register,
                InsArgTypes.Register,
            };

        public override OpCode OpCode =>
            OpCode.LSF_REG_REG;

        public override string AsmName => "lsf";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            aCpu.Registers[(Registers)aData[0]] <<= 
                aCpu.Registers[(Registers)aData[1]];

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg1 = (Registers)aData[0];
            var fromReg2 = (Registers)aData[1];

            // lsf R1, R2
            return $"{AsmName} {fromReg1}, {fromReg2}";
        }
    }
}
