using System;

namespace VMCore.VM.Core.Mem
{
    public class MemoryRegion
    {
        public int Start { get; private set; }

        public int End { get; private set; }

        public MemoryAccess Access { get; set; }

        public MemoryRegion(int start, int end, MemoryAccess access)
        {
            Start = start;
            End = end;
            Access = access;
        }
    }
}
