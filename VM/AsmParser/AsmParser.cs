#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using VMCore.Assembler;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;
using VMCore.VM.Instructions;
using OpCode = VMCore.VM.Core.OpCode;

namespace VMCore.AsmParser
{
    public class AsmParser
    {
        /// <summary>
        /// A cached of the opcodes to their instruction instances.
        /// </summary>
        private readonly Dictionary<OpCode, Instruction> _insCache =
            ReflectionUtils.InstructionCache;

        private readonly Dictionary<InsCacheEntry, OpCode> _insCacheEntries =
            new Dictionary<InsCacheEntry, OpCode>();

        private readonly Dictionary<string, Registers> _registerLookUp 
            = new Dictionary<string, Registers>();

        #region EXCEPTIONS

        /// <summary>
        /// A list of exception IDs used by this class.
        /// </summary>
        private enum ExIDs
        {
            MismatchedBrackets,
            InvalidBracketPosition,
            MismatchedString,
            InvalidLabel,
            InvalidSubroutineLabel,
            InvalidIntLiteral,
            InvalidRegisterIdentifier,
            InvalidArgumentType,
            MultipleArgumentLabels,
            InvalidInstruction,
        };

        /// <summary>
        /// A list of the exception messages used by this class.
        /// </summary>
        private readonly Dictionary<ExIDs, string> _exMessages =
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
                ExIDs.InvalidRegisterIdentifier,
                "The string was not a valid register identifier: '{0}'."
            },
            {
                ExIDs.InvalidArgumentType,
                "The argument type for the string could not be " +
                "determined. String = '{0}'."
            },
            {
                ExIDs.MultipleArgumentLabels,
                "More than one label was provided as arguments to the " +
                "instruction. This is not currently supported.\n" +
                "Instruction = '{0}', Arguments = {1}."
            },
            {
                ExIDs.InvalidInstruction,
                "No matching instruction was found for the input string.\n" +
                "Instruction = '{0}'\nArguments = {1}\nArgument Types = {2}\n" +
                "Argument Ref Types = {3}."
            },
        };

        #endregion // EXCEPTIONS

        private int _subRoutineSeqId;

        public AsmParser()
        {
            foreach (var insKvp in _insCache)
            {
                var (opCode, insData) = insKvp;

                var insCacheEntry =
                    new InsCacheEntry(insData.AsmName,
                                     insData.ArgumentTypes,
                                     insData.ArgumentRefTypes);

                _insCacheEntries.Add(insCacheEntry, opCode);
            }

            var registers = (Registers[])Enum.GetValues(typeof(Registers));
            foreach (var register in registers)
            {
                _registerLookUp.Add(register.ToString().ToLower(), register);
            }
        }

        /// <summary>
        /// Parse an input string into an array of instructions.
        /// </summary>
        /// <param name="aInput">The input string to be parsed.</param>
        /// <returns>
        /// An array of instructions representing the input data.
        /// </returns>
        public QuickIns[] Parse(string aInput)
        {
            var newLineSkip =
                Environment.NewLine.Length - 1;

            var insList = new List<QuickIns>();
            var dirList = new List<QuickDir>();

            var lineNo = 0;
            var isLine = false;
            var inString = false;
            var section = BinSections.Undefined;

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
                    // Yes, update the end point of our range
                    // to account for it.
                    endPos = i - 1;
                    skipChars = newLineSkip;
                    isLine = true;
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
                    ParseDataOrDirectiveLine(section,
                                             span[startPos..endPos],
                                             ref insList,
                                             ref dirList);
                }

                startPos = i + skipChars;
                isLine = false;
                ++lineNo;
            }

            return insList.ToArray();
        }

        private void ParseDataOrDirectiveLine(BinSections aSec,
                                              ReadOnlySpan<char> aLine,
                                              ref List<QuickIns> aInsList,
                                              ref List<QuickDir> aDirList)
        {
            switch (aSec)
            {
                case BinSections.Code:
                {
                    var ins =
                        ParseInsLine(aLine);
                    if (!(ins is null))
                    {
                        aInsList.Add(ins);
                    }

                    break;
                }

                case BinSections.Data:
                {
                    var directive =
                        ParseDirLine(aLine);
                    if (!(directive is null))
                    {
                        aDirList.Add(directive);
                    }

                    break;
                }
            }
        }

        private BinSections ParseSectionLine(ReadOnlySpan<char> aLine)
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

                    case { } when char.IsWhiteSpace(c):
                        skipNext = hasSeparator;
                        hasSeparator = true;
                        break;
                }

                // Do we need to skip this character?
                if (skipNext || skipUntilEnd)
                {
                    skipNext = false;
                    continue;
                }

                buffer.Append(c);
            }

            var section = sectionId.ToLower() switch
            {
                ".section code" => BinSections.Code,
                ".section data" => BinSections.Data,
                _               => BinSections.Undefined
            };

            return section;
        }

        private QuickDir? ParseDirLine(ReadOnlySpan<char> aLine)
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
                    Assert(inString,
                           ExIDs.MismatchedString,
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

        private QuickDir? BuildDirective(string[] aSegments)
        {
            // The line could not be a valid directive with
            // less than three segments.
            if (aSegments.Length < 3)
            {
                return null;
            }

            // Decompose the segments array into the instruction
            // name and the argument data.
            var dirLabel = aSegments[0];
            var dirType = aSegments[1].ToUpper();
            var args = aSegments[2..];

            Debug.WriteLine($"dirLabel = {dirLabel}");
            Debug.WriteLine($"dirType = {dirType}");
            Debug.WriteLine($"Args = {string.Join(", ", args)}");

            if (!Enum.TryParse(typeof(DirectiveCodes),
                               dirType,
                               out var dirCode))
            {
                throw new Exception();
            }

            string? directiveStrData = null;
            byte[]? directiveData = null;
            switch (dirCode)
            {
                case DirectiveCodes.DB:
                    Debug.WriteLine("DB directive code.");
                    directiveData =
                        ConvertDbDirectiveArgs(args);
                    break;

                case DirectiveCodes.EQU:
                    Debug.WriteLine("EQU directive code.");
                    break;

                default:
                    throw new Exception();
                    break;
            }

            return 
                new QuickDir((DirectiveCodes)dirCode,
                             directiveData,
                             directiveStrData);
        }

        private byte[] ConvertDbDirectiveArgs(IEnumerable<string> aArgs)
        {
            List<byte> bytes = new List<byte>();

            foreach (var str in aArgs)
            {
                switch (str[0])
                {
                    case '\'':
                    case '"':
                        // This is a string.
                        // We do not care about the quotation marks.
                        bytes.AddRange(Encoding.UTF8.GetBytes(str[1..^2]));
                        break;

                    case '$':
                        var value =
                            ParseIntegerLiteral(str[1..]);
                        bytes.AddRange(BitConverter.GetBytes(value));
                        break;

                    default:
                        // TODO - What to do here?
                        break;
                }
            }

            Debug.WriteLine(bytes.Count);

            return bytes.ToArray();
        }

        /// <summary>
        /// Parse a string and convert it into an instruction.
        /// </summary>
        /// <param name="aLine">The string to be parsed.</param>
        /// <returns>
        /// A nullable QuickIns object representing the parsed data.
        /// This can be null if there was no instruction to output.
        /// </returns>
        private QuickIns? ParseInsLine(ReadOnlySpan<char> aLine)
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
            var inBracket = false;
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
                    // that has a mismatched bracket.
                    Assert(inBracket,
                           ExIDs.MismatchedBrackets,
                           segments.Count + 1,
                           i);

                    // We should not be attempting to push a line
                    // that has an unmatched string.
                    Assert(inString,
                           ExIDs.MismatchedString,
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
                        inString = !inString;
                        break;

                    case ';':
                        skipUntilEnd = !inString;
                        break;

                    case '[':
                        inBracket = !inString;
                        break;

                    case ']':
                        // Do we have a closing bracket but no matching
                        // opening bracket?
                        Assert(!inBracket && !inString,
                               ExIDs.MismatchedBrackets,
                               segments.Count + 1,
                               i);

                        inBracket = false;
                        break;

                    case ',':
                        skipNext = !inString;
                        pushString = !inString;
                        break;

                    case { } when char.IsWhiteSpace(c):
                        pushString = !inString && segments.Count == 0;
                        skipNext = !inString;
                        break;
                }

                // Do we need to push the contents of the buffer
                // into the segment list?
                if (pushString)
                {
                    // We should not be attempting to push a string
                    // that has a mismatched bracket within it.
                    Assert(inBracket,
                           ExIDs.InvalidBracketPosition,
                           segments.Count + 1,
                           i);

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

            return BuildInstruction(segments.ToArray());
        }

        /// <summary>
        /// Build a QuickIns object based on the parsed data. 
        /// </summary>
        /// <param name="aSegments">
        /// An array of strings representing the segments of the
        /// instruction. The first segment will always be the
        /// instruction mnemonic, any additional segments will
        /// represent the arguments provided to the instruction.
        /// </param>
        /// <returns>
        /// A nullable QuickIns object representing the parsed data.
        /// This can be null if there was no instruction to output.
        /// </returns>
        private QuickIns? BuildInstruction(string[] aSegments)
        {
            // The line was likely a comment line.
            if (aSegments.Length == 0)
            {
                return null;
            }

            // Decompose the segments array into the instruction
            // name and the argument data.
            var asmName = aSegments[0];
            var args = aSegments[1..];

            var lastSignificantChar = '\0';
            foreach (var c in asmName)
            {
                if (!char.IsLetterOrDigit(c) && c != ':')
                {
                    break;
                }

                lastSignificantChar = c;
            }

            // Is this a label?
            if (asmName[0] != '@' && lastSignificantChar != ':')
            {
                // No. This is a normal instruction.
                // If the instruction has no arguments then it is simple
                // to identify the opcode for the instruction
                // belonging to it.
                // If it has arguments then the job is more difficult
                // as we will have to perform validation on the number
                // of arguments, their types, etc. to find the best match.
                return
                    aSegments.Length == 1 ? 
                        ParseSimple(asmName) : 
                        ParseComplex(asmName, args);
            }

            // Yes, this is a label or a subroutine.
            // These are special cases.
            // Trying to parse these in the normal way will fail
            // as it will be formatted like one of these:
            // @LABEL or SUBROUTINE:
            var label = TryParseLabel(asmName[1..]);

            // This is a label.
            if (lastSignificantChar != ':')
            {
                return
                    new QuickIns(OpCode.LABEL,
                        new object[] { label });
            }

            // This is a subroutine label.
            var subLabel = TryParseSubroutine(asmName);

            // The subroutine instruction only expects
            // one argument but we pass a second one here.
            // In this instance it doesn't matter as we only
            // need this data for use in the compiler.
            return
                new QuickIns(OpCode.SUBROUTINE,
                    new object[] { _subRoutineSeqId++, subLabel });
        }

        /// <summary>
        /// Parse a simple instruction - an instruction that has
        /// no arguments.
        /// </summary>
        /// <param name="aInsName">
        /// The mnemonic of the instruction.
        /// </param>
        /// <returns>
        /// A nullable QuickIns object containing the parsed data.
        /// </returns>
        private QuickIns? ParseSimple(string aInsName)
        {
            if (Enum.TryParse(aInsName, true, out OpCode op))
            {
                return new QuickIns(op);
            }

            Assert(true,
                   ExIDs.InvalidInstruction,
                   aInsName,
                   "", "", "");

            return null;
        }

        /// <summary>
        /// Parse the arguments to be passed to a complex instruction.
        /// </summary>
        /// <param name="aArgs">
        /// An array containing the data for the arguments to be parsed.
        /// </param>
        /// <returns>
        /// A ParInstructionData object containing the data parsed from
        /// the arguments.
        /// </returns>
        private ParInstructionData ParseArgs(IReadOnlyList<string> aArgs)
        {
            var len = aArgs.Count;
            var values = new object[len];
            var refTypes = new InsArgTypes[len];
            var labels = new string[len];

            for (var i = 0; i < len; i++)
            {
                var arg = aArgs[i];

                switch (arg[0])
                {
                    case '[':
                        // This is an expression, these are always left
                        // as strings. The compiler will need to check
                        // them to see if they are valid or can be 
                        // simplified.
                        values[i] = arg[1..^1];
                        refTypes[i] = InsArgTypes.Expression;
                        continue;

                    case '$':
                        // This is a literal.
                        // These can be any of the following:
                        // * binary (donated by an 0b prefix)
                        // * hexadecimal (donated by an 0x prefix)
                        // * octal (donated by an 0 prefix)
                        // * or a decimal (anything else).
                        values[i] = ParseIntegerLiteral(arg[1..]);
                        refTypes[i] = InsArgTypes.LiteralInteger;
                        continue;

                    case '&' when arg[1] == '$':
                        // This is a literal address pointer.
                        // This can be treated as a normal
                        // integer argument.
                        // There is one exception and that is that
                        // they cannot be signed (negative) as that
                        // would point to an invalid memory address.
                        values[i] =
                            ParseIntegerLiteral(arg[2..],
                                                false);
                        refTypes[i] = InsArgTypes.LiteralPointer;
                        continue;

                    case '&':
                        {
                            // This is a register argument pointer so we need
                            // to parse this as a register identifier instead.
                            if (!TryParseRegister(arg[1..],
                                                  out var regPtr))
                            {
                                Assert(true,
                                       ExIDs.InvalidRegisterIdentifier,
                                       arg[1..]);
                            }

                            values[i] = regPtr;
                            refTypes[i] = InsArgTypes.RegisterPointer;
                            continue;
                        }

                    case '@':
                        // This is a label.
                        // We need to add a dummy value here. This will be
                        // updated at compile time.
                        values[i] = 0;
                        refTypes[i] = InsArgTypes.LiteralPointer;
                        labels[i] = TryParseLabel(arg[1..]);
                        continue;

                    case '!':
                        // This is a subroutine call.
                        // We need to add a dummy value here. This will be
                        // updated at compile time.
                        values[i] = 0;
                        refTypes[i] = InsArgTypes.LiteralPointer;
                        labels[i] = TryParseLabel(arg[1..]);
                        continue;
                }

                // We have not found one of the easy to identify
                // indicators of the type.
                // Currently the only thing that we have left to
                // check if a register identifier.
                if (TryParseRegister(arg, out var reg))
                {
                    // This is a register identifier.
                    values[i] = reg;
                    refTypes[i] = InsArgTypes.Register;
                    continue;
                }

                Assert(true,
                       ExIDs.InvalidArgumentType,
                       arg);
            }

            return new ParInstructionData(values, refTypes, labels);
        }

        /// <summary>
        /// Parse a complex instruction - an instruction that has one
        /// or more arguments.
        /// </summary>
        /// <param name="aInsName">
        /// The mnemonic name of the instruction.
        /// </param>
        /// <param name="aRawArgs">
        /// The name of the instruction.
        /// </param>
        /// <returns>
        /// A nullable QuickIns object containing the parsed data.
        /// </returns>
        private QuickIns? ParseComplex(string aInsName,
                                       IReadOnlyList<string> aRawArgs)
        {
            var args = ParseArgs(aRawArgs);
            var insName = aInsName.ToLower();

            var len = args.Arguments.Length;
            var argTypes = new Type[len];
            var labelIndices = new List<int>();

            AsmLabel? asmLabel = null;
            for (var i = 0; i < len; i++)
            {
                argTypes[i] = args.Arguments[i].GetType();

                if (string.IsNullOrWhiteSpace(args.BoundLabels[i]))
                {
                    continue;
                }

                labelIndices.Add(i);

                // We cannot have more than one bound label
                // to an instruction currently.
                Assert(!(asmLabel is null),
                       ExIDs.MultipleArgumentLabels,
                       aInsName,
                       string.Join(", ", aRawArgs));

                asmLabel = new AsmLabel(args.BoundLabels[i], i);
            }

            var pEntry =
                new InsCacheEntry(insName,
                                  argTypes,
                                  args.ArgRefTypes);

            if (!_insCacheEntries.TryGetValue(pEntry, out var op))
            {
                Assert(true,
                       ExIDs.InvalidInstruction,
                       aInsName,
                       string.Join(", ", aRawArgs),
                       string.Join<Type>(", ", argTypes),
                       string.Join(", ", args.ArgRefTypes));
            }

            // We have a potential match.
            // Now we need to check if we can bind labels to the
            // arguments, if specified.
            foreach (var labelIndex in labelIndices)
            {
                if (!_insCache[op].CanBindToLabel(labelIndex))
                {
                    return null;
                }
            }

            return new QuickIns(op, args.Arguments, asmLabel);
        }

        /// <summary>
        /// Parse a label .
        /// </summary>
        /// <param name="aData">
        /// A string containing the name of the label.
        /// </param>
        /// <returns>
        /// A string containing the parsed name of the label.
        /// </returns>
        private string TryParseLabel(string aData)
        {
            // A valid label is any one or more
            // alpha numeric characters.
            var label = new StringBuilder(aData.Length);

            var len = aData.Length;

            for (var i = 0; i < len; i++)
            {
                var c = aData[i];

                if (!char.IsLetterOrDigit(c))
                {
                    break;
                }

                label.Append(c);
            }

            // We cannot have an empty label.
            Assert(label.Equals(string.Empty),
                   ExIDs.InvalidLabel);

            return label.ToString();
        }

        /// <summary>
        /// Parse a subroutine label.
        /// </summary>
        /// <param name="aData">
        /// A string containing the name of the subroutine.
        /// </param>
        /// <returns>
        /// A string containing the parsed name of the subroutine.
        /// </returns>
        private string TryParseSubroutine(string aData)
        {
            // A valid label is any one or more
            // alpha numeric characters.
            var label = new StringBuilder(aData.Length);

            var len = aData.Length;

            var hasSubMarker = false;
            for (var i = 0; i < len; i++)
            {
                var c = aData[i];

                if (!char.IsLetterOrDigit(c) && c != ':')
                {
                    break;
                }

                if (c == ':')
                {
                    // TODO - add tests for this.
                    Assert(i == 0 || hasSubMarker,
                           ExIDs.InvalidSubroutineLabel,
                           aData);

                    hasSubMarker = true;
                }

                label.Append(c);
            }

            var lbl = label.ToString();

            // We cannot have an empty subroutine label.
            Assert(label.Equals(string.Empty),
                   ExIDs.InvalidSubroutineLabel,
                   lbl);

            // A subroutine must end with a colon character.
            Assert(lbl[^1] != ':',
                   ExIDs.InvalidSubroutineLabel,
                   lbl);

            // We do not care about the last character as it
            // is always a colon.
            return lbl[..^1];
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
        /// An integer containing the parsed value.
        /// </returns>
        private int ParseIntegerLiteral(string aData,
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

            return (!isSigned) ? result : result * -1;
        }

        /// <summary>
        /// Attempt to parse a string as a register identifier.
        /// </summary>
        /// <param name="aStr">The string to be parsed.</param>
        /// <param name="aReg">
        /// An identifier within the Registers enum representing
        /// the string.
        /// </param>
        /// <returns>
        /// A boolean, true parsing the string yielded a valid
        /// Register identifier, false otherwise.
        /// </returns>
        private bool TryParseRegister(string aStr,
                                      out Registers aReg)
        {
            return 
                _registerLookUp.TryGetValue(aStr.ToLower(),
                                            out aReg);
        }

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
        private void Assert(bool aCondition,
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
