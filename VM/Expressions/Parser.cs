namespace VMCore.Expressions
{
    public class Parser
    {
        // TODO - the parser does not handle expressions
        // in the format of 5(1+2). As it stands I do
        // not see a use for it. If that changes then
        // I will add it later.

        /// <summary>
        /// The tokenizer for the parser.
        /// </summary>
        private readonly Tokenizer _tokenizer;

        public Parser(string aInput)
        {
            _tokenizer = new Tokenizer(aInput);
        }

        /// <summary>
        /// Initiate a full parse of the expression.
        /// </summary>
        /// <returns>
        /// A Node object containing the parsed data.
        /// </returns>
        public Node ParseExpression()
        {
            var expr = ParseAddSubtract();

            if (_tokenizer.Token != Tokens.EOF)
            {
                throw new ExprParserException
                (
                    "ParseExpression: unexpected characters at " +
                    "end of expression."
                );
            }

            return expr;
        }

        /// <summary>
        /// Parse an add or subtract section of an expression.
        /// </summary>
        /// <returns>A node containing the tokenized expression.</returns>
        public Node ParseAddSubtract()
        {
            var lhs = ParseMultiplyDivide();

            while (true)
            {
                OpTypes op;
                switch (_tokenizer.Token)
                {
                    case Tokens.Add:
                        op = OpTypes.Add;
                        break;

                    case Tokens.Subtract:
                        op = OpTypes.Subtract;
                        break;

                    default:
                        return lhs;
                }

                _tokenizer.NextToken();
                lhs = new NodeBinary(lhs, ParseMultiplyDivide(), op);
            }
        }

        /// <summary>
        /// Parse an add or subtract section of an expression.
        /// </summary>
        /// <returns>A node containing the tokenized expression.</returns>
        public Node ParseMultiplyDivide()
        {
            var lhs = ParseUnary();

            while (true)
            {
                OpTypes op;
                switch (_tokenizer.Token)
                {
                    case Tokens.Multiply:
                        op = OpTypes.Multiply;
                        break;

                    case Tokens.Divide:
                        op = OpTypes.Divide;
                        break;

                    default:
                        return lhs;
                }

                _tokenizer.NextToken();
                lhs = new NodeBinary(lhs, ParseUnary(), op);
            }
        }

        /// <summary>
        /// Parse a unary (single operand) section of an expression.
        /// </summary>
        /// <returns>A node containing the tokenized expression.</returns>
        public Node ParseUnary()
        {
            while (true)
            {
                switch (_tokenizer.Token)
                {
                    case Tokens.Add:
                        _tokenizer.NextToken();
                        continue;

                    case Tokens.Subtract:
                        _tokenizer.NextToken();
                        return 
                            new NodeUnary(ParseUnary(),
                                          OpTypes.Negate);

                    default:
                        return ParseLeaf();
                }
            }
        }

        /// <summary>
        /// Parse a leaf node (non-operand) section of an expression.
        /// </summary>
        /// <returns>A node containing the tokenized expression.</returns>
        public Node ParseLeaf()
        {
            switch (_tokenizer.Token)
            {
                case Tokens.Number:
                {
                    var node = new NodeNumber(_tokenizer.Number);
                    _tokenizer.NextToken();
                    return node;
                }

                case Tokens.OpenBracket:
                {
                    // Skip the open bracket.
                    _tokenizer.NextToken();
                    var node = ParseAddSubtract();
                    _tokenizer.NextToken();
                    return node;
                }

                default:
                    throw new ExprParserException
                    (
                        "ParseLeaf: unexpected token: " +
                        $"{_tokenizer.Token}."
                    );
            }
        }
    }
}
