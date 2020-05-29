#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;

namespace VMCore.Assembler
{
    public class AsmLabel : IEquatable<AsmLabel>
    {
        /// <summary>
        /// The name of this label.
        /// </summary>
        public string Name { get;}

        /// <summary>
        /// The argument to which this label is bound.
        /// </summary>
        public int BoundArgumentIndex { get; }

        public AsmLabel(string aName, int aBoundArgIndex)
        {
            Name = aName;
            BoundArgumentIndex = aBoundArgIndex;
        }

        public override bool Equals(object? aObj)
        {
            return Equals(aObj as AsmLabel);
        }

        public bool Equals([AllowNull] AsmLabel aOther)
        {
            // Check for null and compare run-time types.
            if (aOther is null || GetType() != aOther.GetType())
            {
                return false;
            }

            var label = aOther;

            return
                label.Name == Name &&
                label.BoundArgumentIndex == BoundArgumentIndex;
        }

        public override int GetHashCode()
        {
            return Tuple.Create(Name, BoundArgumentIndex).GetHashCode();
        }

        public static bool operator ==(AsmLabel aLeft,
                                       AsmLabel aRight)
        {
            return Equals(aLeft, aRight);
        }

        public static bool operator !=(AsmLabel aLeft,
                                       AsmLabel aRight)
        {
            return !(aLeft == aRight);
        }
    }
}
