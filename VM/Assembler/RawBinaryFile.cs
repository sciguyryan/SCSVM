using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace VMCore.Assembler
{
    public class RawBinaryFile
    {
        // ψ - Psi, Psi-Core, (p)sci-Core. Someone out there will understand!
        /// <summary>
        /// The magic number for the PsiCore binary files.
        /// </summary>
        public static readonly int MagicNumber = 0x03C8;

        public RawBinaryMeta Meta => 
            RawBinaryMeta.Deserialize(this[RawBinarySections.Metadata].Raw);

        public List<RawBinarySection> Sections { get; set; } = 
            new List<RawBinarySection>();

        public RawBinarySection this[RawBinarySections section]
        {
            // TODO - there probably isn't any need to optimize this...
            // but noting this here just in case.
            get
            {
                return (from s in Sections 
                        where s.Name == Enum.GetName(typeof(RawBinarySections), section) 
                        select s).FirstOrDefault();
            }
            set { }
        }

        /// <summary>
        /// Creates the RawBinaryFile object represented by a byte array.
        /// </summary>
        /// <param name="data">A byte array representing a RawBinaryFile object.</param>
        /// <returns>A RawBinaryFile containing the deserialized binary data.</returns>
        public static RawBinaryFile Load(byte[] data)
        {
            using var br = new BinaryReader(new MemoryStream(data));
            var rbf = new RawBinaryFile();

            var magic = br.ReadInt32();
            if (magic != RawBinaryFile.MagicNumber)
            {
                throw new Exception("Load: Unrecognized binary format.");
            }

            var sectionCount = br.ReadInt32();
            for (var i = 0; i < sectionCount; i++)
            {
                var sect = new RawBinarySection
                {
                    Name = br.ReadString()
                };

                var sectionSize = br.ReadInt32();
                sect.Raw = br.ReadBytes(sectionSize);

                rbf.Sections.Add(sect);
            }

            br.Close();

            return rbf;
        }
    }
}