using System;

namespace VMCore.VM.Core
{
    [Flags]
    public enum DataAccessType
    {
        Read    = 1 << 0,
        Write   = 1 << 1
    }
}
