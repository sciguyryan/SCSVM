﻿#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VMCore.VM.Core;
using VMCore.VM.Core.Utilities;

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

        public AsmLabel? Label { get; }

        public CompilerIns(OpCode aOpCode,
                        object[]? aArgs = null,
                        AsmLabel? aLabel = null)
        {
            Op = aOpCode;
            Args = aArgs ?? new object[0];
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

            // If one (but not both) of the labels are not
            // null then they cannot be equal.
            // We need to check this here to ensure that we do not
            // pass a null value to SequenceEqual or it will throw
            // an exception.
            if (Label is null || q.Label is null)
            {
                return (Label is null && q.Label is null);
            }

            return
                Op == q.Op &&
                Label == q.Label &&
                Args.SequenceEqual(q.Args);
        }

        public override int GetHashCode()
        {
            var hash = Op.GetHashCode();

            if (Label is null)
            {
                return hash;
            }

            hash = (hash * 17) + Label.GetHashCode();
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
