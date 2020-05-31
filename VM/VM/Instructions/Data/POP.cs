﻿using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions.Data
{
    internal class POP
        : Instruction
    {
        public override Type[] ArgumentTypes =>
            new Type[]
            {
                typeof(Registers)
            };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[]
            {
                InsArgTypes.Register,
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                null,
            };

        public override OpCode OpCode => 
            OpCode.POP;

        public override string AsmName => "pop";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            aCpu.Registers[(Registers)aData[0]] = 
                aCpu.Vm.Memory.StackPopInt();

            // Update the stack pointer register
            // to reflect the new stack position.
            aCpu.Registers[Registers.SP] = 
                aCpu.Vm.Memory.StackPointer;

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var reg = (Registers)aData[0];

            // pop R1
            return $"{AsmName} {reg}";
        }
    }
}