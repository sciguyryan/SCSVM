#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using VMCore.Assembler;
using VMCore.Expressions;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;
using VMCore.VM.Instructions;

namespace VMCore.AsmParser
{
    public class AsmInstructionParser
    {
        #region Private Properties

        /// <summary>
        /// A cached of the opcodes to their instruction instances.
        /// </summary>
        private readonly Dictionary<OpCode, Instruction> _insCache =
            ReflectionUtils.InstructionCache;

        /// <summary>
        /// A cached list of instruction data used for quickly
        /// identifying an instruction.
        /// </summary>
        private readonly Dictionary<InsCacheEntry, OpCode> _insCacheEntries =
            new Dictionary<InsCacheEntry, OpCode>();

        /// <summary>
        /// A cached lookup of register string to Registers.
        /// </summary>
        private readonly Dictionary<string, Registers> _registerLookUp
            = new Dictionary<string, Registers>();

        /// <summary>
        /// A cached lookup of instruction size hint string to InstructionSizeHint.
        /// </summary>
        private readonly Dictionary<string, InstructionSizeHint> _sizeHintLookUp
            = new Dictionary<string, InstructionSizeHint>();

        /// <summary>
        /// A unique subroutine counter.
        /// </summary>
        private int _subRoutineSeqId;

        #endregion // Private Properties

        public AsmInstructionParser()
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

            var sizeHints =
                (InstructionSizeHint[])Enum.GetValues(typeof(InstructionSizeHint));
            foreach (var sizeHint in sizeHints)
            {
                _sizeHintLookUp.Add(sizeHint.ToString().ToLower(), sizeHint);
            }
        }

        /// <summary>
        /// Parse a string and convert it into an instruction.
        /// </summary>
        /// <param name="aLine">The string to be parsed.</param>
        /// <returns>
        /// A nullable CompilerIns object representing the parsed data.
        /// This can be null if there was no instruction to output.
        /// </returns>
        public CompilerIns? ParseLine(ReadOnlySpan<char> aLine)
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
                    AsmParser.Assert(inBracket,
                                    AsmParser.ExIDs.MismatchedBrackets,
                                    segments.Count + 1,
                                    i);

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
                        AsmParser.Assert
                        (
                            !inBracket && !inString,
                            AsmParser.ExIDs.MismatchedBrackets,
                            segments.Count + 1,
                            i
                        );

