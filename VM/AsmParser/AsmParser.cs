#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using VMCore.Assembler;
using VMCore.VM.Core.Utilities;

namespace VMCore.AsmParser
{
    public class AsmParser
    {
        #region Public Properties

        public static readonly char LineContinuation = '\\';

        /// <summary>
        /// A list of exception IDs used by this class.
        /// </summary>
        public enum ExIDs
        {
            MismatchedBrackets,
            InvalidBracketPosition,
            MismatchedString,
            InvalidLabel,
            InvalidSubroutineLabel,
            InvalidIntLiteral,
            InvalidByteLiteral,
            InvalidRegisterIdentifier,
            InvalidArgumentType,
            InvalidInstruction,
            InvalidSectionIdentifier,
            InvalidExpression,
            InvalidDirectiveType,
            NestedSubQuery,
        };

        #endregion // Public Properties

        #region Private Properties

        #region EXCEPTIONS

        /// <summary>
        /// A list of the exception messages used by this class.
        /// </summary>
        private static readonly Dictionary<ExIDs, string> _exMessages =
            new Dictionary<ExIDs, string>()
        {
            {
                ExIDs.MismatchedBrackets,
                "An unmatched bracket was identified on line {0}, " +
                "position {1}. The data could not be parsed."
            },
            {
                ExIDs.InvalidBracketPosition,
                "A bracket was detected at an invalid position " +
                "on line {0}, position {1}. The data could not " +
                "be parsed."
            },
            {
                ExIDs.MismatchedString,
                "An unmatched string was identified on line {0}, " +
                "position {1}. The data could not be parsed."
            },
            {
                ExIDs.InvalidLabel,
                "Attempted to parse a label with an invalid or " +
                "missing identifier."
            },
            {
                ExIDs.InvalidSubroutineLabel,
                "Attempted to parse a subroutine with an invalid " +
                "identifier '{0}'."
            },
            {
                ExIDs.InvalidIntLiteral,
                "Failed to parse an integer literal. " +
                "Value = '{0}', AllowNegative = {1}."
            },
            {
                ExIDs.InvalidByteLiteral,
                "Failed to parse a byte literal. Value = '{0}'"
            },
            {
                ExIDs.InvalidRegisterIdentifier,
                "The string was not a valid register identifier: '{0}'."
            },
            {
                ExIDs.InvalidArgumentType,
                "The argument type for the string could not be " +
                "determined. String = '{0}'."
            },
            {
                ExIDs.InvalidInstruction,
                "No matching instruction was found for the input " +
                "string.\nInstruction = '{0}'\nArguments = {1}\n" +
                "Argument Types = {2}\nArgument Ref Types = {3}."
            },
            {
                ExIDs.InvalidSectionIdentifier,
                "Attempted to parse a line without a valid section " +
                "identifier.\nLine = '{0}'."
            },
            {
                ExIDs.InvalidExpression,
                "The specified expression was invalid. Expression = '{0}'."
            },
            {
                ExIDs.InvalidDirectiveType,
                "No valid directive type was identified."
            },
            {
                ExIDs.NestedSubQuery,
                "Nested sub queries are not supported."
            }
        };

        #endregion // EXCEPTIONS

        /// <summary>
        /// The parser used for parsing compiler directives.
        /// </summary>
        private readonly AsmDirectiveParser _dirParser = 
            new AsmDirectiveParser();

        /// <summary>
        /// The parser used for parsing instructions.
        /// </summary>
        private readonly AsmInstructionParser _insParser =
            new AsmInstructionParser();

        #endregion // Private Properties

