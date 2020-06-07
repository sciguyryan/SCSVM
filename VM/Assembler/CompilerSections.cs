#nullable enable

using System.Collections.Generic;

namespace VMCore.Assembler
{
    public class CompilerSections
    {
        public List<CompilerIns> CodeSectionData { get; set; }

        public List<CompilerDir> DataSectionData { get; set; }

        public CompilerSections()
        {
            CodeSectionData = new List<CompilerIns>();
            DataSectionData = new List<CompilerDir>();
        }
    }
}
