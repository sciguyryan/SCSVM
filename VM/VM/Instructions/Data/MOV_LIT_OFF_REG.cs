﻿using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class MOV_LIT_OFF_REG
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[]
            {
                typeof(int),
                typeof(Registers),
                typeof(Registers)
            };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[]
            {
                InsArgTypes.LiteralInteger,
                InsArgTypes.RegisterPointer,
                InsArgTypes.Register,
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null,
                null
            };

        public override OpCode OpCode => 
            OpCode.MOV_LIT_OFF_REG;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            var pos = 
                (int)aData[0] +
                aCpu.Registers[(Registers)aData[1]];

            aCpu.Registers[(Registers)aData[2]] = 
                aCpu.Vm.Memory
                .GetInt(pos, GetSecurityContext(), false);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var literal = (int)aData[0];
            var fromReg = (int)aData[1];
            var toReg = (Registers)aData[2];

            // mov $LITERAL, &R1, R2
            return (OutputLiteralsAsHex) ?
                $"{AsmName} $0x{literal:X}, &{fromReg}, {toReg}" :
                $"{AsmName} ${literal}, &{fromReg}, {toReg}";
        }
    }
}
