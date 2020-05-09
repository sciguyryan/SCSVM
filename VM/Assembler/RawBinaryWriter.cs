using System;
using System.Collections.Generic;
using System.IO;

namespace VMCore.Assembler
{
    public class RawBinaryWriter
    {
        public Dictionary<RawBinarySections, RawBinarySection> Sections { get; set; } = 
            new Dictionary<RawBinarySections, RawBinarySection>();

        public void AddMeta(RawBinaryMeta info)
        {
            var metaSection = CreateSection(RawBinarySections.Metadata);
            metaSection.Raw = info.Serialize();
        }

        public RawBinarySection CreateSection(RawBinarySections section)
        {
            if (Sections.ContainsKey(section))
            {
                return Sections[section];
            }

            var s = new RawBinarySection
            {
                Name = Enum.GetName(typeof(RawBinarySections), section)
            };

            Sections.Add(section, s);

            return s;
        }

        public byte[] Save()
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            // Our binary magic number.
            bw.Write(RawBinaryFile.MagicNumber);

            // The number of sections within this binary file.
            bw.Write(Sections.Count);

            // The raw data for each of the sections.
            foreach (var kvp in Sections)
            {
                bw.Write(kvp.Value.Name);
                bw.Write(kvp.Value.Raw.Length);
                bw.Write(kvp.Value.Raw);
            }

            bw.Close();

            return ms.ToArray();
        }
    }
}