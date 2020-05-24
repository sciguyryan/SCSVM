using System;

namespace VMCore.AsmParser
{
    public class AsmParserException
        : Exception
    {
        public AsmParserException(string aMessage)
            : base(aMessage)
        {
        }
    }
}
