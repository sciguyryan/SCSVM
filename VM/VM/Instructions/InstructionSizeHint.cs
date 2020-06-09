namespace VMCore.VM.Instructions
{
    /// <summary>
    /// A size hint indicator for an opcode instruction.
    /// </summary>
    public enum InstructionSizeHint : byte
    {
        /// <summary>
        /// Instruction data should be of size: 1 byte (4 bits)
        /// </summary>
        BYTE,
        /// <summary>
        /// Instruction data should be of size: 4 bytes (32 bits)
        /// </summary>
        WORD,
        /// <summary>
        /// Instruction data should be of size: 8 bytes (64 bits)
        /// </summary>
        DWORD
    }
}
