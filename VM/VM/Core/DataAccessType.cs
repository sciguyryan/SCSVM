using System;

namespace VMCore.VM.Core
{
    [Flags]
    public enum DataAccessType
    {
        Read,
        Write
    }
}
