using System;

namespace VMCore.Expressions
{
    public class ExprParserException
        : Exception
    {
        public ExprParserException(string aMessage)
            : base(aMessage)
        {
        }
    }
}
