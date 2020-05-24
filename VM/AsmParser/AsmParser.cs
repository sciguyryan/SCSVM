using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using VMCore.Assembler;
using VMCore.VM;
using VMCore.VM.Core;
using VMCore.VM.Instructions;

namespace VMCore.AsmParser
{
    public class AsmParser
    {
        /// <summary>
        /// A cached of the opcodes to their instruction instances.
        /// </summary>
        /// <remarks>
        /// Since the CPU cannot be run on its own then this is safe
        /// to use here as the virtual machine parent will always
        /// have called the method to build these caches.
        /// </remarks>
        private readonly Dictionary<OpCode, Instruction> _insCache;

        public AsmParser()
        {
            // We need to do this just in case the cache
            // has not already been built.
            ReflectionUtils.BuildCachesAndHooks(true);

            // Load the instruction cache.
            _insCache = ReflectionUtils.InstructionCache;
        }

        /// <summary>
        /// Parse a complete input string.
        /// </summary>
        /// <param name="aInput">The input string to be parsed.</param>
        /// <returns>
        /// An array of instructions representing the input data.
        /// </returns>
        public QuickIns[] Parse(string aInput)
        {
            var lines =
                aInput.Split(new[] { Environment.NewLine },
                             StringSplitOptions.RemoveEmptyEntries );

            return 
                lines
                    .Select(ParseLine)
                    .Where(aIns => aIns != null)
                    .ToArray();
        }

