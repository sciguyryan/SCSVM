namespace VMCore.VM.Core.Breakpoints
{
    /// <summary>
    /// A type of breakpoint to be fired.
    /// </summary>
    public enum BreakpointType
    {
        /// <summary>
        /// Memory read.
        /// </summary>
        MemoryRead,
        /// <summary>
        /// Memory write.
        /// </summary>
        MemoryWrite,
        /// <summary>
        /// When the value of a register is read.
        /// </summary>
        RegisterRead,
        /// <summary>
        /// When the value of a register is changed.
        /// </summary>
        RegisterWrite
    }
}
