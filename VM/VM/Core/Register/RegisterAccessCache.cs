using System;
using System.Collections.Generic;

namespace VMCore.VM.Core.Reg
{
    public static class RegisterAccessCache
    {
        /// <summary>
        /// A dictionary mapping the access flags to their respective position
        /// within the enum. Used for bitshifting.
        /// </summary>
        public static Dictionary<RegisterAccess, int> FlagIndicies
            = new Dictionary<RegisterAccess, int>();

        /// <summary>
        /// Build the flag cache.
        /// </summary>
        public static void BuildCache()
        {
            if (FlagIndicies.Count > 0)
            {
                return;
            }

            var flags =
                (RegisterAccess[])Enum.GetValues(typeof(RegisterAccess));
            for (var i = 0; i < flags.Length; i++)
            {
                FlagIndicies.Add(flags[i], i);
            }
        }
    }
}
