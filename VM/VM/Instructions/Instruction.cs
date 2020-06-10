#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Instructions
{
    public abstract class Instruction
    {
        /// <summary>
        /// If the ToString methods should display literal values
        /// in hexadecimal.
        /// </summary>
        public bool OutputLiteralsAsHex = true;

        /// <summary>
        /// The security context to be used when executing this
        /// instruction. Defaults to user.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SecurityContext GetSecurityContext()
        {
            return SecurityContext.User;
        }

        /// <summary>
        /// The list of the argument types expected for this instruction.
        /// </summary>
        public abstract Type[] ArgumentTypes { get; }

        /// <summary>
        /// The list of the argument reference type specifiers for
        /// this instruction.
        /// </summary>
        public abstract InsArgTypes[] ArgumentRefTypes { get; }

        /// <summary>
        /// The number of bytes required to store the arguments.
        /// Note that if the argument is a string type then this
        /// will not give the correct result.
        /// </summary>
        public int ArgumentByteSize
        {
            get
            {
                // We do not want to have to calculate this every time
                // we access this property.
                if (_argumentByteSize == -1)
                {
                    UpdateArgumentSizeCache();
                }

                return _argumentByteSize;
            }
        }

        /// <summary>
        /// The opcode for this specific function.
        /// </summary>
        public abstract OpCode OpCode { get; }

        /// <summary>
        /// The assembly command name for this instruction.
        /// </summary>
        public abstract string AsmName { get; }

        /// <summary>
        /// Executes a given instruction within a given CPU instance.
        /// </summary>
        /// <param name="aData">
        /// The data associated with this instruction.
        /// </param>
        /// <param name="aCpu">
        /// The CPU that will be executing the command.
        /// </param>
        /// <returns>
        /// True to indicate that the machine should halt execution,
        /// false otherwise.
        /// </returns>
        /// <remarks>
        /// CPU registers are stored as a byte, however they must be cast
        /// to an integer before being cast to the Registers enum type,
        /// otherwise it will fail.
        /// </remarks>
        public abstract bool Execute(InstructionData aData, Cpu aCpu);

        /// <summary>
        /// Provided the assembly textual command associated with a given
        /// byte code instruction.
        /// </summary>
        /// <param name="aData">
        /// The data associated with this instruction.
        /// </param>
        /// <returns>A string giving the assembly command.</returns>
        /// <remarks>
        /// CPU registers are stored as a byte, however they must be cast
        /// to an integer before being cast to the Registers enum type,
        /// otherwise it will fail.
        /// </remarks>
        public abstract string ToString(InstructionData aData);

        /// <summary>
        /// Updates the CPU operation result flags based on the
        /// result of a calculation.
        /// </summary>
        /// <param name="aCpu">
        /// The CPU that will be executing the assembly command.
        /// </param>
        /// <param name="aResult">
        /// The result of the calculation performed.
        /// </param>
        /// <param name="aOverflow">
        /// A boolean, true if the results of the calculation overflowed
        /// the type bounds, false otherwise.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UpdateCalculationFlags(Cpu aCpu,
                                              long aResult,
                                              bool aOverflow = false)
        {
            // We need to cast to int here
            // as when things overflow we need
            // to be sure that the flags are
            // correctly set.
            aCpu.SetResultFlagPair((int)aResult);

            aCpu.SetFlagState(CpuFlags.O, aOverflow);
        }

        /// <summary>
        /// Calculate the total size of the arguments in bytes
        /// based on the expected types provided.
        /// </summary>
        private void UpdateArgumentSizeCache()
        {
            foreach (var t in ArgumentTypes)
            {
                if (t.IsEnum)
                {
                    // Enumerations are an exception here as
                    // they cannot be created by directly by
                    // CreateInstance.
                    // Since we know it's type then we can
                    // easily account for this.
                    _argumentByteSize += t switch
                    {
                        { } when t == typeof(Registers) 
                            => sizeof(Registers),

                        _
                            => throw new NotSupportedException
                            (
                                $"ArgumentByteSize: the type {t} " +
                                "was passed as an argument type, but no " +
                                "support has been provided for that type."
                            ),
                    };
                }
                else if (t == typeof(string))
                {
                    // We cannot calculate this here and it will need to
                    // be calculated later.
                    continue;
                }
                else
                {
                    // We cannot use sizeof() here as the type
                    // cannot be determined at compile time.
                    var i = Activator.CreateInstance(t);
                    if (i is null)
                    {
                        continue;
                    }

                    _argumentByteSize += Marshal.SizeOf(i);
                }

                // As the value is defaulted to -1 we need
                // to add one here to give the correct value.
                ++_argumentByteSize;
            }
        }

        /// <summary>
        /// The cached number of bytes required to store the arguments.
        /// </summary>
        internal int _argumentByteSize = -1;
    }
}
