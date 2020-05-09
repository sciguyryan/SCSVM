using System;
using System.IO;

namespace VMCore.Assembler
{
    public class RawBinaryMeta
    {
        /// <summary>
        /// The binary version of this meta data section.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// The unique identifier for this meta data section.
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Deserialize a RawBinaryMeta object from a byte array.
        /// </summary>
        /// <param name="raw">A byte array holding the raw binary data.</param>
        /// <returns>A RawBinaryMeta object derived from the byte array.</returns>
        public static RawBinaryMeta Deserialize(byte[] raw)
        {
            using var br = new BinaryReader(new MemoryStream(raw));
            var res = new RawBinaryMeta
            {
                ID = new Guid(br.ReadBytes(16)),
                Version = new Version(br.ReadString())
            };

            br.Close();

            return res;
        }

        /// <summary>
        /// Serialize a RawBinaryMeta into a byte array.
        /// </summary>
        /// <returns>A byte array representing the RawBinaryInfo object.</returns>
        public byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            bw.Write(ID.ToByteArray());
            bw.Write(Version.ToString());
            bw.Close();

            return ms.ToArray();
        }
    }
}