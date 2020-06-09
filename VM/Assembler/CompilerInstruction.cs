using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;
using OpCode = VMCore.VM.Core.OpCode;

namespace VMCore.Assembler
{
    /// <summary>
    /// A compiler instruction class primarily for use
    /// with the quick compiler.
    /// </summary>
    public class CompilerIns
    {
        public OpCode Op { get; }

        public object[] Args { get; }

        public AsmLabel[] Labels { get; }

        public CompilerIns(OpCode aOpCode)
        {
            Op = aOpCode;
            Args = new object[0];
            Labels = new AsmLabel[0];
        }

        public CompilerIns(OpCode aOpCode,
                           object[] aArgs)
        {
            Op = aOpCode;
            Args = aArgs;
            Labels = new AsmLabel[0];
        }

        public CompilerIns(OpCode aOpCode,
                           object[] aArgs,
                           AsmLabel aLabel)
        {
            Op = aOpCode;
            Args = aArgs;
            Labels = new [] { aLabel };
        }

        public CompilerIns(OpCode aOpCode,
                           object[] aArgs,
                           AsmLabel[] aLabels)
        {
            Op = aOpCode;
            Args = aArgs;
            Labels = aLabels;
        }

        public override string ToString()
        {
            var opIns = new InstructionData
            {
                OpCode = Op
            };

            foreach (var arg in Args)
            {
                var insArg = new InstructionArg
                {
                    Value = arg
                };

                opIns.Args.Add(insArg);
            }

            return ReflectionUtils.InstructionCache[Op].ToString(opIns);
        }
    }
}
