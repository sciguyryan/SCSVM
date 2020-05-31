﻿using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions.Branching
{
    internal class JLT_REG
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

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
                null
            };

        public override OpCode OpCode =>
            OpCode.JLT_REG;

        public override string AsmName => "jlt";

        public override bool CanBindToLabel(int aArgumentId)
        {
            return (aArgumentId == 1);
        }

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            if (aCpu.Registers[(Registers)aData[0]] <
                aCpu.Registers[Registers.AC])
            {
                // Offset the address by current base size of the memory.
                // This is the bound of the memory outside of the 
                // executable memory region (e.g. main memory and stack).
                aCpu.Registers[Registers.IP] =
                    aCpu.Vm.Memory.BaseMemorySize + (int)aData[1];
            }

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var fromReg = (Registers)aData[0];
            var address = (int)aData[1];

            // jlt R1, &ADDRESS
            return (OutputLiteralsAsHex) ?
                $"{AsmName} {fromReg}, &0x{address:X}" :
                $"{AsmName} {fromReg}, &{address}";
        }
    }
}
