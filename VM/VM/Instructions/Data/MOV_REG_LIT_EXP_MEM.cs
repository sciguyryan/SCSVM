﻿using System;
using VMCore.Expressions;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions.Data
{
    internal class MOV_REG_LIT_EXP_MEM
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[]
            { 
                typeof(Registers),
                typeof(string)
            };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[]
            {
                InsArgTypes.Register,
                InsArgTypes.Expression,
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                typeof(int)
            };

        public override OpCode OpCode => 
            OpCode.MOV_REG_LIT_EXP_MEM;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            var pos = 
                new Parser((string)aData[1])
                    .ParseExpression()
                    .Evaluate(aCpu);

            // We do not care if this write
            // is within an executable
            // region or not.
            aCpu.Vm.Memory
                .SetInt(pos,
                        aCpu.Registers[(Registers)aData[0]],
                        GetSecurityContext(),
                        false);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg = (Registers)aData[0];

            // mov R1, [EXPRESSION]
            return $"{AsmName} {fromReg}, [{aData[1]}]";
        }
    }
}
