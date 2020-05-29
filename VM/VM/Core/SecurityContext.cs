namespace VMCore.VM.Core
{
    public enum SecurityContext
    {
        /// <summary>
        /// Direct execution of a command, usually via the CPU.
        /// </summary>
        System,
        /// <summary>
        /// Indirect execution of a command, usually via an
        /// executed byte code instruction.
        /// </summary>
        User,
    }
}
