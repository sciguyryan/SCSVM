namespace VMCore.VM.Core.Expressions
{
    public class Parser
    {
        public bool IsSimple { get; private set; } = true;

        private Tokenizer _tokenizer;

        public Parser(string input)
        {
            _tokenizer = new Tokenizer(input);
        }

        public Node ParseExpression()
        {
            var expr = ParseAddSubtract();

            if (_tokenizer.Token != Tokens.EOF)
            {
                throw new ParserException("ParseExpression: unexpected characters at end of expression.");
            }

            return expr;
        }

        public Node ParseAddSubtract()
        {
            var lhs = ParseMultiplyDivide();

            while (true)
            {
                OpTypes op;
                if (_tokenizer.Token == Tokens.Add)
                {
                    op = OpTypes.Add;
                }
                else if (_tokenizer.Token == Tokens.Subtract)
                {
                    op = OpTypes.Subtract;
                }
                else
                {
                    return lhs;
                }

                _tokenizer.NextToken();

                lhs = new NodeBinary(lhs, ParseMultiplyDivide(), op);
            }
        }

        public Node ParseMultiplyDivide()
        {
            var lhs = ParseUnary();

            while (true)
            {
                OpTypes op;
                if (_tokenizer.Token == Tokens.Multiply)
                {
                    op = OpTypes.Multiply;
                }
                else if (_tokenizer.Token == Tokens.Divide)
                {
                    op = OpTypes.Divide;
                }
                else
                {
                    return lhs;
                }

                _tokenizer.NextToken();

                lhs = new NodeBinary(lhs, ParseUnary(), op);
            }
        }

        public Node ParseUnary()
        {
            while (true)
            {
                if (_tokenizer.Token == Tokens.Add)
                {
                    _tokenizer.NextToken();
                    continue;
                }

                if (_tokenizer.Token == Tokens.Subtract)
                {
                    _tokenizer.NextToken();

                    return new NodeUnary(ParseUnary(), OpTypes.Negate);
                }

                return ParseLeaf();
            }
        }

        public Node ParseLeaf()
        {
            if (_tokenizer.Token == Tokens.Number)
            {
                var node = new NodeNumber(_tokenizer.Number);
                _tokenizer.NextToken();
                return node;
            }

            if (_tokenizer.Token == Tokens.OpenBracket)
            {
                // Skip the open bracket.
                _tokenizer.NextToken();

                var node = ParseAddSubtract();

                // Check for the close bracket
                // throw an exception if not found.
                if (_tokenizer.Token != Tokens.CloseBracket)
                {
                    throw new ParserException("ParseLeaf: missing close parenthesis!");
                }

                _tokenizer.NextToken();

                return node;
            }

            if (_tokenizer.Token == Tokens.Register)
            {
                // This is not a simple expression
                // so we cannot resolve it immediately.
                IsSimple = false;

                var name = _tokenizer.Register;
                _tokenizer.NextToken();

                 return new NodeRegister(name);
            }

            throw new ParserException($"ParseLeaf: unexpected token: {_tokenizer.Token}.");
        }
    }
}
