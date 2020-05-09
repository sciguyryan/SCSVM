using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Expressions;

namespace VMCore.VM.Instructions
{
    internal class MOV_LIT_EXP_OFF_REG : Instruction
    {
        public override Type[] ArgumentTypes =>
            new[] { typeof(string), typeof(Registers) };

        public override Type[] ExpressionArgumentTypes =>
            new[] { typeof(int), null };

        public override OpCode OpCode =>
            OpCode.MOV_LIT_EXP_OFF_REG;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData data, CPU cpu)
        {
            var p = 
                new Parser((string)data[0]);

            var pos = (int)p.ParseExpression().Evaluate(cpu);

            cpu.Registers[(Registers)data[1]] =
                cpu.VM.Memory
                .GetValueAsType<int>(pos, GetSecurityContext());

            return false;
        }

        public override string ToString(InstructionData data)
        {
            var toReg = (Registers)data[1];

            // mov [EXPRESSION], R2
            return $"{AsmName} [{data[0]}], {toReg}";
        }
    }
}