﻿using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions.Branching
{
    internal class JLE_REG
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
                InsArgTypes.LiteralPointer,
            };

        public override OpCode OpCode =>
            OpCode.JLE_REG;

        public override string AsmName => "jle";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            if (aCpu.Registers[(Registers)aData[0]] <=
                aCpu.Registers[Registers.AC])
            {
                aCpu.Registers[Registers.IP] = (int)aData[1];
            }

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg = (Registers)aData[0];
            var address = (int)aData[1];

            // jle R1, &ADDRESS
            return (OutputLiteralsAsHex) ?
                $"{AsmName} {fromReg}, &0x{address:X}" :
                $"{AsmName} {fromReg}, &{address}";
        }
    }
}
