﻿using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions.Data
{
    internal class MOV_REG_PTR_REG
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
                InsArgTypes.RegisterPointer,
                InsArgTypes.Register,
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode => 
            OpCode.MOV_REG_PTR_REG;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            aCpu.Registers[(Registers)aData[1]] = 
                aCpu.Vm.Memory
                .GetInt(aCpu.Registers[(Registers)aData[0]],
                        GetSecurityContext(),
                        false);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg = (Registers)aData[0];
            var toReg = (Registers)aData[1];

            // mov &R1, R2
            return $"{AsmName} &{fromReg}, {toReg}";
        }
    }
}
