using System;

namespace VMCore.VM.Core.Exceptions
{
    public class InvalidRegisterException : Exception
    {
        public InvalidRegisterException()
        {
        }

        public InvalidRegisterException(string message)
            : base(message)
        {
        }

        public InvalidRegisterException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
