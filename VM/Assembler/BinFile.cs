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
            // TODO - this method needs lots of error checking.
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

            // Next we need to read the section information pointer.
            // This will let us correctly decode the sections.
            var infoSectionPrt = br.ReadInt32();
            
            // Set the stream position to the location as specified
            // by the information section pointer.
            br.BaseStream.Position = infoSectionPrt;

            var sectionCount = br.ReadInt32();
            Debug.WriteLine($"sectionCount = {sectionCount}");

            var sectionData = new List<SectionInfo>();

            for (var i = 0; i < sectionCount; i++)
            {
                var id = (BinSections) br.ReadInt32();
                var startPtr = br.ReadInt32();
                var length = br.ReadInt32();

                if (length == 0)
                {
                    // We are not interested in zero-length
                    // sections.
                    continue;
                }

                var sec = new SectionInfo(id, startPtr, length);
                sectionData.Add(sec);
            }

            Debug.WriteLine("----------------------");
            foreach (var sec in sectionData)
            {
                Debug.WriteLine($"{sec.SectionId} = {sec.StartPosition}, {sec.Length}");

                var sect = new BinSection
                {
                    SectionId = sec.SectionId,
                    EntryPoint = sec.StartPosition
                };

                // Set the location of the stream to the pointer
                // specified.
                var pos = (int)br.BaseStream.Position;
                br.BaseStream.Position = sec.StartPosition;

                // Read the specified block of data.
                sect.Raw = br.ReadBytes(sec.Length);

                // Add section to our collection.
                rbf.Sections.Add(sect);
            }

            br.Close();

            return rbf;
        }

        /// <summary>
        /// Creates the RawBinaryFile object represented by a byte array.
        /// </summary>
        /// <param name="aData">
        /// A byte array representing a RawBinaryFile object.
        /// </param>
        /// <returns>
        /// A RawBinaryFile containing the deserialized binary data.
        /// </returns>
        public static BinFile Load1(byte[] aData)
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
                    //Name = br.ReadString()
                };

                var sectionSize = br.ReadInt32();
                if (sectionSize == 0)
                {
                    // We are not interested in zero-length
                    // sections.
                    continue;
                }

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
