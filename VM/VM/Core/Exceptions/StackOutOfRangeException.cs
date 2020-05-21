using System;

namespace VMCore.VM.Core.Exceptions
{
    public class StackOutOfRangeException
        : Exception
    {
        public StackOutOfRangeException()
        {
        }

        public StackOutOfRangeException(string aMessage)
            : base(aMessage)
        {
        }

        public StackOutOfRangeException(string aMessage, Exception aInner)
            : base(aMessage, aInner)
        {
        }
    }
}
