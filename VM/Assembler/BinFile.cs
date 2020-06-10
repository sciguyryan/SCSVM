using System.Collections.Generic;
using System.IO;

namespace VMCore.Assembler
{
    public class BinFile
    {
        #region Public Properties

        // ψ - Psi, Psi-Core, (p)sci-Core. Someone out there will understand!
        /// <summary>
        /// The magic number for the PsiCore binary files.
        /// </summary>
        public static readonly int MagicNumber = 0x03C8;

        /// <summary>
        /// The entry address for this binary file.
        /// </summary>
        public int InitialAddress { get; set; }

        /// <summary>
        /// A list of sections within this binary file.
        /// </summary>
        public Dictionary<BinSections, BinSection> Sections { get; set; } = 
            new Dictionary<BinSections, BinSection>();

        /// <summary>
        /// The raw binary data for this file.
        /// </summary>
        public byte[] Raw { get; set; }

        #endregion // Public Properties

        public BinFile(byte[] aData)
        {
            if (aData.Length == 0)
            {
                throw new InvalidDataException
                (
                    "BinFile: no binary data provided."
                );
            }

            Raw = aData;

            using var br = new BinaryReader(new MemoryStream(aData));

            var magic = br.ReadInt32();
            if (magic != MagicNumber)
            {
                throw new InvalidDataException
                (
                    "BinFile: unrecognized file format."
                );
            }

            // Next we need to read the section information pointer.
            // This will let us correctly decode the sections.
            var infoSectionPrt = br.ReadInt32();

            InitialAddress = br.ReadInt32();

            // Set the stream position to the location as specified
            // by the information section pointer.
            br.BaseStream.Position = infoSectionPrt;

            // The number of sections that this binary file
            // is listed as containing.
            var sectionCount = br.ReadInt32();

            // Iterate over each of the sections that
            // we expect to find.
            var sectionData = new List<SectionInfo>(sectionCount);
            for (var i = 0; i < sectionCount; i++)
            {
                var id = (BinSections)br.ReadInt32();
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

            foreach (var sec in sectionData)
            {
                var sect = new BinSection
                {
                    SectionId = sec.SectionId
                };

                // Set the location of the stream to the pointer
                // specified.
                br.BaseStream.Position = sec.StartPosition;

                // Read the specified block of data.
                sect.Raw = br.ReadBytes(sec.Length);

                // Add section to our collection.
                Sections.Add(sec.SectionId, sect);
            }

            // We cannot have a valid binary with no sections.
            if (Sections.Count == 0)
            {
                throw new InvalidDataException
                (
                    "BinFile: no valid binary data provided."
                );
            }

            br.Close();
        }
    }
}
