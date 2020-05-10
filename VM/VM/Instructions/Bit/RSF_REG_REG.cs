﻿using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class RSF_REG_REG
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[]
            {
                typeof(Registers),
                typeof(Registers)
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode => 
            OpCode.RSF_REG_REG;

        public override string AsmName => "rsf";

        public override bool Execute(InstructionData aData, CPU aCpu)
        {
            aCpu.Registers[(Registers)aData[0]] >>= 
                aCpu.Registers[(Registers)aData[1]];

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg1 = (Registers)aData[0];
            var fromReg2 = (Registers)aData[1];

            // rsf R1, R2
            return $"{AsmName} {fromReg1}, {fromReg2}";
        }
    }
}