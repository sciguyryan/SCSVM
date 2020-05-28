﻿namespace VMCore.VM
{
    public enum SecurityContext
    {
        /// <summary>
        /// Direct execution of a command, usually via the Cpu.
        /// </summary>
        System,
        /// <summary>
        /// Indirect execution of a command, usually via an executed byte code instruction.
        /// </summary>
        User,
    }
}
