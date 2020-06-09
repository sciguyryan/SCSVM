#nullable enable

namespace VMCore.Assembler
{
    public static class QuickCompile
    {
        /// <summary>
        /// Compile a list of instructions into a raw binary
        /// bytecode array.
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

        /// <summary>
        /// Compile a list of instructions into a binary file.
        /// </summary>
        /// <param name="aIns">
        /// The list of instruction to be compiled.
        /// </param>
        /// <param name="aOptimize">
        /// A boolean indicating if we should attempt to optimize
        /// the assembled code.
        /// </param>
        /// <returns>
        /// A binary file containing the compiled data.
        /// </returns>
        public static BinFile CompileToBinFile(CompilerIns[] aIns,
                                               bool aOptimize = false)
        {
            var bytes = RawCompile(aIns, aOptimize);
            return new BinFile(bytes);
        }

        /// <summary>
        /// Compile a list of compiler sections into a raw binary
        /// bytecode array.
        /// </summary>
        /// <param name="aSecs">
        /// The list of compiler sections to be compiled.
        /// </param>
        /// <param name="aOptimize">
        /// A boolean indicating if we should attempt to optimize
        /// the assembled code.
        /// </param>
        /// <returns>
        /// A byte array containing the bytecode data for the program.
        /// </returns>
        public static byte[] RawCompile(CompilerSections aSecs,
                                        bool aOptimize = false)
        {
            return new Compiler(aSecs, null, aOptimize).Compile();
        }

        /// <summary>
        /// Compile a list of instructions into a binary file.
        /// </summary>
        /// <param name="aSecs">
        /// The list of compiler sections to be compiled.
        /// </param>
        /// <param name="aOptimize">
        /// A boolean indicating if we should attempt to optimize
        /// the assembled code.
        /// </param>
        /// <returns>
        /// A binary file containing the compiled data.
        /// </returns>
        public static BinFile CompileToBinFile(CompilerSections aSecs,
                                               bool aOptimize = false)
        {
            var bytes = RawCompile(aSecs, aOptimize);
            return new BinFile(bytes);
        }
    }
}
