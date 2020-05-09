using System;
using VMCore.Expressions;
using VMCore.VM.Core;

namespace VMCore.VM.Instructions
{
    internal class MOV_REG_LIT_EXP_MEM : Instruction
    {
        public override Type[] ArgumentTypes => 
            new[] { typeof(Registers), typeof(string) };

        public override Type[] ExpressionArgumentTypes =>
            new Type[] { null, typeof(int) };

        public override OpCode OpCode => 
            OpCode.MOV_REG_LIT_EXP_MEM;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            var bytes = 
                BitConverter
                    .GetBytes(cpu.Registers[(Registers)data[0]]);

            var pos = (int)new Parser((string)data[1])
                    .ParseExpression()
                    .Evaluate(cpu);

            cpu.VM.Memory
                .SetValueRange(pos, bytes,
                               GetSecurityContext());

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var fromReg = (Registers)data[0];
            var memoryAddr = (int)data[1];

            // mov R1, [EXPRESSION]
            return $"{AsmName} {fromReg}, ${memoryAddr:X}";
        }
    }
}