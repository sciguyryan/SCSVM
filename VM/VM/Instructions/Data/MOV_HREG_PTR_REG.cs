using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions.Data
{
    internal class MOV_HREG_PTR_REG
        : Instruction
    {
        public override Type[] ArgumentTypes => 
            new Type[]
            {
                typeof(InstructionSizeHint),
                typeof(Registers),
                typeof(Registers),
            };

        public override InsArgTypes[] ArgumentRefTypes =>
            new InsArgTypes[]
            {
                InsArgTypes.InstructionSizeHint,
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
            OpCode.MOV_HREG_PTR_REG;

        public override string AsmName => "mov";

        public override bool Execute(InstructionData aData, Cpu aCpu)
        {
            var address = 
                aCpu.Registers[(Registers) aData[1]];

            var value = (InstructionSizeHint)aData[0] switch
            {
                InstructionSizeHint.BYTE
                    =>
                    aCpu.Vm.Memory
                        .GetValue(address,
                                  GetSecurityContext(),
                                  false),

                InstructionSizeHint.WORD
                    =>
                    aCpu.Vm.Memory
                        .GetInt(address,
                            GetSecurityContext(),
                            false),

                InstructionSizeHint.DWORD
                    =>
                    throw new NotImplementedException(),

                _ => throw new ArgumentOutOfRangeException()
            };

            aCpu.Registers[(Registers)aData[2]] = value;

            return false;
        }

        public override string ToString(InstructionData aData)
        {
            var sizeHintStr =
                ((InstructionSizeHint)aData[0]).ToString();

            var fromReg = (Registers)aData[1];
            var toReg = (Registers)aData[2];

            // mov TYPE &R1, R2
            return $"{AsmName} {sizeHintStr.ToUpper()} &{fromReg}, {toReg}";
        }
    }
}
