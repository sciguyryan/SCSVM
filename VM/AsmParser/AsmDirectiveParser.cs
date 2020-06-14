#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using VMCore.Assembler;
using VMCore.VM.Core;

namespace VMCore.AsmParser
{
    public class AsmDirectiveParser
    {
        /// <summary>
        /// Parse a string and convert it into a compiler directive.
        /// </summary>
        /// <param name="aLine">The line to be parsed.</param>
        /// <returns>
        /// A nullable CompilerDir object representing the parsed data.
        /// This can be null if there was no data to output.
        /// </returns>
        public CompilerDir? ParseDirLine(ReadOnlySpan<char> aLine)
        {
            // Fast path return for lines that are comments.
            if (aLine[0] == ';')
            {
                return null;
            }

            var len = aLine.Length;
            var buffer = new StringBuilder(len);
            var segments = new List<string>();

            var skipNext = false;
            var inString = false;
            var skipUntilEnd = false;
            var pushString = false;

            for (var i = 0; i <= len; i++)
            {
                // Always ensure that we push the last segment to
                // the list.
                // This needs to be done here otherwise we might end
                // up skipping the entire last segment of the line.
                if (i == len)
                {
                    // We should not be attempting to push a line
                    // that has an unmatched string.
                    AsmParser.Assert(inString,
                                     AsmParser.ExIDs.MismatchedString,
                                     segments.Count + 1,
                                     i);

                    if (buffer.Length > 0)
                    {
                        segments.Add(buffer.ToString());
                    }

                    break;
                }

                var c = aLine[i];
                switch (c)
                {
                    case '"':
                    case '\'':
                        inString = !inString;
                        break;

                    case ';':
                        skipUntilEnd = !inString;
                        break;

                    case ',':
                        skipNext = !inString;
                        pushString = !inString;
                        break;

                    case { } when c == AsmParser.LineContinuation:
                        skipNext = !inString;
                        break;

                    case { } when char.IsWhiteSpace(c):
                        pushString = !inString;
                        skipNext = !inString;
                        break;
                }

                // Do we need to push the contents of the buffer
                // into the segment list?
                if (pushString)
                {
                    if (buffer.Length > 0)
                    {
                        segments.Add(buffer.ToString());
                        buffer.Clear();
                    }

                    pushString = false;
                }

                // Do we need to skip this character?
                if (skipNext || skipUntilEnd)
                {
                    skipNext = false;
                    continue;
                }

                buffer.Append(c);
            }

            return BuildDirective(segments.ToArray());
        }

        /// <summary>
        /// Build a CompilerIns object based on the parsed data. 
        /// </summary>
        /// <param name="aSegments">
        /// An array of strings representing the segments of the
        /// directive.
        /// </param>
        /// <param name="aStartIndex">
        /// The index from which we should begin to parse the
        /// arguments.
        /// </param>
        /// <returns>
        /// A nullable CompilerDir object representing the parsed data.
        /// This can be null if there was no data to output.
        /// </returns>
        private CompilerDir? BuildDirective(IReadOnlyList<string> aSegments,
                                            int aStartIndex = 0)
        {
            // The line could not be a valid directive with
            // less than three segments.
            if (aSegments.Count < 3)
            {
                return null;
            }

            // If this is the first segment (startIndex == 0)
            // then the directive label will be the first entry
            // of the segment array. Otherwise it will be empty.
            // This will be the case in sub directives.
            var dirLabel =
                aStartIndex == 0 ? aSegments[0] : string.Empty;

            object? dirCodeObj = null;
            var foundDirective = false;
            var len = aSegments.Count;
            var argIndex = aStartIndex == 0 ? 1 : aStartIndex;
            for (; argIndex < len; argIndex++)
            {
                // Did we find a known directive identifier?
                foundDirective =
                    Enum.TryParse(typeof(DirectiveCodes),
                                  aSegments[argIndex].ToUpper(),
                                  out dirCodeObj);
                if (foundDirective)
                {
                    break;
                }
            }

            // We did not find a valid directive within the
            // input arguments. The data must be invalid.
            if (!foundDirective || dirCodeObj is null)
            {
                AsmParser.Assert(true,
                                 AsmParser.ExIDs.InvalidDirectiveType);
                return null;
            }

            var dirCode = (DirectiveCodes)dirCodeObj;

            // Are we working with a times directive?
            string? timesPreExprStr = null;
            if (dirCode == DirectiveCodes.TIMES)
            {
                timesPreExprStr =
                    HandleTimesDirective(aSegments,
                                         ref argIndex,
                                         ref dirCode);
            }

            // Advance to the next segment.
            ++argIndex;

            // Which type of directive are we parsing?
            var canHaveSubDirective = false;
            string? directiveStrData = null;
            byte[]? directiveData = null;
            switch (dirCode)
            {
                case DirectiveCodes.DB:
                    canHaveSubDirective = true;
                    directiveData =
                        ParseDbDirectiveArgs(aSegments, ref argIndex);
                    break;

                case DirectiveCodes.EQU:
                    // We only expect a single argument with
                    // the EQU directive, any additional arguments
                    // will simply be disregarded.
                    directiveStrData = aSegments[2];
                    break;

                case DirectiveCodes.TIMES:
                default:
                    AsmParser.Assert(true,
                                     AsmParser.ExIDs.InvalidDirectiveType);
                    break;
            }

            // Do we need to handle a sub directive here?
            CompilerDir? subDir = null;
            if (canHaveSubDirective && argIndex < len)
            {
                // We do not handle nested sub directives.
                AsmParser.Assert((aStartIndex > 0),
                                 AsmParser.ExIDs.NestedSubQuery);

                // We have a sub directive to handle.
                // This will usually be a "TIMES" directive.
                // We will start processing the sub directive
                // from the current argument index.
                subDir = BuildDirective(aSegments, argIndex);
            }

            // We are done! Construct and return the
            // compiler directive object.
            return
                new CompilerDir(dirCode,
                                dirLabel,
                                directiveData,
                                directiveStrData,
                                timesPreExprStr,
                                subDir);
        }

