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
        private Dictionary<OpCode, Instruction> _instructionCache =
            ReflectionUtils.InstructionCache;

        public AsmParser()
        {
        }

        /// <summary>
        /// Initiate a full parse of the input string.
        /// </summary>
        public QuickIns[] Parse(string aInput)
        {
            var instructions = new List<QuickIns>();

            var lines =
                aInput.Split(new[] { Environment.NewLine },
                             StringSplitOptions.RemoveEmptyEntries );

            foreach (var line in lines)
            {
                // If this is null then the line was likely a comment.
                var ins = ParseLine(line);
                if (ins != null)
                {
                    instructions.Add(ins);
                }
            }

            return instructions.ToArray();
        }

        public QuickIns ParseLine(string aData)
        {
            // Fast path return for lines that are comments.
            if (aData[0] == ';')
            {
                return null;
            }

            var buffer = new StringBuilder();
            var segments = new List<string>();

            var skipNext = false;
            var inBrackets = false;
            var inString = false;
            var skipUntilEnd = false;
            var pushBufferContents = false;

            foreach (var c in aData)
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
                        inBrackets = true;
                        break;

                    case ']':
                        if (!inBrackets)
                        {
                            throw new AsmParserException("melon");
                        }

                        inBrackets = false;
                        break;

                    case ',':
                        skipNext = !inString;
                        pushBufferContents = !inString;
                        break;

                    case char _ when char.IsWhiteSpace(c):
                        pushBufferContents = (segments.Count == 0);
                        skipNext = true;
                        break;
                }

                if (pushBufferContents)
                {
                    if (buffer.Length > 0)
                    {
                        segments.Add(buffer.ToString());
                        buffer.Clear();
                    }

                    pushBufferContents = false;
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

            // Push whatever is left in the buffer into the segment
            // list. This will always need to happen as the last segment
            // will never be pushed.
            if (buffer.Length > 0)
            {
                segments.Add(buffer.ToString());
            }

            return BuildInstruction(segments.ToArray());
        }

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

            // This instruction has no arguments, no ambiguity
            // so very easy to resolve.
            if (aSegments.Length == 1)
            {
                return ParseSimpleInstruction(asmName.ToLower());
            }

            // We have arguments, this is a little more complex
            // as we have to do some validation.
            var data = ParseArguments(args);

            return
                ParseComplexInstruction(asmName.ToLower(),
                                        data.Arguments,
                                        data.ArgRefTypes,
                                        data.BoundLabels);
        }

        private ParInstructionData ParseArguments(string[] args)
        {
            var len = args.Length;
            var parsed = new object[len];
            var refTypes = new InsArgTypes[len];
            var labels = new string[len];

            for (var i = 0; i < len; i++)
            {
                var arg = args[i];

                if (arg[0] == '[')
                {
                    // This is an expression, these are always left
                    // as strings. The compiler will need to check
                    // them to see if anything clever can be done
                    // with them.
                    parsed[i] = arg[1..^1];
                    refTypes[i] = InsArgTypes.Expression;

                    continue;
                }
                else if (arg[0] == '$')
                {
                    // This is a literal. These can be hexadecimal
                    // (donated by an 0x prefix) or a decimal (anything
                    // without one).
                    parsed[i] = ParseIntegerLiteral(arg[1..]);
                    refTypes[i] = InsArgTypes.LiteralInteger;

                    continue;
                }
                else if (arg[0] == '&')
                {
                    // This is an address pointer.
                    if (arg[1] == '$')
                    {
                        // This is a literal address pointer and so
                        // can be treated as a normal integer
                        // argument.
                        parsed[i] = ParseIntegerLiteral(arg[2..]);
                        refTypes[i] = InsArgTypes.LiteralPointer;

                        continue;
                    }

                    // This is a register argument pointer so we need
                    // to parse this as a register identifier instead.
                    if (!ParseRegister(arg[1..], out Registers regPtr))
                    {
                        throw new AsmParserException("plum");
                    }

                    parsed[i] = regPtr;
                    refTypes[i] = InsArgTypes.RegisterPointer;

                    continue;
                }
                else if (arg[0] == '@')
                {
                    // This is a label. We need to add a dummy
                    // value here.
                    parsed[i] = 0;
                    refTypes[i] = InsArgTypes.LiteralPointer;
                    labels[i] = ParseLabel(arg[1..]);

                    continue;
                }

                if (ParseRegister(arg, out Registers reg))
                {
                    parsed[i] = reg;
                    refTypes[i] = InsArgTypes.Register;

                    continue;
                }
                else
                {
                    throw new AsmParserException("kiwi");
                }
            }

            return new ParInstructionData(parsed, refTypes, labels);
        }

        private QuickIns ParseSimpleInstruction(string aName)
        {
            if (!Enum.TryParse(aName.ToUpper(), out OpCode op))
            {
                throw new AsmParserException("dragon fruit");
            }

            return new QuickIns(op);
        }

        private QuickIns ParseComplexInstruction(string aInsName,
                                                 object[] aArgs,
                                                 InsArgTypes[] aRefTypes,
                                                 string[] aLabels)
        {
            var insName = aInsName.ToLower();

            var len = aArgs.Length;
            var argTypes = new Type[len];

            AsmLabel asmLabel = null;

            for (var i = 0; i < len; i++)
            {
                argTypes[i] = aArgs[i].GetType();

                if (!string.IsNullOrEmpty(aLabels[i]))
                {
                    if (asmLabel != null)
                    {
                        // We cannot have more than one bound label
                        // to an instruction.
                        throw new AsmParserException("peach");
                    }

                    asmLabel = new AsmLabel(aLabels[i], i);
                }

                //Debug.WriteLine($"{argTypes[i].GetFriendlyName()} {argTypes[i]}");
            }

            OpCode? op = null;

            // Iterate over our cached list of instructions
            // to find the one that best matches.
            foreach (var insKVP in _instructionCache)
            {
                var ins = insKVP.Value;

                if (ins.AsmName != insName)
                {
                    // The ASM name does not match.
                    continue;
                }

                if (!argTypes.SequenceEqual(ins.ArgumentTypes) ||
                    !aRefTypes.SequenceEqual(ins.ArgumentRefTypes))
                {
                    // The number, type or reference types of the
                    // arguments do not match.
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

            return new QuickIns((OpCode)op, aArgs, asmLabel);
        }

        private string ParseLabel(string aData)
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

        private int ParseIntegerLiteral(string aData)
        {
            var result = 0;
            var success = false;

            if (aData[0..2] == "0x")
            {
                success = TryParseHexInt(aData[2..], out result);
            }
            else if (aData[0..2] == "0b")
            {
                success = TryParseBinInt(aData[2..], out result);
            }
            else if (aData[0] == '0')
            {
                success = TryParseOctInt(aData[1..], out result);
            }
            else
            {
                success = TryParseInt(aData, out result);
            }

            if (!success)
            {
                throw new AsmParserException("parsnip");
            }

            return result;
        }

        private bool ParseRegister(string aData, out Registers reg)
        {
            return Enum.TryParse(aData, out reg);
        }

        /// <summary>
        /// Attempt to parse the integer contained in a string
        /// as an integer.
        /// </summary>
        /// <param name="aStr">The string to be parsed.</param>
        /// <param name="aNum">
        /// An integer representing the parsed value.
        /// </param>
        /// <returns>
        /// A boolean, true if the parsing yielded a valid integer,
        /// false otherwise.
        /// </returns>
        private bool TryParseInt(string aStr, out int aNum)
        {
            return int.TryParse(aStr, out aNum);
        }

        /// <summary>
        /// Attempt to parse the string as a binary integer.
        /// </summary>
        /// <param name="aStr">The string to be parsed.</param>
        /// <param name="aNum">
        /// An integer representing the parsed value.
        /// </param>
        /// <returns>
        /// A boolean, true if the parsing yielded a valid integer,
        /// false otherwise.
        /// </returns>
        private bool TryParseBinInt(string aStr, out int aNum)
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
        /// A boolean, true if the parsing yielded a valid integer,
        /// false otherwise.
        /// </returns>
        private bool TryParseOctInt(string aStr, out int aNum)
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
        /// A boolean, true if the parsing yielded a valid integer,
        /// false otherwise.
        /// </returns>
        private bool TryParseHexInt(string aStr, out int aNum)
        {
            return
                int.TryParse(aStr,
                             NumberStyles.HexNumber,
                             CultureInfo.CurrentCulture,
                             out aNum);
        }
    }

    internal class ParInstructionData
    {
        public object[] Arguments;
        public InsArgTypes[] ArgRefTypes;
        public string[] BoundLabels;

        public ParInstructionData(object[] aArgs,
                                  InsArgTypes[] aRefTypes,
                                  string[] aLabels)
        {
            Arguments = aArgs;
            ArgRefTypes = aRefTypes;
            BoundLabels = aLabels;
        }
    }
}