        /// <summary>
        /// Parse an input string into an array of instructions.
        /// </summary>
        /// <param name="aInput">The input string to be parsed.</param>
        /// <returns>
        /// An array of instructions representing the input data.
        /// </returns>
        public CompilerSections Parse(string aInput)
        {
            var newLineSkip =
                Environment.NewLine.Length - 1;

            var lineNo = 0;
            var isLine = false;
            var inString = false;
            BinSections? section = null;

            var compSec = new CompilerSections();

            var skipChars = 0;
            var startPos = 0;
            var endPos = 0;
            var span = new ReadOnlySpan<char>(aInput.ToCharArray());

            var len = span.Length;
            for (var i = 0; i < len; i++)
            {
                var c = span[i];

                // Do we have a string?
                if (c == '"')
                {
                    // Yes, ensure that new lines are not treated as
                    // separators while we are within it.
                    inString = !inString;
                }

                // Do we have a line delimiter?
                if (!inString && c == '\n')
                {
                    // Was the last character before the new line
                    // break a line continuation operator?
                    // If so we do not want to perform the line break
                    // here.
                    var contPos = i - newLineSkip - 1;
                    if (contPos < 0 || span[contPos] != '\\')
                    {
                        // Yes, update the end point of our range
                        // to account for it.
                        endPos = i - 1;
                        skipChars = newLineSkip;
                        isLine = true;
                    }
                }

                // Are we are the end of the string?
                if (i == len - 1)
                {
                    // Always ensure that we take the contents of the
                    // last line.
                    endPos = i + 1;
                    isLine = true;
                }

                // We do not have a complete line yet.
                // Move on to the next character.
                if (!isLine)
                {
                    continue;
                }

                // We should not be attempting to push a line
                // that has an unmatched string.
                Assert(inString,
                       ExIDs.MismatchedString,
                       lineNo,
                       i);

                // We have a line.
                // Pass the span into the next stage of the parser.
                if (span[startPos] == '.')
                {
                    // The line is a section identifier.
                    section = 
                        ParseSectionLine(span[startPos..endPos]);
                }
                else
                {
                    // Parse a line based on the type
                    // of section we are currently within.
                    ParseLineByType(section,
                                    span[startPos..endPos],
                                    ref compSec);
                }

                startPos = i + skipChars;
                isLine = false;
                ++lineNo;
            }

            return compSec;
        }

