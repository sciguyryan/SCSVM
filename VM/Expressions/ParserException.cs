using System;

namespace VMCore.Expressions
{
    public class ParserException : Exception
    {
        public ParserException(string message)
            : base(message)
        {
        }
    }
}
