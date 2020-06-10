#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;
using OpCode = VMCore.VM.Core.OpCode;

namespace VMCore.Assembler
{
    /// <summary>
    /// A compiler instruction class primarily for use
    /// with the quick compiler.
    /// </summary>
    public class CompilerIns : IEquatable<CompilerIns>
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
            Labels = new AsmLabel[Args.Length];
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

        public override bool Equals(object? aObj)
        {
            return Equals(aObj as CompilerIns);
        }

        public bool Equals([AllowNull] CompilerIns aOther)
        {
            // Check for null and compare run-time types.
            if (aOther is null || GetType() != aOther.GetType())
            {
                return false;
            }

            var q = aOther;

            return
                Op == q.Op &&
                Labels.SequenceEqual(q.Labels) &&
                Args.SequenceEqual(q.Args);
        }

        public override int GetHashCode()
        {
            var hash = Op.GetHashCode();

            hash = (hash * 17) + Labels.Length;
            foreach (var l in Labels)
            {
                if (l is null)
                {
                    continue;
                }

                hash *= 17;
                hash += l.GetHashCode();
            }

            hash = (hash * 17) + Args.Length;
            foreach (var a in Args)
            {
                hash *= 17;
                hash += a.GetHashCode();
            }

            return hash;
        }

        public static bool operator ==(CompilerIns aLeft,
            CompilerIns aRight)
        {
            return Equals(aLeft, aRight);
        }

        public static bool operator !=(CompilerIns aLeft,
            CompilerIns aRight)
        {
            return !(aLeft == aRight);
        }
    }
}
