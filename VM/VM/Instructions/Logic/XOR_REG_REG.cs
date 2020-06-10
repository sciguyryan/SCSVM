using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions.Logic
{
    internal class XOR_REG_REG
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
            OpCode.XOR_REG_REG;

        public override string AsmName => "xor";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            var result = 
                aCpu.Registers[(Registers)aData[0]] ^ 
                aCpu.Registers[(Registers)aData[1]];

            aCpu.Registers[Registers.AC] = result;

            // Update the CPU flags based on the result of
            // the calculation just performed.
            // We do not need to check for an overflow here as it 
            // is not possible for an XOR operation on two
            // of the same type to ever overflow. No new bits are
            // added and no casts are performed.
            base.UpdateCalculationFlags(aCpu, result);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg1 = (Registers)aData[0];
            var fromReg2 = (Registers)aData[1];

            // xor R1, R2
            return $"{AsmName} {fromReg1}, {fromReg2}";
        }
    }
}
