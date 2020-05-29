using System;

namespace VMCore.VM.Core.Utilities
{
    public static class Extensions
    {
        /// <summary>
        /// Convert a type into a friendly type name.
        /// </summary>
        /// <param name="aT">The type to be converted.</param>
        /// <returns>
        /// A string giving the friendly name for the type,
        /// if one has been specified.
        /// </returns>
        public static string GetFriendlyName(this Type aT)
        {
            return aT switch
            {
                Type _ when aT == typeof(int) => "int",
                Type _ when aT == typeof(short) => "short",
                Type _ when aT == typeof(byte) => "byte",
                Type _ when aT == typeof(bool) => "bool",
                Type _ when aT == typeof(long) => "long",
                Type _ when aT == typeof(float) => "float",
                Type _ when aT == typeof(long) => "double",
                Type _ when aT == typeof(float) => "decimal",
                Type _ when aT == typeof(string) => "string",
                Type _ when aT == typeof(Registers) => "Registers",
                Type _ when aT == typeof(OpCode) => "OpCode",
                _ => aT.Name,
            };
        }
    }
}
