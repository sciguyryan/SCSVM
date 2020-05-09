using System;

namespace VMCore.VM.Core.Exceptions
{
    public class RegisterAccessViolationException : Exception
    {
        public RegisterAccessViolationException()
        {
        }

        public RegisterAccessViolationException(string message)
            : base(message)
        {
        }

        public RegisterAccessViolationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
