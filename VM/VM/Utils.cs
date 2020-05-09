using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using VMCore.Assembler;

namespace VMCore.VM
{
    public class Utils
    {
        /// <summary>
        /// Convert an integer into a string representing the flags
        /// that would be set if it were cast to an flags enum.
        /// </summary>
        /// <param name="enumType">The type of the flags enumeration to be referenced.</param>
        /// <param name="value">An integer, the bits of which should be treated as flags.</param>
        /// <returns></returns>
        /// <remarks>
        /// Intended to be used for the CPU flags register.
        /// Due to the way that they are set out, it is not possible to
        /// directly type-cast. If someone knows of a way then please
        /// do let me know.
        /// </remarks>
        public static string MapIntegerBitsToFlagsEnum(Type enumType, int value)
        {
            if (!enumType.IsEnum)
            {
                // TODO - might need to handle this a bit better.
                return "";
            }

            var enumEntries = enumType.GetEnumNames();
            var enumEntriesLen = enumEntries.Length;

            var flagStates = new List<string>();
            for (byte i = 0; i < enumEntriesLen; i++)
            {
                flagStates.Add(enumEntries[i] + " = " + Utils.IsBitSet(value, i));
            }

            return string.Join(", ", flagStates.ToArray());
        }

        /// <summary>
        /// Check if a given bit is set within an integer.
        /// </summary>
        /// <param name="v">The value to be modified.</param>
        /// <param name="p">The position of the bit to be modified.</param>
        /// <returns>An integer with the specified bit modified.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(int v, int p)
        {
            return (v & (1 << p)) != 0;
        }

        /// <summary>
        /// Set a given bit within an integer.
        /// </summary>
        /// <param name="v">The value to be modified.</param>
        /// <param name="p">The position of the bit to be modified.</param>
        /// <returns>An integer with the specified bit modified.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SetBit(int v, int p)
        {
            return v | 1 << p;
        }

        /// <summary>
        /// Clear a given bit within an integer.
        /// </summary>
        /// <param name="position">The bit to be cleared.</param>
        /// <returns>An integer with the specified bit modified.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ClearBit(int value, int position)
        {
            return value & ~(1 << position);
        }

        /// <summary>
        /// Set the state of a specific bit within an integer.
        /// </summary>
        /// <param name="v">The value to be modified.</param>
        /// <param name="p">The position of the bit to be modified.</param>
        /// <param name="s">The state to which the bit should be set.</param>
        /// <returns>An integer with the specified bit modified.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SetBitState(int v, int p, int s)
        {
            var mask = 1 << p;
            return (v & ~mask) | ((s << p) & mask);
        }

        /// <summary>
        /// Cast an object to a given type.
        /// </summary>
        /// <typeparam name="T">The type that the object should be cast into.</typeparam>
        /// <param name="obj">The object to be case.</param>
        /// <returns>An object of the designated type.</returns>
        public static T CastTo<T>(object obj)
        {
            return (T)obj;
        }

        /// <summary>
        /// Read data from a binary reader based on the specified type.
        /// </summary>
        /// <param name="type">The type of the data being written.</param>
        /// <param name="br">The binary reader from which the data will be read.</param>
        /// <returns>An object representing the read data.</returns>
        /// <exception>NotSupportedException if the specified type is currently not supported.</exception>
        public static object ReadDataByType(Type type, BinaryReader br)
        {
            // This is the cleanest way that I can come up with
            // to do this. It uses the new C# switch pattern
            // matching statement.
            return type switch
            {
                Type _ when type == typeof(byte)        => br.ReadByte(),
                Type _ when type == typeof(int)         => br.ReadInt32(),
                Type _ when type == typeof(string)      => br.ReadString(),
                Type _ when type == typeof(Registers)   => (Registers)(int)br.ReadByte(),
                _                                       => throw new NotSupportedException($"ReadDataByType: the type {type} was passed as an argument type, but no support has been given provided for that type.")
            };
        }

