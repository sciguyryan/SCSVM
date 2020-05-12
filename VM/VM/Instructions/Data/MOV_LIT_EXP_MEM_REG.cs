using System;
using VMCore.VM.Core;
using VMCore.Expressions;

namespace VMCore.VM.Instructions
{
    internal class MOV_LIT_EXP_MEM_REG
        : Instruction
    {
        public override Type[] ArgumentTypes =>
            new Type[]
            {
                typeof(string),
                typeof(Registers)
            };

        public override Type[] ExpressionArgumentTypes =>
            new Type[]
            {
                typeof(int),
                null
            };

        public override OpCode OpCode =>
            OpCode.MOV_LIT_EXP_MEM_REG;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData aData, CPU aCpu)
        {
            var pos = (int)new Parser((string)aData[0])
                    .ParseExpression()
                    .Evaluate(aCpu);

            // We do not care if this read
            // is within an executable
            // region or not.
            aCpu.Registers[(Registers)aData[1]] =
                aCpu.VM.Memory
                .GetInt(pos, GetSecurityContext(), false);

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var toReg = (Registers)aData[1];

            // mov [EXPRESSION], R2
            return $"{AsmName} [{aData[0]}], {toReg}";
        }
    }
}
