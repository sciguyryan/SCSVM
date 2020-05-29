using System;

namespace VMCore.VM.Core.Exceptions
{
    public class MemoryOutOfRangeException
        : Exception
    {
        public MemoryOutOfRangeException(string aMessage)
            : base(aMessage)
        {
        }

        public MemoryOutOfRangeException(string aMessage, Exception aInner)
            : base(aMessage, aInner)
        {
        }
    }
}
