using System;

namespace VMCore.VM.Core.Exceptions
{
    public class RegisterAccessViolationException
        : Exception
    {
        public RegisterAccessViolationException()
        {
        }

        public RegisterAccessViolationException(string aMessage)
            : base(aMessage)
        {
        }

        public RegisterAccessViolationException(string aMessage, Exception aInner)
            : base(aMessage, aInner)
        {
        }
    }
}
