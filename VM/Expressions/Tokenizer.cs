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
        public float Number { get; private set; }

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


        public Tokenizer(string input)
        {
            _str = input;

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
                    NextChar();
                    Token = Tokens.OpenBracket;
                    return;
                case ')':
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
            var sb = new StringBuilder();

            // Accept letter or digit.
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
            // a float: 1.0101
            var sb = new StringBuilder();
            bool hasDecimalPoint = false;
            while (IsNumericLiteral(_char))
            {
                // If we already have a decimal point,
                // or if we are working with a hex
                // number then we have invalid data
                // and so need to throw an exception here.
                if (_char == '.')
                {
                    if (hasDecimalPoint || isHex)
                    {
                        throw new ParserException("HandleNumericToken: failed to parse numerical token from data.");
                    }

                    hasDecimalPoint = true;
                }

                sb.Append(_char);
                NextChar();
            }

            // An indication on whether the parsing
            // was successful or not.
            bool success;

            float numf;
            if (!isHex)
            {
                // A value is always parsed as a float,
                // even if the result is requested to be
                // an integer. The float is converted
                // to an integer later as needed.
                // This simplifies the parsing logic.
                // If it creates bugs then I will separate
                // it out.
                success = TryParseFloat(sb, out numf);
            }
            else
            {
                // Hex literals can only be parsed
                // as integers.
                success =
                    TryParseHexInt(sb, out int numi);

                numf = (float)numi;
            }

            // We were not able to successfully
            // parse a numeric value. We cannot
            // continue here.
            if (!success)
            {
                throw new ParserException("HandleNumericToken: failed to parse numerical token from data. Numeric value was invalid.");
            }

            Number = numf;
            Token = Tokens.Number;
            return;
        }

        /// <summary>
        /// Attempt to parse the string contained in a stringbuilder
        /// as a float.
        /// </summary>
        /// <param name="sb">The stringbuilder containing the input string.</param>
        /// <param name="numf">A float representing the parsed value.</param>
        /// <returns>A boolean, true if the parsing yielded a valid float, false otherwise.</returns>
        private bool TryParseFloat(StringBuilder sb, out float numf)
        {
            return 
                float.TryParse(sb.ToString(),
                               NumberStyles.Float,
                               CultureInfo.InvariantCulture,
                               out numf);
        }

        /// <summary>
        /// Attempt to parse the string contained in a stringbuilder
        /// as a hexadecimal integer.
        /// </summary>
        /// <param name="sb">The stringbuilder containing the input string.</param>
        /// <param name="numi">An integer representing the parsed value.</param>
        /// <returns>A boolean, true if the parsing yielded a valid integer, false otherwise.</returns>
        private bool TryParseInt(StringBuilder sb, out int numi)
        {
            return 
                int.TryParse(sb.ToString(),
                             out numi);
        }

        /// <summary>
        /// Attempt to parse the string contained in a stringbuilder
        /// as a integer.
        /// </summary>
        /// <param name="sb">The stringbuilder containing the input string.</param>
        /// <param name="numi">An integer representing the parsed value.</param>
        /// <returns>A boolean, true if the parsing yielded a valid integer, false otherwise.</returns>
        private bool TryParseHexInt(StringBuilder sb, out int numi)
        {
            return 
                int.TryParse(sb.ToString(),
                             NumberStyles.HexNumber,
                             CultureInfo.CurrentCulture,
                             out numi);
        }

        /// <summary>
        /// If the specified character is a valid starting numeric identifier.
        /// This can be any decimal digit or a dollar sign ($)
        /// to specify a hex literal.
        /// </summary>
        /// <param name="c">The character to be tested.</param>
        /// <returns>A boolean, true if the character can represent a valid starting numerical literal, false otherwise.</returns>
        private bool IsNumericLiteralStart(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c == '$');
        }

        /// <summary>
        /// If the specified character is a valid numeric literal.
        /// This can be any decimal digit or a period.
        /// </summary>
        /// <param name="c">The character to be tested.</param>
        /// <returns>A boolean, true if the character can represent a valid numerical literal, false otherwise.</returns>
        private bool IsNumericLiteral(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'a' && c <= 'f') ||
                   (c >= 'A' && c <= 'F') ||
                   (c == '.');
        }
    }
}
