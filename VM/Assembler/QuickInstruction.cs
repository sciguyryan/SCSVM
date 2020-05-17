using VMCore.VM;
using VMCore.VM.Core;

namespace VMCore.Assembler
{
    /// <summary>
    /// Quick and dirty instruction class for use with the quick compiler.
    /// WARNING:
    /// Do not use outside of this purpose as it contains
    /// none of the validation present in the other classes.
    /// </summary>
    public class QuickIns
    {
        public OpCode Op { get; private set; }

        public object[] Args { get; private set; }

        public AsmLabel Label { get; private set; }

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
