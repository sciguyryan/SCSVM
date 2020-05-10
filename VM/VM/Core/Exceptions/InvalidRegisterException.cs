using System;

namespace VMCore.VM.Core.Exceptions
{
    public class InvalidRegisterException
        : Exception
    {
        public InvalidRegisterException()
        {
        }

        public InvalidRegisterException(string aMessage)
            : base(aMessage)
        {
        }

        public InvalidRegisterException(string aMessage, Exception aInner)
            : base(aMessage, aInner)
        {
        }
    }
}
