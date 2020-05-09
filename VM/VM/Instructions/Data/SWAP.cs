using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class SWAP : Instruction
    {
        public override Type[] ArgumentTypes =>
            new [] { typeof(Registers), typeof(Registers) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, null };

        public override OpCode OpCode => 
            OpCode.SWAP;

        public override string AsmName => "swap";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            var r1 = (Registers)data[0];
            var r2 = (Registers)data[1];

            var v1 =
                cpu.Registers[(r1, GetSecurityContext())];
            var v2 =
                cpu.Registers[(r2, GetSecurityContext())];

            cpu.Registers[(r1, GetSecurityContext())] = v2;
            cpu.Registers[(r2, GetSecurityContext())] = v1;

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var r1 = (Registers)data[0];
            var r2 = (Registers)data[1];

            // swap R1, R2
            return $"{AsmName} {r1}, {r2}";
        }
    }
}