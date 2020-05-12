using System;

namespace VMCore.VM.Core.Mem
{
    public class MemoryRegion
    {
        public int Start { get; private set; }

        public int End { get; private set; }

        public MemoryAccess Access { get; set; }

        public int SeqID { get; private set; }

        public MemoryRegion(int aStart,
                            int aEnd,
                            MemoryAccess aAccess,
                            int aSeqID)
        {
            Start = aStart;
            End = aEnd;
            Access = aAccess;
            SeqID = aSeqID;
        }
    }
}
