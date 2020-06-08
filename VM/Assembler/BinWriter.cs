using System;
using System.Collections.Generic;
using System.IO;

namespace VMCore.Assembler
{
    public class BinWriter
    {
        /// <summary>
        /// A list of sections to be created within this binary file.
        /// </summary>
        public Dictionary<BinSections, BinSection> Sections { get; set; } = 
            new Dictionary<BinSections, BinSection>();

        /// <summary>
        /// Add a meta data section to the binary file.
        /// </summary>
        /// <param name="aInfo">
        /// The binary meta data to be added to the file.
        /// </param>
        public void AddMeta(BinMeta aInfo)
        {
            var metaSection = CreateSection(BinSections.Meta);
            metaSection.Raw = aInfo.Serialize();
        }

        /// <summary>
        /// Create a section within the binary file.
        /// </summary>
        /// <param name="aSection">
        /// The type of section to be created.
        /// </param>
        /// <returns>
        /// A BinSection object to hold the any required data
        /// for the section.
        /// </returns>
        public BinSection CreateSection(BinSections aSection)
        {
            if (Sections.ContainsKey(aSection))
            {
                return Sections[aSection];
            }

            var s = new BinSection
            {
                //Name = Enum.GetName(typeof(BinSections), aSection)
            };

            Sections.Add(aSection, s);

            return s;
        }

        /// <summary>
        /// Save the compiled binary data into a binary file.
        /// </summary>
        /// <returns>
        /// A byte array containing the data for the binary file.
        /// </returns>
        public byte[] Save()
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            // Our binary magic number.
            bw.Write(BinFile.MagicNumber);

            // The number of sections within this binary file.
            bw.Write(Sections.Count);

            // The raw data for each of the sections.
            foreach (var (_, value) in Sections)
            {
                bw.Write(value.Name);
                bw.Write(value.Raw.Length);
                bw.Write(value.Raw);
            }

            bw.Close();

            return ms.ToArray();
        }
    }
}
