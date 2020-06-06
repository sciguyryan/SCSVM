#nullable enable

using VMCore.VM.Core;

namespace VMCore.Assembler
{
    /// <summary>
    /// A simple directive class primarily for use
    /// with the quick compiler.
    /// </summary>
    public class QuickDir
    {
        public DirectiveCodes DirCode { get; }

        public byte[] ByteData { get; }

        public string DataString { get; }

        public QuickDir(DirectiveCodes aDirCode,
                        byte[]? aByteData = null,
                        string? aDataString = null)
        {
            DirCode = aDirCode;
            ByteData = aByteData ?? new byte[0];
            DataString = aDataString ?? string.Empty;
        }
    }
}
