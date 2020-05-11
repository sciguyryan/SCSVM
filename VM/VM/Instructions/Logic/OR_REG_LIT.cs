﻿using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class OR_REG_LIT
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[]
            {
                typeof(Registers),
                typeof(int)
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode =>
            OpCode.OR_REG_LIT;

        public override string AsmName => "or";

        public override bool Execute(InstructionData aData, CPU aCpu)
        {
            var result =
                aCpu.Registers[(Registers)aData[0]] |
                (int)aData[1];

            aCpu.Registers[Registers.AC] = result;

            // Update the CPU flags based on the result of
            // the calculation just performed.
            // We do not need to check for an overflow here as it 
            // is not possible for an OR operation on two
            // of the same type to ever overflow. No new bits are
            // added and no casts are performed.
            base.UpdateCalculationFlags(aCpu, result);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg = (Registers)aData[0];
            var literal = (int)aData[1];

            // or R1, $LITERAL
            return $"{AsmName} {fromReg}, ${literal:X}";
        }
    }
}
