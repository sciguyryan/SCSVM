﻿using System;

namespace VMCore.VM.Core.Memory
{
    [Flags]
    public enum MemoryAccess
    {
        N  = 1 << 0,
        R  = 1 << 1,
        W  = 1 << 2,
        PR = 1 << 3,
        PW = 1 << 4,
        EX = 1 << 5,
    }
}
