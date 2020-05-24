﻿using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class LSF_REG_LIT
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[]
            {
                typeof(Registers),
                typeof(int)
            };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[]
            {
                InsArgTypes.Register,
                InsArgTypes.LiteralInteger,
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode => 
            OpCode.LSF_REG_LIT;

        public override string AsmName => "lsf";

        public override bool Execute(InstructionData aData, CPU aCpu)
        {
            aCpu.Registers[(Registers)aData[0]] <<= 
                (int)aData[1];

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg = (Registers)aData[0];
            var literal = (int)aData[1];

            // lsf R1, $LITERAL
            return (OutputLiteralsAsHex) ?
                $"{AsmName} {fromReg}, $0x{literal:X}" :
                $"{AsmName} {fromReg}, ${literal}";
        }
    }
}
