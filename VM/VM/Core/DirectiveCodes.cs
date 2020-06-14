namespace VMCore.VM.Core
{
    public enum DirectiveCodes : int
    {
        /// <summary>
        /// Define Byte(s) - define a sequence of bytes within
        /// the output file.
        /// </summary>
        DB,
        /// <summary>
        /// Equals - evaluate an expression at compile time.
        /// </summary>
        EQU,
        /// <summary>
        /// Times - execute a permitted directive multiple times.
        /// </summary>
        TIMES
    }
}
