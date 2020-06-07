using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace VMCore.Assembler
{
    public class BinFile
    {
        // ψ - Psi, Psi-Core, (p)sci-Core. Someone out there will understand!
        /// <summary>
        /// The magic number for the PsiCore binary files.
        /// </summary>
        public static readonly int MagicNumber = 0x03C8;

        /// <summary>
        /// The meta data section for this binary file.
        /// </summary>
        public BinMeta Meta => 
            BinMeta.Deserialize(this[BinSections.Meta].Raw);

        /// <summary>
        /// A list of sections within this binary file.
        /// </summary>
        public List<BinSection> Sections { get; set; } = 
            new List<BinSection>();

        public BinSection this[BinSections aSection] =>
            // TODO - there probably isn't any need to optimize this...
            // but noting this here just in case.
            (from s in Sections 
                where s.Name == 
                      Enum.GetName(typeof(BinSections), aSection)
                select s).FirstOrDefault();

        public byte[] RawBytes { get; set; }

        /// <summary>
        /// Creates the RawBinaryFile object represented by a byte array.
        /// </summary>
        /// <param name="aData">
        /// A byte array representing a RawBinaryFile object.
        /// </param>
        /// <returns>
        /// A RawBinaryFile containing the deserialized binary data.
        /// </returns>
        public static BinFile Load(byte[] aData)
        {
            using var br = new BinaryReader(new MemoryStream(aData));
            var rbf = new BinFile
            {
                RawBytes = aData
            };

            var magic = br.ReadInt32();
            if (magic != MagicNumber)
            {
                throw new Exception("Load: Unrecognized binary format.");
            }

            var sectionCount = br.ReadInt32();

            for (var i = 0; i < sectionCount; i++)
            {
                var sect = new BinSection
                {
                    Name = br.ReadString()
                };

                var sectionSize = br.ReadInt32();

                sect.EntryPoint = 
                    (int)br.BaseStream.Position;

                sect.Raw = br.ReadBytes(sectionSize);

                rbf.Sections.Add(sect);
            }

            br.Close();

            return rbf;
        }
    }
}
