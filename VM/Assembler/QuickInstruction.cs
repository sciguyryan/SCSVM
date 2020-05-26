using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;

namespace VMCore.Assembler
{
    /// <summary>
    /// A simple instruction class primarily for use
    /// with the quick compiler.
    /// </summary>
    public class QuickIns : IEquatable<QuickIns>
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

        public override bool Equals(object? aObj)
        {
            return Equals(aObj as QuickIns);
        }

        public bool Equals([AllowNull] QuickIns aOther)
        {
            // Check for null and compare run-time types.
            if (aOther == null || this.GetType() != aOther.GetType())
            {
                return false;
            }

            var q = (QuickIns)aOther;

            // We need to ensure that we do not pass a null value to
            // SequenceEqual or it will throw an exception.
            return
                Op == q.Op &&
                Label == q.Label &&
                (Args == null || q.Args == null) ?
                    Args == q.Args :
                    Args.SequenceEqual(q.Args);
        }

        public override int GetHashCode()
        {
            var hash = Op.GetHashCode();

            if (Label == null)
            {
                return hash;
            }

            hash = (hash * 17) + Label.GetHashCode();

            if (Args == null)
            {
                return hash;
            }

            hash = (hash * 17) + Args.Length;
            foreach (var a in Args)
            {
                hash *= 17;
                if (a != null)
                {
                    hash += a.GetHashCode();
                }
            }

            return hash;
        }

        public static bool operator ==(QuickIns aLeft,
                                       QuickIns aRight)
        {
            return object.Equals(aLeft, aRight);
        }

        public static bool operator !=(QuickIns aLeft,
                                       QuickIns aRight)
        {
            return !(aLeft == aRight);
        }
    }
}
