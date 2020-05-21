using System;

namespace VMCore.VM.Core.Utilities
{
    public static class Extensions
    {
        /// <summary>
        /// Convert a type into a friendly type name.
        /// </summary>
        /// <param name="t">The type to be converted.</param>
        /// <returns>
        /// A string giving the friendly name for the type,
        /// if one has been specified.
        /// </returns>
        public static string GetFriendlyName(this Type t)
        {
            return t switch
            {
                Type _ when t == typeof(int) => "int",
                Type _ when t == typeof(short) => "short",
                Type _ when t == typeof(byte) => "byte",
                Type _ when t == typeof(bool) => "bool",
                Type _ when t == typeof(long) => "long",
                Type _ when t == typeof(float) => "float",
                Type _ when t == typeof(long) => "double",
                Type _ when t == typeof(float) => "decimal",
                Type _ when t == typeof(string) => "string",
                Type _ when t == typeof(Registers) => "Registers",
                Type _ when t == typeof(OpCode) => "OpCode",
                _ => t.Name,
            };
        }
    }
}
