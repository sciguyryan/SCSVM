using System;

namespace VMCore.VM.Core.Exceptions
{
    public class MemoryAccessViolationException : Exception
    {
        public MemoryAccessViolationException()
        {
        }

        public MemoryAccessViolationException(string message)
            : base(message)
        {
        }

        public MemoryAccessViolationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