                        inBracket = false;
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
                    // We should not be attempting to push a string
                    // that has a mismatched bracket within it.
                    AsmParser.Assert
                    (
                        inBracket,
                        AsmParser.ExIDs.InvalidBracketPosition,
                        segments.Count + 1,
                        i
                    );

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
        /// Build a CompilerIns object based on the parsed data. 
        /// </summary>
        /// <param name="aSegments">
        /// An array of strings representing the segments of the
        /// instruction. The first segment will always be the
        /// instruction mnemonic, any additional segments will
        /// represent the arguments provided to the instruction.
        /// </param>
        /// <returns>
        /// A nullable CompilerIns object representing the parsed data.
        /// This can be null if there was no instruction to output.
        /// </returns>
        private CompilerIns? BuildInstruction(string[] aSegments)
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
                    new CompilerIns(OpCode.LABEL,
                        new object[] { label });
            }

            // This is a subroutine label.
            var subLabel = TryParseSubroutine(asmName);

            // The subroutine instruction only expects
            // one argument but we pass a second one here.
            // In this instance it doesn't matter as we only
            // need this data for use in the compiler.
            return
                new CompilerIns(OpCode.SUBROUTINE,
                                new object[]
                                {
                                    _subRoutineSeqId++,
                                    subLabel
                                });
        }

        /// <summary>
        /// Parse a simple instruction - an instruction that has
        /// no arguments.
        /// </summary>
        /// <param name="aInsName">
        /// The mnemonic of the instruction.
        /// </param>
        /// <returns>
        /// A nullable CompilerIns object containing the parsed data.
        /// </returns>
        private CompilerIns? ParseSimple(string aInsName)
        {
            if (Enum.TryParse(aInsName, true, out OpCode op))
            {
                return new CompilerIns(op);
            }

            AsmParser.Assert
            (
                true,
                AsmParser.ExIDs.InvalidInstruction,
                aInsName,
                "", "", ""
            );

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
                        // This is a literal expression.
                        // We need to evaluate it here.
                        try
                        {
                            values[i] =
                                new Parser(arg[1..^1])
                                    .ParseExpression()
                                    .Evaluate();
                            refTypes[i] = InsArgTypes.LiteralInteger;
                        }
                        catch (ExprParserException)
                        {
                            AsmParser.Assert
                            (
                                true,
                                AsmParser.ExIDs.InvalidExpression,
                                arg[1..^1]
                            );
                        }
                        continue;

                    case '$':
                        // This is a literal.
                        // These can be any of the following:
                        // * binary (donated by an 0b prefix)
                        // * hexadecimal (donated by an 0x prefix)
                        // * octal (donated by an 0 prefix)
                        // * or a decimal (anything else).
                        values[i] = 
                            AsmParser.ParseIntegerLiteral(arg[1..]);
                        refTypes[i] = InsArgTypes.LiteralInteger;
                        continue;

                    case '&' when arg[1] == '[':
                        // This is an expression pointer.
                        // We need to evaluate it here.
                        try
                        {
                            values[i] =
                                new Parser(arg[2..^1])
                                    .ParseExpression()
                                    .Evaluate();
                            refTypes[i] = InsArgTypes.LiteralPointer;
                        }
                        catch (ExprParserException)
                        {
                            AsmParser.Assert
                            (
                                true,
                                AsmParser.ExIDs.InvalidExpression,
                                arg[2..^1]
                            );
                        }
                        continue;

                    case '&' when arg[1] == '$':
                        // This is a literal address pointer.
                        // This can be treated as a normal
                        // integer argument.
                        // There is one exception and that is that
                        // they cannot be signed (negative) as that
                        // would point to an invalid memory address.
                        values[i] =
                            AsmParser.ParseIntegerLiteral(arg[2..],
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
                                AsmParser.Assert
                                (
                                    true,
                                    AsmParser.ExIDs.InvalidRegisterIdentifier,
                                    arg[1..]
                                );
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
                // Check if this is a register identifier.
                if (TryParseRegister(arg, out var reg))
                {
                    // This is a register identifier.
                    values[i] = reg;
                    refTypes[i] = InsArgTypes.Register;
                    continue;
                }

                // Is this an instruction size hint identifier?
                if (TryParseSizeHint(arg, out var sizeHint))
                {
                    // This is a size hint identifier.
                    values[i] = sizeHint;
                    refTypes[i] = InsArgTypes.InstructionSizeHint;
                    continue;
                }

                // If none of the conditions have matched then this
                // is likely to be a compiler directive label.
                values[i] = 0;
                refTypes[i] = InsArgTypes.LiteralInteger;
                labels[i] = TryParseLabel(arg);
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
        /// A nullable CompilerIns object containing the parsed data.
        /// </returns>
        private CompilerIns? ParseComplex(string aInsName,
                                          IReadOnlyList<string> aRawArgs)
        {
            var args = ParseArgs(aRawArgs);
            var insName = aInsName.ToLower();

            var len = args.Arguments.Length;
            var argTypes = new Type[len];

            var asmLabels = new AsmLabel[len];
            for (var i = 0; i < len; i++)
            {
                // Build the argument type list for this instruction.
                argTypes[i] = args.Arguments[i].GetType();

                if (string.IsNullOrWhiteSpace(args.BoundLabels[i]))
                {
                    continue;
                }

                // Build the for this argument, if one has been specified.
                asmLabels[i] =
                    new AsmLabel(args.BoundLabels[i], i);
            }

            // Generate out cache entry based on the extracted information.
            var pEntry =
                new InsCacheEntry(insName,
                                  argTypes,
                                  args.ArgRefTypes);

            // Do we have an instruction that matches the data we have been
            // able to parse?
            if (!_insCacheEntries.TryGetValue(pEntry, out var op))
            {
                AsmParser.Assert
                (
                    true,
                    AsmParser.ExIDs.InvalidInstruction,
                    aInsName,
                    string.Join(", ", aRawArgs),
                    string.Join<Type>(", ", argTypes),
                    string.Join(", ", args.ArgRefTypes)
                );
            }

            return new CompilerIns(op, args.Arguments, asmLabels);
        }

        #region Argument Parsing

        /// <summary>
        /// Parse a label.
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

                // A label cannot start with a digit.
                if (i == 0 && char.IsDigit(c))
                {
                    AsmParser.Assert
                    (
                        true,
                        AsmParser.ExIDs.InvalidLabel
                    );
                }

                label.Append(c);
            }

            // We cannot have an empty label.
            AsmParser.Assert
            (
                label.Equals(string.Empty),
                AsmParser.ExIDs.InvalidLabel
            );

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
                    AsmParser.Assert
                    (
                        i == 0 || hasSubMarker,
                        AsmParser.ExIDs.InvalidSubroutineLabel,
                        aData
                    );

                    hasSubMarker = true;
                }

                label.Append(c);
            }

            var lbl = label.ToString();

            // We cannot have an empty subroutine label.
            AsmParser.Assert
            (
                label.Equals(string.Empty),
                AsmParser.ExIDs.InvalidSubroutineLabel,
                lbl
            );

            // A subroutine must end with a colon character.
            AsmParser.Assert
            (
                lbl[^1] != ':',
                AsmParser.ExIDs.InvalidSubroutineLabel,
                lbl
            );

            // We do not care about the last character as it
            // is always a colon.
            return lbl[..^1];
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
        /// Attempt to parse a string as a size hint identifier.
        /// </summary>
        /// <param name="aStr">The string to be parsed.</param>
        /// <param name="aSizeHint">
        /// An identifier within the InstructionSizeHint enum
        /// representing the string.
        /// </param>
        /// <returns>
        /// A boolean, true parsing the string yielded a valid
        /// Register identifier, false otherwise.
        /// </returns>
        private bool TryParseSizeHint(string aStr,
            out InstructionSizeHint aSizeHint)
        {
            return
                _sizeHintLookUp.TryGetValue(aStr.ToLower(),
                                            out aSizeHint);
        }

        #endregion // Argument Parsing
    }
}
