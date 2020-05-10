namespace VMCore.VM.Core
{
    public class InstructionArg
    {
        public enum AsmArgType
        {
            Register,
            Literal,
            Label,
            Special
        }

        /// <summary>
        /// The type of this instruction argument.
        /// </summary>
        public AsmArgType Type { get; set; }

        /// <summary>
        /// The value of this instruction argument.
        /// </summary>
        public object Value { get; set; }
    }
}
