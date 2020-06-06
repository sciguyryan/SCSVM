#nullable enable

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

        public CompilerDir(DirectiveCodes aDirCode,
                        string aDirLabel,
                        byte[]? aByteData = null,
                        string? aStringData = null)
        {
            DirCode = aDirCode;
            DirLabel = aDirLabel;
            ByteData = aByteData ?? new byte[0];
            StringData = aStringData ?? string.Empty;
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
            }
            else
            {
                sb.Append($"({StringData})");
            }

            return sb.ToString();
        }
    }
}