        /// <summary>
        /// Parse a times directive and its arguments.
        /// </summary>
        /// <param name="aSegments">
        /// A list of arguments to be parsed.
        /// </param>
        /// <param name="aArgIndex">
        /// A reference to the argument index from which parsing
        /// should begin. This will be updated within the method
        /// as segments are parsed.
        /// </param>
        /// <param name="aDirCode">
        /// The directive type of the argument to which the times
        /// directive has been applied. This will be updated within
        /// the method as segments are parsed.
        /// </param>
        /// <returns>
        /// A string giving the times directive expression string.
        /// </returns>
        private string HandleTimesDirective(IReadOnlyList<string> aSegments,
                                            ref int aArgIndex,
                                            ref DirectiveCodes aDirCode)
        {
            // Skip the TIMES directive term as it will
            // trigger an early loop break below.
            ++aArgIndex;

            object? dirCode = null;
            string timesExprStr = string.Empty;

            var hasValidDir = false;
            var len = aSegments.Count;
            for (; aArgIndex < len; aArgIndex++)
            {
                if (!Enum.TryParse(typeof(DirectiveCodes),
                                   aSegments[aArgIndex].ToUpper(),
                                   out dirCode))
                {
                    // We have not found a valid directive.
                    // Add this argument to the expression string.
                    timesExprStr += aSegments[aArgIndex];
                    continue;
                }

                // Ensure that this directive can use the times
                // prefix.
                hasValidDir = dirCode switch
                {
                    DirectiveCodes.DB => true,
                    _                 => false
                };
                break;
            }

            // We did not find a valid directive after the times
            // prefix. This syntax is invalid.
            // We have to null check dirCode here as the compiler
            // doesn't understand that it cannot ever be null here.
            if (!hasValidDir || dirCode is null)
            {
                AsmParser.Assert(true, 
                                 AsmParser.ExIDs.InvalidDirectiveType);
                return string.Empty;
            }

            // Update the directive code.
            aDirCode = (DirectiveCodes)dirCode;

            return timesExprStr;
        }

        /// <summary>
        /// Parse DB (define bytes) compiler directive arguments.
        /// </summary>
        /// <param name="aSegments">
        /// An array of strings representing the segments of the
        /// directive.
        /// </param>
        /// <param name="aArgIndex">
        /// The argument index from which to begin parsing.
        /// </param>
        /// <returns>
        /// An array of bytes representing the parsed data.
        /// </returns>
        private byte[] ParseDbDirectiveArgs(IReadOnlyList<string> aSegments,
                                            ref int aArgIndex)
        {
            var bytes = new List<byte>(10_000);

            var len = aSegments.Count;
            for (; aArgIndex < len; aArgIndex++)
            {
                var str = aSegments[aArgIndex];

                // We will need to create a sub directive to handle
                // this if we encounter a times directive.
                // We can break here as there will be nothing more
                // for us to do.
                if (str.ToUpper() == "TIMES")
                {
                    break;
                }

                // Add the bytes to our list.
                bytes.AddRange(DirArgumentsToBytes(str));
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// Convert a byte-type directive string into a byte array.
        /// </summary>
        /// <param name="aStr">
        /// The string to be converted into bytes.
        /// </param>
        /// <returns>A byte array representing the input string.</returns>
        private IEnumerable<byte> DirArgumentsToBytes(string aStr)
        {
            switch (aStr[0])
            {
                case '\'':
                case '"':
                    // This is a string.
                    // We do not care about the type of
                    // quotation marks used here
                    return Encoding.UTF8.GetBytes(aStr[1..^1]);

                case '$':
                    // This is a byte literal.
                    return new[] { AsmParser.ParseByteLiteral(aStr[1..]) };

                default:
                    // TODO - need to handle this.
                    throw new Exception();
            }
        }
    }
}
