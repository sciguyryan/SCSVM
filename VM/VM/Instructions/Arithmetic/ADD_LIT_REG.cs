﻿using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class ADD_LIT_REG : Instruction
    {
        public override Type[] ArgumentTypes => 
            new [] { typeof(int), typeof(Registers) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, null };

        public override OpCode OpCode => 
            OpCode.ADD_LIT_REG;

        public override string AsmName => "add";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            // Intentionally do the calculation as a long
            // so that we can check for an overflow.
            // At least one of these must be cast to a long
            // in order for this to work as expected.
            long result =
                (long)((int)data[0]) + 
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
            var literal = (int)data[0];
            var fromReg = (Registers)data[1];

            // add $LITERAL, R1
            return $"{AsmName} ${literal:X}, {fromReg}";
        }
    }
}