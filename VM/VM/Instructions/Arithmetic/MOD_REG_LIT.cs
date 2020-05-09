﻿using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class MOD_REG_LIT : Instruction
    {
        public override Type[] ArgumentTypes => 
            new [] { typeof(Registers), typeof(int) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, null };

        public override OpCode OpCode => 
            OpCode.MOD_REG_LIT;

        public override string AsmName => "mod";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            // TODO - modulo is very slow, so if we can find any fast
            // paths here, we should probably do that.
            long result = 
                (long)((int)data[1]) % 
                cpu.Registers[(Registers)data[0]];

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
            var fromReg = (Registers)data[0];
            var literal = (int)data[1];

            // mod R1, $LITERAL
            return $"{AsmName} {fromReg}, ${literal:X}";
        }
    }
}