namespace VMCore.VM.Core
{
    public enum Registers : byte
    {
        // TODO - I had initially planned to allow access
        // to the same register types as in Intel processors
        // but it adds overhead and complications that I
        // do not want to deal with for the time being.
        // As such I have commented them out.
        // I might come back along later and work on these.

        // Layout
        // bits  : 8 16 24 32
        // bytes : 1 2  3  4 

        // Bits  : 8 16
        // Bytes : 1  2
        /// <summary>
        /// Data register 1: two highest order bytes.
        /// </summary>
        //AX,
        // Bits  : 16
        // Byte  : 2
        /// <summary>
        /// Data register 1: second highest order byte.
        /// </summary>
        //AH,
        // Bits  : 8
        // Byte  : 1
        /// <summary>
        /// Data register 1 - highest order byte.
        /// </summary>
        //AL,
        ////////////////////////////////////////////////////
        /// <summary>
        /// Data register 1.
        /// </summary>
        R1,
        /// <summary>
        /// Data register 2.
        /// </summary>
        R2,
        /// <summary>
        /// Data register 3.
        /// </summary>
        R3,
        /// <summary>
        /// Data register 4.
        /// </summary>
        R4,
        /// <summary>
        /// Data register 5.
        /// </summary>
        R5,
        /// <summary>
        /// Data register 6.
        /// </summary>
        R6,
        /// <summary>
        /// Data register 7.
        /// </summary>
        R7,
        /// <summary>
        /// Data register 8.
        /// </summary>
        R8,

        /// <summary>
        /// Accumulator register.
        /// </summary>
        AC,

        /// <summary>
        /// Instruction Pointer register.
        /// </summary>
        IP,
        /// <summary>
        /// Stack pointer register.
        /// </summary>
        SP,
        /// <summary>
        /// Flags register.
        /// </summary>
        FL,
        /// <summary>
        /// Program counter register.
        /// </summary>
        PC,
        /// <summary>
        /// A register used for unit testing.
        /// </summary>
        TESTER
    }
}