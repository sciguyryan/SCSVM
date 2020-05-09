﻿using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class AND_REG_LIT : Instruction
    {
        public override Type[] ArgumentTypes => 
            new [] { typeof(Registers), typeof(int) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, null };

        public override OpCode OpCode => 
            OpCode.AND_REG_LIT;

        public override string AsmName => "and";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            var result =
                cpu.Registers[(Registers)data[0]] & 
                (int)data[1];

            cpu.Registers[Registers.AC] = result;

            // Update the CPU flags based on the result of
            // the calculation just performed.
            // We do not need to check for an overflow here as it 
            // is not possible for an AND operation on two
            // of the same type to ever overflow. No new bits are
            // added and no casts are performed.
            base.UpdateCalculationFlags(cpu, result);

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var fromReg = (Registers)data[0];
            var literal = (int)data[1];

            // and R1, $LITERAL
            return $"{AsmName} {fromReg}, ${literal:X}";
        }
    }
}