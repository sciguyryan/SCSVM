#nullable enable

using System;
using System.Collections.Generic;
using VMCore.VM.Core;

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

        public int GetDataSectionLength()
        {
            var size = 0;

            foreach (var data in DataSectionData)
            {
                switch (data.DirCode)
                {
                    case DirectiveCodes.DB:
                        size += data.ByteData.Length;
                        break;

                    case DirectiveCodes.EQU:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return size;
        }
    }
}
