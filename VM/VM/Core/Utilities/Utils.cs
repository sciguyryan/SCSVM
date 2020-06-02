#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using VMCore.Assembler;
using VMCore.VM.Core.Register;

namespace VMCore.VM.Core.Utilities
{
    public class Utils
    {
        /// <summary>
        /// Convert an integer into a string representing the flags
        /// that would be set if it were cast to an flags enum.
        /// </summary>
        /// <param name="aEnumType">
        /// The type of the flags enumeration to be referenced.
        /// </param>
        /// <param name="aValue">
        /// An integer, the bits of which should be treated as flags.
        /// </param>
        /// <returns>
        /// A comma delimited string giving the flags set within a
        /// flag-type enum.
        /// </returns>
        /// <remarks>
        /// Intended to be used for the CPU flags register.
        /// Due to the way that they are set out, it is not possible to
        /// directly type-cast. If someone knows of a way then please
        /// do let me know.
        /// </remarks>
        public static string MapIntegerBitsToFlagsEnum(Type aEnumType,
                                                       int aValue)
        {
            if (!aEnumType.IsEnum)
            {
                return string.Empty;
            }

            var enumEntries = aEnumType.GetEnumNames();
            var enumEntriesLen = enumEntries.Length;

            var flagStates = new List<string>();
            for (byte i = 0; i < enumEntriesLen; i++)
            {
                var str =
                    enumEntries[i] + " = " + Utils.IsBitSet(aValue, i);

                flagStates.Add(str);
            }

            return string.Join(", ", flagStates.ToArray());
        }

        /// <summary>
        /// Check if a given bit is set within an integer.
        /// </summary>
        /// <param name="aV">The value to be modified.</param>
        /// <param name="aP">The position of the bit to be modified.</param>
        /// <returns>An integer with the specified bit modified.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(int aV, int aP)
        {
            return (aV & (1 << aP)) != 0;
        }

        /// <summary>
        /// Set a given bit within an integer.
        /// </summary>
        /// <param name="aV">The value to be modified.</param>
        /// <param name="aP">The position of the bit to be modified.</param>
        /// <returns>An integer with the specified bit modified.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SetBit(int aV, int aP)
        {
            return aV | 1 << aP;
        }

        /// <summary>
        /// Clear a given bit within an integer.
        /// </summary>
        /// <param name="aV">The value to be modified.</param>
        /// <param name="aP">The bit to be cleared.</param>
        /// <returns>An integer with the specified bit modified.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ClearBit(int aV, int aP)
        {
            return aV & ~(1 << aP);
        }

        /// <summary>
        /// Set the state of a specific bit within an integer.
        /// </summary>
        /// <param name="aV">The value to be modified.</param>
        /// <param name="aP">The position of the bit to be modified.</param>
        /// <param name="aS">The state to which the bit should be set.</param>
        /// <returns>An integer with the specified bit modified.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SetBitState(int aV, int aP, int aS)
        {
            var mask = 1 << aP;
            return (aV & ~mask) | ((aS << aP) & mask);
        }

        /// <summary>
        /// Cast an object to a given type.
        /// </summary>
        /// <typeparam name="T">
        /// The type that the object should be cast into.
        /// </typeparam>
        /// <param name="aObj">The object to be case.</param>
        /// <returns>An object of the designated type.</returns>
        public static T CastTo<T>(object aObj)
        {
            return (T)aObj;
        }

        /// <summary>
        /// Write data to a binary writer based on the specified type.
        /// </summary>
        /// <param name="aType">
        /// The type of the data being written.
        /// </param>
        /// <param name="aData">
        /// An object representing the data to be written.
        /// </param>
        /// <param name="aBw">
        /// The binary writer into which the data will be written.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// Thrown if the specified type is currently not supported.
        /// </exception>
        public static void WriteDataByType(Type aType,
                                           object aData,
                                           BinaryWriter aBw)
        {
            // This is the cleanest way that I can come up with
            // to do this. It uses the new C# switch pattern
            // matching statement.
            switch (aType)
            {
                case { } when aType == typeof(byte):
                case { } when aType == typeof(Registers):
                    aBw.Write((byte)aData);
                    break;

                case { } when aType == typeof(int):
                    aBw.Write((int)aData);
                    break;

                case { } when aType == typeof(string):
                    var bytes = Encoding.UTF8.GetBytes((string)aData);
                    // Write the number of bytes that made up
                    // the string. This is -not- the string
                    // length but the length of the byte array!
                    aBw.Write(bytes.Length);
                    aBw.Write(bytes);
                    break;

                default:
                    throw new NotSupportedException
                    (
                        $"WriteDataByType: the type {aType} was " +
                        "passed as an argument type, but no support " +
                        "has been provided for that type."
                    );
            }
        }

