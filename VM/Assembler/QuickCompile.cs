#nullable enable

using System;

namespace VMCore.Assembler
{
    public static class QuickCompile
    {
        /// <summary>
        /// Compile a list of instructions directly into a bytecode array.
        /// </summary>
        /// <param name="aIns">
        /// The list of instruction to be compiled.
        /// </param>
        /// <param name="aOptimize">
        /// A boolean indicating if we should attempt to optimize
        /// the assembled code.
        /// </param>
        /// <returns>
        /// A byte array containing the bytecode data for the program.
        /// </returns>
        public static byte[] RawCompile(CompilerIns[] aIns,
                                        bool aOptimize = false)
        {
            var aw = new AsmWriter(aOptimize);

            foreach (var entry in aIns)
            {
                aw.AddWithLabel(entry.Op, entry.Args, entry.Label);
            }

            return aw.Save();
        }


        public static byte[] RawCompile(CompilerSections aSecs,
                                        bool aOptimize = false)
        {
            return new AsmWriter(null, aSecs, aOptimize).Save();
        }

        /// <summary>
        /// Builds a binary file with the specified parameters.
        /// </summary>
        /// <param name="aSecs">
        /// An array of the sections to be added to the binary.
        /// </param>
        /// <param name="aVersion">The version of the binary.</param>
        /// <returns>
        /// A RawBinaryWriter containing the specified sections.
        /// </returns>
        public static BinWriter BinFileBuilder(BinSections[]? aSecs = null,
                                               Version? aVersion = null)
        {
            var rbw = new BinWriter();
            var rbi = new BinMeta
            {
                Version = aVersion ?? new Version("1.0.0.0"),
                Id = Guid.NewGuid(),
            };

            rbw.AddMeta(rbi);

            // Create all sections by default if none
            // were provided.
            if (aSecs is null || aSecs.Length == 0)
            {
                aSecs =
                    (BinSections[])Enum.GetValues(typeof(BinSections));
            }

            foreach (var s in aSecs)
            {
                _ = rbw.CreateSection(s);
            }

            return rbw;
        }
    }
}
