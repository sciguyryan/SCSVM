using System;

namespace VMCore.VM.Core.Exceptions
{
    public class MemoryOutOfRangeException : Exception
    {
        public MemoryOutOfRangeException()
        {
        }

        public MemoryOutOfRangeException(string message)
            : base(message)
        {
        }

        public MemoryOutOfRangeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
