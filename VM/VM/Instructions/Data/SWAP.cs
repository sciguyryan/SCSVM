using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions.Data
{
    internal class SWAP
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
            OpCode.SWAP;

        public override string AsmName => "swap";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            var r1 = (Registers)aData[0];
            var r2 = (Registers)aData[1];

            var v1 =
                aCpu.Registers[(r1, GetSecurityContext())];
            var v2 =
                aCpu.Registers[(r2, GetSecurityContext())];

            aCpu.Registers[(r1, GetSecurityContext())] = v2;
            aCpu.Registers[(r2, GetSecurityContext())] = v1;

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var r1 = (Registers)aData[0];
            var r2 = (Registers)aData[1];

            // swap R1, R2
            return $"{AsmName} {r1}, {r2}";
        }
    }
}
