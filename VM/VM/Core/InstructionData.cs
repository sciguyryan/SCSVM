using System.Collections.Generic;

namespace VMCore.VM.Core
{
    public class InstructionData
    {
        /// <summary>
        /// A list of the arguments associated with this instruction.
        /// </summary>
        public List<InstructionArg> Args { get; set; } = 
            new List<InstructionArg>();

        /// <summary>
        /// The name of this instruction.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The opcode for this instruction.
        /// </summary>
        public OpCode OpCode { get; set; }

        /// <summary>
        /// The value of the specified argument.
        /// </summary>
        /// <param name="index">The argument index to be returned.</param>
        /// <returns>An object giving the value of the argument.</returns>
        public object this[int index] => 
            Args[index].Value;

        public override string ToString()
        {
            string argString = "";
            foreach (var arg in Args)
            {
                if (!string.IsNullOrEmpty(argString))
                {
                    argString += ", ";
                }

                argString += arg.Value;
            }

            return $"{OpCode} {argString}";
        }
    }
}
