#nullable enable

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
            var compSecs = new CompilerSections();
            compSecs.CodeSectionData.AddRange(aIns);

            return
                new Compiler(compSecs, null, aOptimize).Compile();
        }

        public static BinFile CompileToBinFile(CompilerIns[] aIns,
                                               bool aOptimize = false)
        {
            var bytes = RawCompile(aIns, aOptimize);
            return new BinFile(bytes);
        }


        public static byte[] RawCompile(CompilerSections aSecs,
                                        bool aOptimize = false)
        {
            return new Compiler(aSecs, null, aOptimize).Compile();
        }

        public static BinFile CompileToBinFile(CompilerSections aSecs,
                                               bool aOptimize = false)
        {
            var bytes = RawCompile(aSecs, aOptimize);
            return new BinFile(bytes);
        }
    }
}
