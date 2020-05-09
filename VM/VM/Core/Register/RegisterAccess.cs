using System;

namespace VMCore.VM.Core.Reg
{
    [Flags]
    public enum RegisterAccess
    {
        N  = 0,
        R  = 1 << 0,
        W  = 1 << 1,
        PR = 1 << 2,
        PW = 1 << 3
    }
}
