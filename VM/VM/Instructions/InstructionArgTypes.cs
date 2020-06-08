namespace VMCore.VM.Instructions
{
    public enum InsArgTypes
    {
        /// <summary>
        /// The argument is a register identifier.
        /// </summary>
        Register,
        /// <summary>
        /// The argument is a literal integer.
        /// </summary>
        LiteralInteger,
        /// <summary>
        /// The argument is a literal float.
        /// </summary>
        LiteralFloat,
        /// <summary>
        /// The argument is a register pointer.
        /// </summary>
        RegisterPointer,
        /// <summary>
        /// The argument is a literal pointer.
        /// </summary>
        LiteralPointer,
        /// <summary>
        /// The argument is an expression.
        /// </summary>
        Expression,
        /// <summary>
        /// The argument is a string.
        /// </summary>
        String,
        /// <summary>
        /// An argument indicating the size of the instruction.
        /// </summary>
        InstructionSizeHint
    }
}