        /// <summary>
        /// Parse a single instruction line.
        /// </summary>
        /// <param name="aLine">The input string to be parsed.</param>
        /// <returns>
        /// A QuickIns object representing the parsed data.
        /// This can be null if there was no instruction to output.
        /// </returns>
        public QuickIns ParseLine(string aLine)
        {
            // Fast path return for lines that are comments.
            if (aLine[0] == ';')
            {
                return null;
            }

            var buffer = new StringBuilder();
            var segments = new List<string>();

            var skipNext = false;
            var inBracket = false;
            var inString = false;
            var skipUntilEnd = false;
            var pushBuffer = false;

            foreach (var c in aLine)
            {
                switch (c)
                {
                    case '"':
                        inString = !inString;
                        break;

                    case ';':
                        skipUntilEnd = !inString;
                        break;

                    case '[':
                        inBracket = true;
                        break;

                    case ']':
                        if (!inBracket)
                        {
                            throw new AsmParserException("melon");
                        }

                        inBracket = false;
                        break;

                    case ',':
                        skipNext = !inString;
                        pushBuffer = !inString;
                        break;

                    case char _ when char.IsWhiteSpace(c):
                        pushBuffer = (segments.Count == 0);
                        skipNext = true;
                        break;
                }

                // Do we need to push the contents of the buffer
                // into the segment list?
                if (pushBuffer)
                {
                    // We never want to push an empty buffer.
                    if (buffer.Length > 0)
                    {
                        segments.Add(buffer.ToString());
                        buffer.Clear();
                    }

                    pushBuffer = false;
                }

                // We should append this character to the buffer provided
                // that we have not been told to skip.
                if (!skipNext && !skipUntilEnd)
                {
                    buffer.Append(c);
                }
                else
                {
                    skipNext = false;
                }
            }

            if (inString)
            {
                // We have an unmatched string, this will throw an
                // exception as we cannot be sure of the validity of
                // the parsed data.
                throw new AsmParserException("watermelon");
            }

            if (inBracket)
            {
                // We have an unmatched bracket, this will throw an
                // exception as we cannot be sure of the validity of
                // the parsed data.
                throw new AsmParserException("lemon");
            }

            // Push whatever is left in the buffer into the segment
            // list. This will always need to happen as the last
            // segment will never be pushed.
            if (buffer.Length > 0)
            {
                segments.Add(buffer.ToString());
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
        /// A QuickIns object representing the parsed data.
        /// This can be null if there was no instruction to output.
        /// </returns>
        private QuickIns BuildInstruction(string[] aSegments)
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

            // This is a special case, this is a label.
            // Trying to parse this in the normal way will fail
            // as it is in a format like this:
            // @GOOD
            // In this case we pass "GOOD" as the argument and
            // then we are done.
            if (asmName[0] == '@')
            {
                return
                    new QuickIns(OpCode.LABEL,
                                 new object[] { asmName[1..] });
            }

            // If the instruction has no arguments then it is simple to
            // identify the opcode for the instruction belonging to it. 
            // If it has arguments then the job is more difficult as we
            // will have to perform validation on the number of arguments,
            // their types, etc. to find the best match.
            return aSegments.Length == 1 ? 
                ParseSimpleInstruction(asmName) :
                ParseComplexInstruction(asmName, args);
        }

        private QuickIns ParseSimpleInstruction(string aName)
        {
            if (!Enum.TryParse(aName.ToUpper(), out OpCode op))
            {
                throw new AsmParserException("dragon fruit");
            }

            return new QuickIns(op);
        }

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
                        values[i] = ParseIntegerLiteral(arg[2..]);
                        refTypes[i] = InsArgTypes.LiteralPointer;
                        continue;

                    case '&':
                        {
                            // This is a register argument pointer so we need
                            // to parse this as a register identifier instead.
                            if (!TryParseRegister(arg[1..],
                                                  out var regPtr))
                            {
                                throw new AsmParserException("plum");
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
                        labels[i] = ParseLabel(arg[1..]);
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

                // We did not find a valid argument type so the data
                // is likely invalid.
                throw new AsmParserException("kiwi");
            }

            return new ParInstructionData(values, refTypes, labels);
        }

        private QuickIns ParseComplexInstruction(string aInsName,
                                                 string[] aRawArgs)
        {
            var args = ParseArgs(aRawArgs);
            var insName = aInsName.ToLower();

            var len = args.Arguments.Length;
            var argTypes = new Type[len];

            AsmLabel asmLabel = null;
            for (var i = 0; i < len; i++)
            {
                argTypes[i] = args.Arguments[i].GetType();

                if (string.IsNullOrEmpty(args.BoundLabels[i]))
                {
                    continue;
                }

                if (asmLabel != null)
                {
                    // We cannot have more than one bound label
                    // to an instruction currently.
                    throw new AsmParserException("peach");
                }

                asmLabel = new AsmLabel(args.BoundLabels[i], i);
            }

            OpCode? op = null;

            // Iterate over our cached list of instructions
            // to find the one that best matches.
            foreach (var insPair in _insCache)
            {
                var ins = insPair.Value;

                if (ins.AsmName != insName ||
                    !argTypes.SequenceEqual(ins.ArgumentTypes) ||
                    !args.ArgRefTypes.SequenceEqual(ins.ArgumentRefTypes))
                {
                    // One (or more) of the following do not match:
                    // * the mnemonic (ASM) name;
                    // * the number or type of the arguments;
                    // * the number of ref type of the arguments;
                    // This instruction cannot be a match based
                    // on our data.
                    continue;
                }

                // Everything matches. We have found the
                // correct instruction.
                op = ins.OpCode;
                break;
            }

            // We did not find a valid match.
            if (op == null)
            {
                throw new AsmParserException("mushrooms");
            }

            return new QuickIns((OpCode)op, args.Arguments, asmLabel);
        }

        private static string ParseLabel(string aData)
        {
            // A valid label is any one or more
            // alpha numeric characters.
            var label = new StringBuilder();

            foreach (var c in aData)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    break;
                }

                label.Append(c);
            }

            return label.ToString();
        }

        private static int ParseIntegerLiteral(string aData)
        {
            var result = 0;
            var success = false;

            switch (aData[0..2])
            {
                case "0b":
                    // A binary literal.
                    success = TryParseBinInt(aData[2..], out result);
                    break;

                case "0x":
                    // A hexadecimal literal.
                    success = TryParseHexInt(aData[2..], out result);
                    break;

                default:
                {
                    if (aData[0] == '0')
                    {
                        // An octal literal.
                        success = TryParseOctInt(aData[1..], out result);
                    }
                    else
                    {
                        // If all else fails, we will try a normal integer
                        // parse.
                        success = TryParseInt(aData, out result);
                    }

                    break;
                }
            }

            if (!success)
            {
                throw new AsmParserException("parsnip");
            }

            return result;
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
        private static bool TryParseRegister(string aStr,
                                             out Registers aReg)
        {
            return Enum.TryParse(aStr, out aReg);
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
    }
}
