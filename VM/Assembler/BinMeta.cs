using System;
using System.IO;

namespace VMCore.Assembler
{
    public class BinMeta
    {
        /// <summary>
        /// The unique identifier for the file.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The binary version of this file.
        /// </summary>
        public Version FileVersion { get; set; }

        /// <summary>
        /// The version of the compiler used to compile this file.
        /// </summary>
        public Version CompilerVersion { get; set; }

        /// <summary>
        /// Deserialize a RawBinaryMeta object from a byte array.
        /// </summary>
        /// <param name="aRaw">
        /// A byte array holding the raw binary data.
        /// </param>
        /// <returns>
        /// A RawBinaryMeta object derived from the byte array.
        /// </returns>
        public static BinMeta Deserialize(byte[] aRaw)
        {
            using var br = new BinaryReader(new MemoryStream(aRaw));
            var res = new BinMeta
            {
                Id = new Guid(br.ReadBytes(16)),

                FileVersion = 
                    new Version(br.ReadInt32(),
                                br.ReadInt32(),
                                br.ReadInt32(),
                                br.ReadInt32()),

                CompilerVersion =
                    new Version(br.ReadInt32(),
                                br.ReadInt32(),
                                br.ReadInt32(),
                                br.ReadInt32()),
            };

            br.Close();

            return res;
        }

        /// <summary>
        /// Serialize a RawBinaryMeta into a byte array.
        /// </summary>
        /// <returns>
        /// A byte array representing the RawBinaryInfo object.
        /// </returns>
        public byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write(Id.ToByteArray());

            // Write the file version to the stream.
            bw.Write(FileVersion.Major);
            bw.Write(FileVersion.Minor);
            bw.Write(FileVersion.Build);
            bw.Write(FileVersion.Revision);

            // Write the compiler version to the stream.
            bw.Write(CompilerVersion.Major);
            bw.Write(CompilerVersion.Minor);
            bw.Write(CompilerVersion.Build);
            bw.Write(CompilerVersion.Revision);

            bw.Close();

            return ms.ToArray();
        }
    }
}
