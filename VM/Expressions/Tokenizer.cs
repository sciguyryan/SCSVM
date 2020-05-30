using System;
using System.Diagnostics;
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
        private readonly string _str;

        /// <summary>
        /// The depth of brackets that have been
        /// parsed. This number should be zero at
        /// the end of parsing. If it isn't
        /// then there was a mismatch and we cannot
        /// guarantee that the data is parsed
        /// as intended.
        /// </summary>
        private int _bracketDepth;

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
                    throw new ExprParserException
                    (
                        "NextToken: bracket mismatch: the " +
                        "number of open and closing brackets did " +
                        "not match."
                    );
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

            // All integer literals must start with a $.
            if (_char == '$')
            {
                HandleIntegerToken();
                return;
            }

            // As we are not a hexadecimal number
            // then we must be a register identifier.
            if (char.IsLetter(_char))
            {
                HandleRegisterToken();
                return;
            }

            throw new ExprParserException
            (
                "NextToken: failed to parse the string - invalid " +
                $"character {_char} was present in the input string."
            );
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
        }

        /// <summary>
        /// Handle the construction of an integer-type token.
        /// </summary>
        private void HandleIntegerToken()
        {
            // Skip past the integer literal marker.
            NextChar();

            var sb = new StringBuilder(64);

            int result;
            bool success;

            var isSigned = false;
            if (_char == '-')
            {
                isSigned = true;
                NextChar();
            }

            var prefix = new StringBuilder(2);
            prefix.Append(_char);
            NextChar();

            // If we have a numeric literal, an "x" (as in 0x)
            // or "b" (as in 0b) then we can add these to the
            // prefix testing string.
            if (IsNumericLiteral(_char) || 
                _char == 'x' || 
                _char == 'b')
            {
                prefix.Append(_char);
                NextChar();
            }
            else
            {
                // We did not have enough valid characters to provide
                // a proper literal prefix.
                sb.Append(prefix);
                prefix.Clear();

                // We do not want to advance to the next character
                // here as in cases like this:
                // $5+$6
                // ... we would be on the addition (+) symbol and that
                // would then break the parsing from there on.
                // If we hit the end of the string (a null character)
                // then we still do not need to advance here as there
                // is nothing more to read anyway.
            }

            while (IsNumericLiteral(_char))
            {
                sb.Append(_char);
                NextChar();
            }

            var prefixStr = prefix.ToString();
            switch (prefixStr)
            {
                case "0b":
                    // A binary literal.
                    success =
                        TryParseBinInt(sb.ToString(), out result);
                    break;

                case "0x":
                    // A hexadecimal literal.
                    success =
                        TryParseHexInt(sb.ToString(), out result);
                    break;

                default:
                {
                    var octalChar = 
                        (prefix.Length > 0) ? prefix[0] : '\0';

                    if (octalChar == '0')
                    {
                        // An octal literal.
                        // We want to take the second character of
                        // the prefix and add it to the start of the
                        // string, otherwise we will be one short.
                        success =
                            TryParseOctInt($"{prefix[1]}{sb}",
                                           out result);
                        break;
                    }

                    // If all else fails, we will try a normal
                    // (decimal) integer parse.
                    // We need to append the prefix back onto the
                    // string here as it was likely not meant
                    // to be a prefix at all.
                    success =
                        TryParseInt($"{prefixStr}{sb}", out result);
                }
                break;
            }

            Debug.WriteLine($"result = {result}");

            // Was the input string a successfully parsed?
            if (!success)
            {
                throw new ExprParserException
                (
                    "HandleNumericToken: failed to parse " +
                    "numerical token from data. Numeric value " +
                    "was invalid."
                );
            }

            Number = (!isSigned) ? result : result * -1;
            Token = Tokens.Number;
        }

        /// <summary>
        /// Attempt to parse the string as a binary integer.
        /// </summary>
        /// <param name="aStr">The string to be parsed.</param>
        /// <param name="aNum">
        /// An integer representing the parsed value.
        /// </param>
        /// <returns>
        /// A boolean, true if parsing the string yielded a
        /// valid integer, false otherwise.
        /// </returns>
        private static bool TryParseBinInt(string aStr, out int aNum)
        {
            try
            {
                aNum = Convert.ToInt32(aStr, 2);
                return true;
            }
            catch
            {
                aNum = 0;
                return false;
            }
        }

        /// <summary>
        /// Attempt to parse the string as an octal integer.
        /// </summary>
        /// <param name="aStr">The string to be parsed.</param>
        /// <param name="aNum">
        /// An integer representing the parsed value.
        /// </param>
        /// <returns>
        /// A boolean, true if parsing the string yielded a
        /// valid integer, false otherwise.
        /// </returns>
        private static bool TryParseOctInt(string aStr, out int aNum)
        {
            try
            {
                aNum = Convert.ToInt32(aStr, 8);
                return true;
            }
            catch
            {
                aNum = 0;
                return false;
            }
        }

        /// <summary>
        /// Attempt to parse the string as a hexadecimal integer.
        /// </summary>
        /// <param name="aStr">The string to be parsed.</param>
        /// <param name="aNum">
        /// An integer representing the parsed value.
        /// </param>
        /// <returns>
        /// A boolean, true if parsing the string yielded a
        /// valid integer, false otherwise.
        /// </returns>
        private static bool TryParseHexInt(string aStr, out int aNum)
        {
            return
                int.TryParse(aStr,
                             NumberStyles.HexNumber,
                             CultureInfo.CurrentCulture,
                             out aNum);
        }

        /// <summary>
        /// Attempt to parse a string as a decimal integer.
        /// </summary>
        /// <param name="aStr">The string to be parsed.</param>
        /// <param name="aNum">
        /// An integer representing the parsed value.
        /// </param>
        /// <returns>
        /// A boolean, true if parsing the string yielded a
        /// valid integer, false otherwise.
        /// </returns>
        private static bool TryParseInt(string aStr, out int aNum)
        {
            return int.TryParse(aStr, out aNum);
        }

        /// <summary>
        /// If the specified character is a valid numeric literal.
        /// This can be any decimal or hexadecimal digit.
        /// </summary>
        /// <param name="aChar">The character to be tested.</param>
        /// <returns>
        /// A boolean, true if the character can represent a valid
        /// numerical literal, false otherwise.
        /// </returns>
        private bool IsNumericLiteral(char aChar)
        {
            return (aChar >= '0' && aChar <= '9') ||
                   (aChar >= 'a' && aChar <= 'f') ||
                   (aChar >= 'A' && aChar <= 'F');
        }
    }
}
