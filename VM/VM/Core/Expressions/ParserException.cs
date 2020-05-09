using System;

namespace VMCore.VM.Core.Expressions
{
    public class ParserException : Exception
    {
        public ParserException(string message)
            : base(message)
        {
        }
    }
}
