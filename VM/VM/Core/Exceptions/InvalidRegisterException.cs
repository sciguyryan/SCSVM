using System;

namespace VMCore.VM.Core.Exceptions
{
    public class InvalidRegisterException
        : Exception
    {
        public InvalidRegisterException()
            : base()
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
