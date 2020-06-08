namespace VMCore.VM.Core.Memory
{
    public class MemoryRegion
    {
        /// <summary>
        /// The start position of this memory region.
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// The end position of this memory region.
        /// </summary>
        public int End { get; set; }

        /// <summary>
        /// The access flags for this memory region.
        /// </summary>
        public MemoryAccess Access { get; set; }

        /// <summary>
        /// The unique memory sequence identifier for this memory region.
        /// </summary>
        public int SeqID { get; }

        /// <summary>
        /// The name of this memory region.
        /// </summary>
        public string Name { get; }

        public MemoryRegion(int aStart,
                            int aEnd,
                            MemoryAccess aAccess,
                            int aSeqId,
                            string aName)
        {
            Start = aStart;
            End = aEnd;
            Access = aAccess;
            SeqID = aSeqId;
            Name = aName;
        }
    }
}
