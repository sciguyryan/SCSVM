using VMCore.VM;
using VMCore.VM.Core;

namespace VMCore.Assembler
{
    /// <summary>
    /// A simple instruction class primarily for use
    /// with the quick compiler.
    /// </summary>
    public class QuickIns
    {
        public OpCode Op { get; }

        public object[] Args { get; }

        public AsmLabel Label { get; }

        public QuickIns(OpCode aOpCode,
                        object[] aArgs = null,
                        AsmLabel aLabel = null)
        {
            Op = aOpCode;
            Args = aArgs;
            Label = aLabel;
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
