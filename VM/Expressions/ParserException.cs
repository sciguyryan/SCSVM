using System;

namespace VMCore.Expressions
{
    public class ParserException
        : Exception
    {
        public ParserException(string aMessage)
            : base(aMessage)
        {
        }
    }
}