        /// <summary>
        /// Write data to a binary writer based on the specified type.
        /// </summary>
        /// <param name="type">The type of the data being written.</param>
        /// <param name="data">An object representing the data to be written.</param>
        /// <param name="bw">The binary writer into which the data will be written.</param>
        /// <param name="isExprArg">A boolean indicating if the argument is to be treated as an expression.</param>
        /// <exception>NotSupportedException if the specified type is currently not supported.</exception>
        public static void WriteDataByType(Type type, object data, BinaryWriter bw, Type exprArg = null)
        {
            // This is the cleanest way that I can come up with
            // to do this. It uses the new C# switch pattern
            // matching statement.
            switch (type)
            {
                case Type _ when type == typeof(byte):
                case Type _ when type == typeof(Registers):
                    bw.Write((byte)data);
                    break;

                case Type _ when type == typeof(int):
                    bw.Write((int)data);
                    break;

                case Type _ when type == typeof(string):
                    bw.Write((string)data);
                    break;

                default:
                    throw new NotSupportedException($"WriteDataByType: the type {type} was passed as an argument type, but no support has been provided for that type.");
                    break;
            }
        }

        /// <summary>
        /// Strip any whitespace characters from a string.
        /// </summary>
        /// <param name="s">The string to be processed.</param>
        /// <returns>A string with any of the whitespaces removed.</returns>
        public static string StripWhiteSpaces(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            char[] newArr = 
                new char[s.Length];

            var j = 0;
            foreach (var c in s)
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
        /// <param name="sections">An array of the sections to be added to the binary.</param>
        /// <param name="version">The version of the binary.</param>
        /// <returns>A RawBinaryWriter containing the specified sections.</returns>
        public static RawBinaryWriter BinaryFileBuilder(RawBinarySections[] sections = null, Version version = null)
        {
            var rbw = new RawBinaryWriter();
            var rbi = new RawBinaryMeta
            {
                Version = version ?? new Version("1.0.0.0"),
                ID = Guid.NewGuid(),
            };

            rbw.AddMeta(rbi);

            // Create all sections by default if none
            // were provided.
            if (sections == null || sections.Length == 0)
            {
                sections = (RawBinarySections[])Enum.GetValues(typeof(RawBinarySections));
            }

            foreach (var s in sections)
            {
                _ = rbw.CreateSection(s);
            }

            return rbw;
        }

        /// <summary>
        /// Compile a list of instructions directly into a binary file.
        /// </summary>
        /// <param name="instructions">The list of instruction to be compiled.</param>
        /// <returns>A byte array containing the bytecode data for the binary file.</returns>
        public static byte[] QuickFileCompile(List<QuickInstruction> instructions)
        {
            // We are only interested in the code section here.
            RawBinaryWriter writer = 
                Utils.BinaryFileBuilder(new[] { RawBinarySections.Code });

            // Add the compiled opcode instructions to the file section.
            writer.Sections[RawBinarySections.Code].Raw = 
                Utils.QuickRawCompile(instructions);

            // Return the byte stream.
            return writer.Save();
        }

        /// <summary>
        /// Compile a list of instructions directly into a bytecode array.
        /// </summary>
        /// <param name="instructions">The list of instruction to be compiled.</param>
        /// <param name="optimize">A boolean indicating if we should attempt to optimize the assembled code.</param>
        /// <returns>A byte array containing the bytecode data for the program.</returns>
        public static byte[] QuickRawCompile(List<QuickInstruction> instructions, bool optimize = false)
        {
            var aw = new AsmWriter(optimize);

            foreach (var entry in instructions)
            {
                aw.AddWithLabel(entry.Op, entry.Args, entry.Label);
            }

            return aw.Save();
        }

        /// <summary>
        /// Get the current path of this application.
        /// </summary>
        /// <returns>A string giving the path to the directory of this application.</returns>
        public static string GetProgramDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        /// <summary>
        /// Write a single line to a log file.
        /// </summary>
        /// <param name="path">The path to the log file.</param>
        /// <param name="overwrite">A boolean, true indicating that the file should be overwritten and false if it should append the data.</param>
        /// <param name="arg">The string to be written to the file.</param>
        public static void WriteLogFile(string path, bool overwrite, string arg)
        {
            Utils.WriteLogFile(path, overwrite, new[] { arg });
        }

        /// <summary>
        /// Writes one or more lines to a log file.
        /// </summary>
        /// <param name="path">The path to the log file.</param>
        /// <param name="overwrite">A boolean, true indicating that the file should be overwritten and false if it should append the data.</param>
        /// <param name="args">The strings to be written to the file, one line per entry.</param>
        public static void WriteLogFile(string path, bool overwrite, params string[] args)
        {
            using var sw = new StreamWriter(path, !overwrite);
            foreach (var s in args)
            {
                sw.WriteLine(s);
            }
            sw.Close();
        }
    }
}