        /// <summary>
        /// Strip any whitespace characters from a string.
        /// </summary>
        /// <param name="aStr">The string to be processed.</param>
        /// <returns>
        /// A string with any whitespace characters removed.
        /// </returns>
        public static string StripWhiteSpaces(string aStr)
        {
            if (string.IsNullOrWhiteSpace(aStr))
            {
                return aStr;
            }

            var newArr = new char[aStr.Length];

            var j = 0;
            foreach (var c in aStr)
            {
                if (!char.IsWhiteSpace(c))
                {
                    newArr[j++] = c;
                }
            }

            return new string(newArr, 0, j);
        }

        /// <summary>
        /// Builds a binary file with the specified parameters.
        /// </summary>
        /// <param name="aSecs">
        /// An array of the sections to be added to the binary.
        /// </param>
        /// <param name="aVersion">The version of the binary.</param>
        /// <returns>
        /// A RawBinaryWriter containing the specified sections.
        /// </returns>
        public static BinWriter BinFileBuilder(BinSections[]? aSecs = null,
                                               Version? aVersion = null)
        {
            var rbw = new BinWriter();
            var rbi = new BinMeta
            {
                Version = aVersion ?? new Version("1.0.0.0"),
                Id = Guid.NewGuid(),
            };

            rbw.AddMeta(rbi);

            // Create all sections by default if none
            // were provided.
            if (aSecs is null || aSecs.Length == 0)
            {
                aSecs =
                    (BinSections[])Enum.GetValues(typeof(BinSections));
            }

            foreach (var s in aSecs)
            {
                _ = rbw.CreateSection(s);
            }

            return rbw;
        }

        /// <summary>
        /// Compile a list of instructions directly into a binary file.
        /// </summary>
        /// <param name="aIns">
        /// The list of instruction to be compiled.
        /// </param>
        /// <returns>
        /// A byte array containing the bytecode data for the binary file.
        /// </returns>
        public static byte[] QuickFileCompile(QuickIns[] aIns)
        {
            // We are only interested in the code section here.
            var writer =
                BinFileBuilder(new[] { BinSections.Code });

            // Add the compiled opcode instructions to the file section.
            writer.Sections[BinSections.Code].Raw =
                QuickRawCompile(aIns);

            // Return the byte stream.
            return writer.Save();
        }

        /// <summary>
        /// Compile a list of instructions directly into a bytecode array.
        /// </summary>
        /// <param name="aIns">
        /// The list of instruction to be compiled.
        /// </param>
        /// <param name="aOptimize">
        /// A boolean indicating if we should attempt to optimize
        /// the assembled code.
        /// </param>
        /// <returns>
        /// A byte array containing the bytecode data for the program.
        /// </returns>
        public static byte[] QuickRawCompile(QuickIns[] aIns,
                                             bool aOptimize = false)
        {
            var aw = new AsmWriter(aOptimize);

            foreach (var entry in aIns)
            {
                aw.AddWithLabel(entry.Op, entry.Args, entry.Label);
            }

            return aw.Save();
        }

        /// <summary>
        /// Get the current path of this application.
        /// </summary>
        /// <returns>
        /// A string giving the path to the directory of this application.
        /// </returns>
        public static string GetProgramDirectory()
        {
            var loc = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(loc) ?? string.Empty;
        }

        /// <summary>
        /// Write a single line to a log file.
        /// </summary>
        /// <param name="aPath">The path to the log file.</param>
        /// <param name="aOverwrite">
        /// A boolean, true indicating that the file should be
        /// overwritten and false if it should append the data.
        /// </param>
        /// <param name="aArg">
        /// The string to be written to the file.
        /// </param>
        public static void WriteLogFile(string aPath,
                                        bool aOverwrite,
                                        string aArg)
        {
            WriteLogFile(aPath, aOverwrite, new[] { aArg });
        }

        /// <summary>
        /// Writes one or more lines to a log file.
        /// </summary>
        /// <param name="aPath">The path to the log file.</param>
        /// <param name="aOverwrite">
        /// A boolean, true indicating that the file should be
        /// overwritten and false if it should append the data.
        /// </param>
        /// <param name="aArgs">
        /// The strings to be written to the file, one line per entry.
        /// </param>
        public static void WriteLogFile(string aPath,
                                        bool aOverwrite,
                                        params string[] aArgs)
        {
            using var sw = new StreamWriter(aPath, !aOverwrite);
            foreach (var s in aArgs)
            {
                sw.WriteLine(s);
            }
            sw.Close();
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
        public static bool TryParseBinInt(string aStr, out int aNum)
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
        public static bool TryParseOctInt(string aStr, out int aNum)
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
        public static bool TryParseHexInt(string aStr, out int aNum)
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
        public static bool TryParseInt(string aStr, out int aNum)
        {
            return int.TryParse(aStr, out aNum);
        }
    }
}
