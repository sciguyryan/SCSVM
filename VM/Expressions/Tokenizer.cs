using System.Globalization;
using System.Text;

namespace VMCore.Expressions
{
    public class Tokenizer
    {
        /// <summary>
        /// The current token type.
        /// </summary>
        public Tokens Token { get; private set; }

        /// <summary>
        /// A number, if a numeric token type.
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// A register identifier, if a register
        /// token type.
        /// </summary>
        public string Register { get; private set; }

        /// <summary>
        /// The current char within the 
        /// input string.
        /// </summary>
        private char _char;

        /// <summary>
        /// The position of the index within
        /// the input string.
        /// </summary>
        private int _pos;

        /// <summary>
        /// The input string to be tokenized.
        /// </summary>
        private string _str;

        /// <summary>
        /// The depth of brackets that have been
        /// parsed. This number should be zero at
        /// the end of parsing. If it isn't
        /// then there was a mismatch and we cannot
        /// guarantee that the data is parsed
        /// as intended.
        /// </summary>
        private int _bracketDepth = 0;

        public Tokenizer(string aInput)
        {
            _str = aInput;

            NextChar();
            NextToken();
        }

        /// <summary>
        /// Read the next character from the string
        /// and advance the index by one.
        /// </summary>
        private void NextChar()
        {
            _char = (_pos < _str.Length) ? _str[_pos] : '\0';
            ++_pos;
        }

        /// <summary>
        /// Read the next token from the string.
        /// </summary>
        public void NextToken()
        {
            if (_char == '\0')
            {
                if (_bracketDepth != 0)
                {
                    throw new ParserException("NextToken: bracket mismatch: the number of open and closing brackets did not match.");
                }

                Token = Tokens.EOF;
                return;
            }

            while (char.IsWhiteSpace(_char))
            {
                NextChar();
            }

            // Special characters that indicate
            // specific tokens types.
            switch (_char)
            {
                case '(':
                    ++_bracketDepth;
                    NextChar();
                    Token = Tokens.OpenBracket;
                    return;
                case ')':
                    --_bracketDepth;
                    NextChar();
                    Token = Tokens.CloseBracket;
                    return;
                case '+':
                    NextChar();
                    Token = Tokens.Add;
                    return;
                case '-':
                    NextChar();
                    Token = Tokens.Subtract;
                    return;
                case '*':
                    NextChar();
                    Token = Tokens.Multiply;
                    return;
                case '/':
                    NextChar();
                    Token = Tokens.Divide;
                    return;
            }

            // We cannot start a numeric literal
            // with a decimal point.
            if (IsNumericLiteralStart(_char))
            {
                HandleNumericToken();
                return;
            }

            // As we are not a hexadecimal number
            // then we must be a register identifier.
            if (char.IsLetter(_char))
            {
                HandleRegisterToken();
                return;
            }

            throw new ParserException($"NextToken: failed to parse the string - invalid character {_char} was present in the input string.");
        }

        /// <summary>
        /// Handle the construction of a register-type
        /// token.
        /// </summary>
        private void HandleRegisterToken()
        {
            // A register - must only start with a
            // letter.
            var sb = new StringBuilder(64);

            // Accept letters and digits only.
            while (char.IsLetterOrDigit(_char))
            {
                sb.Append(_char);
                NextChar();
            }

            Register = sb.ToString();
            Token = Tokens.Register;
            return;
        }

        /// <summary>
        /// Handle the construction of a
        /// numeric-type token.
        /// </summary>
        private void HandleNumericToken()
        {
            bool isHex = false;
            if (_char == '$')
            {
                isHex = true;

                // Skip the hex marker.
                NextChar();
            }

            // Capture a numeric value.
            // This can be any of the following:
            // a hex number: $EA
            // an integer: 100
            var sb = new StringBuilder(64);
            while (IsNumericLiteral(_char))
            {
                sb.Append(_char);
                NextChar();
            }

            int num;
            var success = (!isHex) ?
                TryParseInt(sb, out num) :
                TryParseHexInt(sb, out num);

            // We were not able to successfully
            // parse a numeric value. We cannot
            // continue here.
            if (!success)
            {
                throw new ParserException("HandleNumericToken: failed to parse numerical token from data. Numeric value was invalid.");
            }

            Number = num;
            Token = Tokens.Number;
            return;
        }

        /// <summary>
        /// Attempt to parse the string contained in a string builder
        /// as a hexadecimal integer.
        /// </summary>
        /// <param name="aSb">The string builder containing the input string.</param>
        /// <param name="aNum">An integer representing the parsed value.</param>
        /// <returns>A boolean, true if the parsing yielded a valid integer, false otherwise.</returns>
        private bool TryParseInt(StringBuilder aSb, out int aNum)
        {
            return 
                int.TryParse(aSb.ToString(),
                             out aNum);
        }

        /// <summary>
        /// Attempt to parse the string contained in a string builder
        /// as a integer.
        /// </summary>
        /// <param name="aSb">The string builder containing the input string.</param>
        /// <param name="aNum">An integer representing the parsed value.</param>
        /// <returns>A boolean, true if the parsing yielded a valid integer, false otherwise.</returns>
        private bool TryParseHexInt(StringBuilder aSb, out int aNum)
        {
            return 
                int.TryParse(aSb.ToString(),
                             NumberStyles.HexNumber,
                             CultureInfo.CurrentCulture,
                             out aNum);
        }

        /// <summary>
        /// If the specified character is a valid starting numeric identifier.
        /// This can be any decimal digit or a dollar sign ($)
        /// to specify a hex literal.
        /// </summary>
        /// <param name="aChar">The character to be tested.</param>
        /// <returns>A boolean, true if the character can represent a valid starting numerical literal, false otherwise.</returns>
        private bool IsNumericLiteralStart(char aChar)
        {
            return (aChar >= '0' && aChar <= '9') ||
                   (aChar == '$');
        }

        /// <summary>
        /// If the specified character is a valid numeric literal.
        /// This can be any decimal or hexadecimal digit.
        /// </summary>
        /// <param name="aChar">The character to be tested.</param>
        /// <returns>A boolean, true if the character can represent a valid numerical literal, false otherwise.</returns>
        private bool IsNumericLiteral(char aChar)
        {
            return (aChar >= '0' && aChar <= '9') ||
                   (aChar >= 'a' && aChar <= 'f') ||
                   (aChar >= 'A' && aChar <= 'F');
        }
    }
}
