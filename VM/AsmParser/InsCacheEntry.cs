#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VMCore.VM.Instructions;

namespace VMCore.AsmParser
{
    internal class InsCacheEntry
    {
        public string InsAsmName { get; }

        public Type[] ArgTypes { get; }

        public InsArgTypes[] ArgRefTypes { get; }

        public int[] BoundLabelIndices { get; }

        public InsCacheEntry(string aAsmName,
                             Type[] aArgTypes,
                             InsArgTypes[] aRefTypes,
                             int[] aBoundLabelIndices)
        {
            InsAsmName = aAsmName;
            ArgTypes = aArgTypes;
            ArgRefTypes = aRefTypes;
            BoundLabelIndices = aBoundLabelIndices;
        }

        public override bool Equals(object? aObj)
        {
            return Equals(aObj as InsCacheEntry);
        }

        public bool Equals([AllowNull] InsCacheEntry aOther)
        {
            // Check for null and compare run-time types.
            if (aOther is null || GetType() != aOther.GetType())
            {
                return false;
            }

            var p = aOther;

            if (ArgTypes.Length != p.ArgTypes.Length ||
                ArgRefTypes.Length != p.ArgRefTypes.Length)
            {
                return false;
            }

            if (BoundLabelIndices.Length != p.BoundLabelIndices.Length)
            {
                return false;
            }

            return
                InsAsmName == p.InsAsmName &&
                ArgTypes.SequenceEqual(p.ArgTypes) &&
                ArgRefTypes.SequenceEqual(p.ArgRefTypes) &&
                BoundLabelIndices.SequenceEqual(p.BoundLabelIndices);
        }

        public override int GetHashCode()
        {
            var hash = InsAsmName.GetHashCode();

            hash = (hash * 31) + ArgTypes.Length;
            foreach (var a in ArgTypes)
            {
                hash = (hash * 31) + a.GetHashCode();
            }

            hash = (hash * 47) + ArgRefTypes.Length;
            foreach (var b in ArgRefTypes)
            {
                hash = (hash * 47) + b.GetHashCode();
            }

            hash = (hash * 83) + BoundLabelIndices.Length;
            foreach (var c in BoundLabelIndices)
            {
                hash = (hash * 83) + c.GetHashCode();
            }

            return hash;
        }

        public static bool operator ==(InsCacheEntry aLeft,
                                       InsCacheEntry aRight)
        {
            return Equals(aLeft, aRight);
        }

        public static bool operator !=(InsCacheEntry aLeft,
                                       InsCacheEntry aRight)
        {
            return !(aLeft == aRight);
        }
    }
}