        /// <summary>
        /// Parse a line of assembly depending on the section
        /// in which it originated.
        /// </summary>
        /// <param name="aSec">
        /// The section in which this instruction belongs.
        /// For example instructions will always be found
        /// within the Text section.
        /// </param>
        /// <param name="aLine">The string to be parsed.</param>
        /// <param name="aCompSec">
        /// The binary section in which this line resides.
        /// </param>
        private void ParseLineByType(BinSections? aSec,
                                     ReadOnlySpan<char> aLine,
                                     ref CompilerSections aCompSec)
        {
            // If the line is empty then we have nothing to do
            // here.
            if (aLine.Length == 0)
            {
                return;
            }

            // Without a valid section indicator we
            // cannot be sure how to parse the line.
            if (aSec is null)
            {
                Assert(true,
                       ExIDs.InvalidSectionIdentifier,
                       aLine.ToString());
                return;
            }

            // Which type of section are we parsing?
            switch (aSec)
            {
                case BinSections.Meta:
                    return;

                case BinSections.Text:
                    {
                        var ins = _insParser.ParseLine(aLine);
                        if (!(ins is null))
                        {
                            aCompSec.CodeSectionData.Add(ins);
                        }
                        
                        break;
                    }

                case BinSections.Data:
                    {
                        var dir = _dirParser.ParseDirLine(aLine);
                        if (!(dir is null))
                        {
                            aCompSec.DataSectionData.Add(dir);
                        }
                        
                        break;
                    }

                case BinSections.RData:
                    throw new NotImplementedException();

                case BinSections.BSS:
                    throw new NotImplementedException();

                case BinSections.SectionInfoData:
                    return;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Section Line Parsing

        /// <summary>
        /// Parse a section identifier line.
        /// </summary>
        /// <param name="aLine">The string to be parsed.</param>
        /// <returns>
        /// A nullable BinSections object representing the parsed data.
        /// This can be null if there was no data to output.
        /// </returns>
        private BinSections? ParseSectionLine(ReadOnlySpan<char> aLine)
        {
            var len = aLine.Length;
            var buffer = new StringBuilder(len);
            var sectionId = string.Empty;

            var hasSeparator = false;
            var skipNext = false;
            var skipUntilEnd = false;

            for (var i = 0; i <= len; i++)
            {
                if (i == len)
                {
                    if (buffer.Length > 0)
                    {
                        sectionId = buffer.ToString();
                    }
                    break;
                }

                var c = aLine[i];
                switch (c)
                {
                    case ';':
                        skipUntilEnd = true;
                        break;

                    case { } when c == LineContinuation:
                        skipNext = true;
                        break;

                    case { } when char.IsWhiteSpace(c):
                        hasSeparator = true;
                        skipNext = true;
                        break;
                }

                // Do we need to skip this character?
                if (skipNext || skipUntilEnd || !hasSeparator)
                {
                    skipNext = false;
                    continue;
                }

                buffer.Append(c);
            }

            return sectionId.ToLower() switch
            {
                "text" => BinSections.Text,
                "data" => BinSections.Data,
                _      => null
            };
        }

        #endregion // Section Line Parsing

        #region Argument Type Parsing

        /// <summary>
        /// Parse a byte literal argument.
        /// </summary>
        /// <param name="aData">
        /// A string containing the byte to be parsed.
        /// </param>
        /// <returns>
        /// A byte resulting from parsing the string.
        /// </returns>
        public static byte ParseByteLiteral(string aData)
        {
            // An empty string cannot be considered a valid
            // byte literal.
            Assert(string.IsNullOrWhiteSpace(aData),
                   ExIDs.InvalidByteLiteral,
                   aData);

            byte result;
            bool success;

            // The prefix is the section that indicates
            // the type of integer that we are dealing
            // with. This is usually the first or second
            // character of the value.
            var prefix = "";
            if (aData.Length > 2)
            {
                prefix = aData[..2];
            }

            switch (prefix)
            {
                case "0b":
                    // A binary literal.
                    success =
                        Utils.TryParseBinByte(aData[2..], out result);
                    break;

                case "0x":
                    // A hexadecimal literal.
                    success =
                        Utils.TryParseHexByte(aData[2..], out result);
                    break;

                default:
                    {
                        var octalChar = '\0';
                        if (prefix.Length > 1)
                        {
                            octalChar = aData[0];
                        }

                        if (octalChar == '0')
                        {
                            // An octal literal.
                            success =
                                Utils.TryParseOctByte(aData[1..],
                                                      out result);
                            break;
                        }

                        // If all else fails, we will try a normal
                        // (decimal) byte parse.
                        success =
                            Utils.TryParseByte(aData, out result);
                        break;
                    }
            }

            // Was the input string a successfully parsed?
            Assert(!success,
                   ExIDs.InvalidByteLiteral,
                   aData);

            return result;
        }

        /// <summary>
        /// Parse an integer literal argument.
        /// </summary>
        /// <param name="aData">
        /// A string containing the integer to be parsed.
        /// </param>
        /// <param name="aAllowNegative">
        /// If negative values should be permitted during parsing.
        /// </param>
        /// <returns>
        /// An integer resulting from the parsed string.
        /// </returns>
        public static int ParseIntegerLiteral(string aData,
                                              bool aAllowNegative = true)
        {
            // An empty string cannot be considered a valid
            // integer literal.
            Assert(string.IsNullOrWhiteSpace(aData),
                   ExIDs.InvalidIntLiteral,
                   aData,
                   aAllowNegative);

            int result;
            bool success;

            var isSigned = aData[0] == '-';

            // Do we need to reject negative (signed) integers?
            Assert(isSigned && !aAllowNegative,
                   ExIDs.InvalidIntLiteral,
                   aData,
                   aAllowNegative);

            // The prefix is the section that indicates
            // the type of integer that we are dealing
            // with. This is usually the first or second
            // character of the value.
            var offset = !isSigned ? 0 : 1;
            var prefix = "";
            if (aData.Length > 2)
            {
                prefix = !isSigned ? aData[..2] : aData[offset..3];
            }

            switch (prefix)
            {
                case "0b":
                    // A binary literal.
                    success =
                        Utils.TryParseBinInt(aData[(2 + offset)..],
                                             out result);
                    break;

                case "0x":
                    // A hexadecimal literal.
                    success =
                        Utils.TryParseHexInt(aData[(2 + offset)..],
                                             out result);
                    break;

                default:
                    {
                        var octalChar = '\0';
                        if (prefix.Length > 1)
                        {
                            octalChar = !isSigned ? aData[0] : aData[1];
                        }

                        if (octalChar == '0')
                        {
                            // An octal literal.
                            success =
                                Utils.TryParseOctInt(aData[(1 + offset)..],
                                                     out result);
                            break;
                        }

                        // If all else fails, we will try a normal
                        // (decimal) integer parse.
                        success =
                            Utils.TryParseInt(aData[offset..],
                                              out result);
                        break;
                    }
            }

            // Was the input string a successfully parsed?
            Assert(!success,
                   ExIDs.InvalidIntLiteral,
                   aData,
                   aAllowNegative);

            return !isSigned ? result : result * -1;
        }

        #endregion // Argument Type Parsing

        /// <summary>
        /// Throw an exception if a given condition is true.
        /// </summary>
        /// <param name="aCondition">
        /// The condition to be checked.
        /// </param>
        /// <param name="aId">
        /// The ID of the exception to be raised.
        /// </param>
        /// <param name="aParams">
        /// Any parameters to be passed into the exception.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Assert(bool aCondition,
                                  ExIDs aId,
                                  params object[] aParams)
        {
            if (!aCondition)
            {
                return;
            }

            throw new AsmParserException
            (
                string.Format(_exMessages[aId], aParams)
            );
        }
    }
}
