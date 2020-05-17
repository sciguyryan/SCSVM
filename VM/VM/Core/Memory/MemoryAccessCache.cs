using System;
using System.Collections.Generic;

namespace VMCore.VM.Core.Mem
{
    public static class MemAccessCache
    {
        /// <summary>
        /// A dictionary mapping the access flags to their respective
        /// position within the enum. Used for bitshifting.
        /// </summary>
        public static Dictionary<MemoryAccess, int> FlagIndicies
            = new Dictionary<MemoryAccess, int>();

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
                (MemoryAccess[])Enum.GetValues(typeof(MemoryAccess));
            for (var i = 0; i < flags.Length; i++)
            {
                FlagIndicies.Add(flags[i], i);
            }
        }
    }
}
