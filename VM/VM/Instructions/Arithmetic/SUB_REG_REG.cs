using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class SUB_REG_REG : Instruction
    {
        public override Type[] ArgumentTypes => 
            new [] { typeof(Registers), typeof(Registers) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, null };

        public override OpCode OpCode => 
            OpCode.SUB_REG_REG;

        public override string AsmName => "sub";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            // Intentionally do the calculation as a long
            // so that we can check for an overflow.
            // At least one of these must be cast to a long
            // in order for this to work as expected.
            long result = 
                (long)cpu.Registers[(Registers)data[0]] - 
                cpu.Registers[(Registers)data[1]];

            // Perform the cast as an unchecked calculation.
            // Simply disregard the MSBs and take LSBs.
            cpu.Registers[Registers.AC] = 
                unchecked((int)result);

            // Update the CPU flags based on the result of
            // the calculation just performed.
            // If the value is above the bounds for an
            // integer then the overflow flag will be set.
            base.UpdateCalculationFlags(cpu, (int)result,
                                        (result > int.MaxValue ||
                                         result < int.MinValue));

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var fromReg1 = (Registers)data[0];
            var fromReg2 = (Registers)data[1];

            // sub R1, R2
            return $"{AsmName} {fromReg1}, {fromReg2}";
        }
    }
}