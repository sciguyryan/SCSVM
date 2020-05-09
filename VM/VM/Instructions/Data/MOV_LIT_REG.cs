using System;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class MOV_LIT_REG : Instruction
    {
        public override Type[] ArgumentTypes =>
            new [] { typeof(int), typeof(Registers) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, null };

        public override OpCode OpCode => 
            OpCode.MOV_LIT_REG;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            cpu.Registers[(Registers)data[1]] = 
                (int)data[0];

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var literal = (int)data[0];
            var toReg = (Registers)data[1];

            // mov $LITERAL, R1
            return $"{AsmName} ${literal:X}, {toReg}";
        }
    }
}