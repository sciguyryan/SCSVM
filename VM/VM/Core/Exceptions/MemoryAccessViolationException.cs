using System;

namespace VMCore.VM.Core.Exceptions
{
    public class MemoryAccessViolationException
        : Exception
    {
        public MemoryAccessViolationException()
        {
        }

        public MemoryAccessViolationException(string aMessage)
            : base(aMessage)
        {
        }

        public MemoryAccessViolationException(string aMessage, Exception aInner)
            : base(aMessage, aInner)
        {
        }
    }
}
