using System;

namespace VMCore.VM.Core
{
    [Flags]
    public enum CPUFlags
    {
        /// <summary>
        /// The sign flag - set to true if the result of an operation is negative.
        /// </summary>
        S = 1 << 0,
        /// <summary>
        /// The zero flag - set to true if the result of an operation is zero.
        /// </summary>
        Z = 1 << 1,
        /// <summary>
        /// The overflow flag - set to true if the result of an operation overflowed.
        /// </summary>
        O = 1 << 2,
        /// <summary>
        /// The carry flag - set to true if the carry bit has been set by an operation.
        /// </summary>
        C = 1 << 3
    }
}
