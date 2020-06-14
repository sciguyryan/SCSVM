#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using VMCore.VM.Core;

namespace VMCore.Assembler
{
    /// <summary>
    /// A compiler directive class primarily for use
    /// with the quick compiler.
    /// </summary>
    public class CompilerDir
    {
        public DirectiveCodes DirCode { get; }

        public string DirLabel { get; }

        public byte[] ByteData { get; }

        public string StringData { get; }

        public string TimesExprString { get; }

        public CompilerDir? SubDirective { get; }

        public CompilerDir(DirectiveCodes aDirCode,
                           string aDirLabel,
                           byte[]? aByteData = null,
                           string? aStringData = null,
                           string? aTimesExprString = null,
                           CompilerDir? aSubDir = null)
        {
            DirCode = aDirCode;
            DirLabel = aDirLabel;
            ByteData = aByteData ?? new byte[0];
            StringData = aStringData ?? string.Empty;
            TimesExprString = aTimesExprString ?? string.Empty;
            SubDirective = aSubDir;

        }

        public override string ToString()
        {
            var sb = new StringBuilder($"{DirCode} '{DirLabel}' Args = ");

            if (ByteData.Length > 0)
            {
                sb.Append("(");

                var len = ByteData.Length;
                for (var i = 0; i < len; i++)
                {
                    sb.Append($"{ByteData[i]:X2}");

                    if (i < len - 1)
                    {
                        sb.Append(", ");
                    }
                }

                sb.Append(")");

                if (TimesExprString != null)
                {
                    sb.Append($" TIMES = '{TimesExprString}'");
                }

                sb.Append($" SUB DIRECTIVE = '{!(SubDirective is null)}'");
            }
            else
            {
                sb.Append($"({StringData})");
            }

            return sb.ToString();
        }

        public override bool Equals(object? aObj)
        {
            return Equals(aObj as CompilerDir);
        }

        public bool Equals([AllowNull] CompilerDir aOther)
        {
            // Check for null and compare run-time types.
            if (aOther is null || GetType() != aOther.GetType())
            {
                return false;
            }

            var q = aOther;

            bool subDirMatch;
            if (SubDirective is null || q.SubDirective is null)
            {
                subDirMatch =
                    (SubDirective is null && q.SubDirective is null);
            }
            else
            {
                subDirMatch = SubDirective == q.SubDirective;
            }

            return
                DirCode == q.DirCode &&
                DirLabel == q.DirLabel &&
                ByteData.SequenceEqual(q.ByteData) &&
                StringData == q.StringData &&
                TimesExprString == q.TimesExprString &&
                subDirMatch;
        }

        public override int GetHashCode()
        {
            var hash = DirCode.GetHashCode();
            hash = (hash * 7) + DirLabel.GetHashCode();
            hash = (hash * 7) + ByteData.Length;
            foreach (var b in ByteData)
            {
                hash *= 7;
                hash += b.GetHashCode();
            }

            hash = (hash * 7) + StringData.GetHashCode();
            hash = (hash * 7) + TimesExprString.GetHashCode();

            if (!(SubDirective is null))
            {
                hash = (hash * 7) + SubDirective.GetHashCode();
            }

            return hash;
        }

        public static bool operator ==(CompilerDir aLeft,
                                       CompilerDir aRight)
        {
            return Equals(aLeft, aRight);
        }

        public static bool operator !=(CompilerDir aLeft,
                                       CompilerDir aRight)
        {
            return !(aLeft == aRight);
        }
    }
}
