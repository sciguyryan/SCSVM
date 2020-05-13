using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using VMCore.VM.Core.Exceptions;

namespace VMCore.VM.Core.Mem
{
    public class Memory
    {
        public int Length
        {
            get
            {
                return Data.Length;
            }
        }

        public int BaseMemorySize { get; private set; }

        /// <summary>
        /// The byte array representing the system memory.
        /// </summary>
        private byte[] Data { get; set; }
            = new byte[0];

        /// <summary>
        /// A list of memory regions and their associated permissions.
        /// </summary>
        private List<MemoryRegion> _memoryRegions = 
            new List<MemoryRegion>();

        /// <summary>
        /// An internal counter for the memory sequence IDs.
        /// </summary>
        private int _seqID = 0;

        /// <summary>
        /// A dictionary mapping the access flags to their respective position
        /// within the enum. Used for bitshifting.
        /// </summary>
        private Dictionary<MemoryAccess, int> _flagIndicies
            = new Dictionary<MemoryAccess, int>();

        public Memory(int aCapacity = 2048)
        {
            BaseMemorySize = aCapacity;

            // TODO - this needs to be in a try-catch as
            // it can fail.
            Data = new byte[aCapacity];

            // By default the read and write permissions are set
            // for the entire memory block.
            AddMemoryRegion(0,
                            aCapacity-1,
                            MemoryAccess.R | MemoryAccess.W);

            var flags = (MemoryAccess[])Enum.GetValues(typeof(MemoryAccess));
            for (var i = 0; i < flags.Length; i++)
            {
                _flagIndicies.Add(flags[i], i);
            }
        }

        /// <summary>
        /// Load a pre-populated memory block into the system memory.
        /// </summary>
        /// <param name="aPayload">The byte array used to represent the system memory.</param>
        public Memory(byte[] aPayload)
        {
            Data = aPayload;
        }

        /// <summary>
        /// Clear the current system memory.
        /// </summary>
        public void Clear()
        {
            new Span<byte>(Data).Fill(0);
        }

        /// <summary>
        /// Remove any executable regions of memory that have been allocated.
        /// </summary>
        public void RemoveExecutableRegions()
        {
            // Resize the memory back to the original.
            // This will get rid of any executable
            // memory blocks that would come at the end.
            Data = new byte[BaseMemorySize];

            // Remove any executable memory regions.
            var tmp = _memoryRegions.ToArray();
            foreach (var item in tmp)
            {
                if (IsFlagSet(item.Access, MemoryAccess.EX))
                {
                    _memoryRegions.Remove(item);
                }
            }
        }

        /// <summary>
        /// Create an executable memory region and load
        /// the provided binary data into it.
        /// </summary>
        /// <param name="aData">The bytecode data to be loaded into the memory region.</param>
        /// <returns>
        /// A tuple of the start and end addresses of the executable region
        /// and the unique sequence ID for the memory region.
        /// </returns>
        public (int start, int end, int seqID) SetupExMemory(byte[] aData)
        {
            var memLen = Data.Length;
            var exLen = aData.Length;
            var newMemLen = memLen + exLen;

            // Resize the memory to the new size required.
            Data = new byte[newMemLen];

            // Add an executable memory region for the
            // region that will contain the executable
            // code.
            var flags = MemoryAccess.R |
                        MemoryAccess.W |
                        MemoryAccess.EX;
            var seqID = 
                AddMemoryRegion(memLen,
                                newMemLen - 1,
                                flags);

            Array.Copy(aData, 0, Data, memLen, aData.Length);

            return (memLen, newMemLen, seqID);
        }

        /// <summary>
        /// Add a memory region to the memory region permission list.
        /// </summary>
        /// <param name="aStart">The starting position of the memory region.</param>
        /// <param name="aEnd">The ending position of the memory region. </param>
        /// <param name="aAccess">The access flags to be applied to the region.</param>
        /// <returns>The sequence ID that uniquely represents the memory region.</returns>
        public int AddMemoryRegion(int aStart,
                                   int aEnd,
                                   MemoryAccess aAccess)
        {
            var region = 
                new MemoryRegion(aStart, aEnd, aAccess, _seqID);
            _memoryRegions.Add(region);

            ++_seqID;

            return region.SeqID;
        }

        /// <summary>
        /// Gets a memory region with a given sequence identifier.
        /// </summary>
        /// <param name="aSeqID">The sequence identifier to be checked.</param>
        public MemoryRegion GetMemoryRegion(int aSeqID)
        {
            for (var i = 0; i < _memoryRegions.Count; i++)
            {
                if (_memoryRegions[i].SeqID == aSeqID)
                {
                    return _memoryRegions[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Remove a region or regions of memory that contain a given position.
        /// </summary>
        /// <param name="aPoint">The position within memory to target.</param>
        /// <param name="aRemoveAll">A boolean, true if all matching memory regions should be removed, false otherwise.</param>
        public void RemoveMemoryRegion(int aPoint,
                                       bool aRemoveAll = false)
        {
            // We want to iterate this list in reverse as
            // we only want to remove the -last- entry only.

            MemoryRegion region;
            var regionID = _memoryRegions.Count - 1;
            while (regionID >= 0)
            {
                region = _memoryRegions[regionID];
                if (aPoint >= region.Start && aPoint <= region.End)
                {
                    _memoryRegions.RemoveAt(regionID);

                    if (!aRemoveAll)
                    {
                        break;
                    }
                }

                --regionID;
            }
        }

        /// <summary>
        /// Remove a memory region with a given sequence identifier.
        /// </summary>
        /// <param name="aSeqID">The sequence identifier to be checked.</param>
        public void RemoveMemoryRegion(int aSeqID)
        {
            _memoryRegions.RemoveAll(x => x.SeqID == aSeqID);
        }

        /// <summary>
        /// Gets the permissions for a specified memory region.
        /// </summary>
        /// <param name="aStart">The start of the memory region range.</param>
        /// <param name="aEnd">The end of the memory region range.</param>
        /// <returns>The permission flags for the memory region.</returns>
        public MemoryAccess GetMemoryPermissions(int aStart, int aEnd)
        {
            var regions = _memoryRegions.ToArray();
            var regionID = regions.Length - 1;

            var access = MemoryAccess.R | MemoryAccess.W;

            // We want to iterate this list in reverse as the last entry
            // can override those entered before it.
            while (regionID >= 0)
            {
                var region = regions[regionID];
                if (aStart <= region.End && region.Start <= aEnd)
                {
                    access = region.Access;
                    break;
                }

                --regionID;
            }

            return access;
        }

        /// <summary>
        /// Read an integer from memory.
        /// </summary>
        /// <param name="aStartPos">The location of the first byte of data to be read.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        /// <returns>An integer derived from the value read from memory.</returns>
        public int GetInt(int aStartPos,
                          SecurityContext aContext,
                          bool aExec)
        {
            var bytes =
                GetValueRange(aStartPos, sizeof(int), aContext, aExec);

            return BitConverter.ToInt32(bytes);
        }

        /// <summary>
        /// Read an opcode from memory.
        /// </summary>
        /// <param name="aStartPos">The location of the first byte of data to be read.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        /// <returns>An opcode derived from the value read from memory.</returns>
        public OpCode GetOpCode(int aStartPos,
                                SecurityContext aContext,
                                bool aExec)
        {
            return (OpCode)GetInt(aStartPos, aContext, aExec);
        }

        /// <summary>
        /// Read a register identifier from memory.
        /// </summary>
        /// <param name="aStartPos">The location of the first byte of data to be read.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        /// <returns>A register identifier derived from the value read from memory.</returns>
        public Registers GetRegister(int aStartPos,
                                     SecurityContext aContext,
                                     bool aExec)
        {
            return (Registers)GetValue(aStartPos, aContext, aExec);
        }

        /// <summary>
        /// Read a string from memory.
        /// </summary>
        /// <param name="aStartPos">The location of the first byte of data to be read.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        /// <returns>
        /// A tuple giving. The first value indicating how many bytes in total
        /// were read in order to construct the string.
        /// The second value being the string that was read from memory.
        /// </returns>
        public (int, string) GetString(int aStartPos,
                                       SecurityContext aContext,
                                       bool aExec)
        {
            // This is the number of bytes
            // that made up the string, not the
            // string length.
            var bytesCount = 
                GetInt(aStartPos, aContext, aExec);

            // We need to skip over the length
            // of the string length indicator
            // as we do not want that data to contaminate
            // out string.
            var bytes = 
                GetValueRange(aStartPos + sizeof(int),
                              bytesCount,
                              aContext,
                              aExec);

            // The number of bytes used to build the string
            // and the string.
            return (bytesCount + sizeof(int),
                    Encoding.UTF8.GetString(bytes));
        }

        /// <summary>
        /// Read a single byte from memory.
        /// </summary>
        /// <param name="aPos">The location of the byte to retrieve.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        /// <returns>A single byte from memory.</returns>
        /// <exception>MemoryAccessViolationException if the specified permission flag is not set for the memory region.</exception>
        public byte GetValue(int aPos,
                             SecurityContext aContext,
                             bool aExec)
        {
            if (aPos < 0 || aPos > Data.Length)
            {
                throw new MemoryOutOfRangeException($"GetValue: the specified memory location is negative and is therefore invalid.");
            }

            ValidateAccess(aPos,
                           aPos,
                           DataAccessType.Read,
                           aContext,
                           aExec);

            return Data[aPos];
        }

        /// <summary>
        /// Reads a range of bytes from memory.
        /// </summary>
        /// <param name="aPos">The location of the first byte to retrieve.</param>
        /// <param name="aLength">The number of bytes to retrieve.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        /// <returns>An array of bytes from memory.</returns>
        /// <exception>MemoryAccessViolationException if the specified permission flag is not set for the memory region.</exception>
        public byte[] GetValueRange(int aPos,
                                    int aLength,
                                    SecurityContext aContext,
                                    bool aExec)
        {
            if (aPos < 0 || aPos > Data.Length)
            {
                throw new MemoryOutOfRangeException($"GetValueRange: the specified memory location is negative and is therefore invalid.");
            }

            ValidateAccess(aPos,
                           aPos + aLength,
                           DataAccessType.Read,
                           aContext,
                           aExec);

            return new Span<byte>(Data).Slice(aPos, aLength).ToArray();
        }

        /// <summary>
        /// Sets a single byte in memory.
        /// </summary>
        /// <param name="aPos">The location of the byte to set.</param>
        /// <param name="aValue">The value of the byte to be set.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        /// <exception>MemoryAccessViolationException is the specified permission flag is not set for the memory region.</exception>
        public void SetValue(int aPos,
                             byte aValue,
                             SecurityContext aContext,
                             bool aExec)
        {
            if (aPos < 0 || aPos > Data.Length)
            {
                throw new MemoryOutOfRangeException($"SetValue: the specified memory location is negative and is therefore invalid.");
            }

            ValidateAccess(aPos,
                           aPos,
                           DataAccessType.Write,
                           aContext,
                           aExec);

            Data[aPos] = aValue;
        }

        /// <summary>
        /// Sets a range of bytes in memory.
        /// </summary>
        /// <param name="aPos">The location of the first byte to be written.</param>
        /// <param name="aBytes">The value of the byte to be set.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        /// <exception>MemoryAccessViolationException is the specified permission flag is not set for the memory region.</exception>
        public void SetValueRange(int aPos,
                                  byte[] aBytes,
                                  SecurityContext aContext,
                                  bool aExec)
        {
            if (aPos < 0 || aPos > Data.Length)
            {
                throw new MemoryOutOfRangeException($"SetValueRange: the specified memory location is negative and is therefore invalid.");
            }

            ValidateAccess(aPos,
                           aPos + aBytes.Length,
                           DataAccessType.Write,
                           aContext,
                           aExec);

            // Sigh... why couldn't array ranges for writing too :(
            for (var i = 0; i < aBytes.Length; i++)
            {
                Data[aPos + i] = aBytes[i];
            }
        }

        /// <summary>
        /// Check if a given flag is set.
        /// </summary>
        /// <param name="aFlags">The flag value to be checked against.</param>
        /// <param name="aFlag">The flag ID to be checked.</param>
        /// <returns>A boolean, true if the flag is set, false otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFlagSet(MemoryAccess aFlags, MemoryAccess aFlag)
        {
            return
                Utils.IsBitSet((int)aFlags,
                               _flagIndicies[aFlag]);
        }

        /// <summary>
        /// Checks if a given range of memory has a flag set.
        /// Using a system-level security context will always grant access.
        /// </summary>
        /// <param name="aStart">The starting location of the memory region.</param>
        /// <param name="aEnd">The ending location of the memory region.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aType">The data access type to check.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        /// <exception>MemoryAccessViolationException if the specified permission flag is not set for the memory region.</exception>
        private void ValidateAccess(int aStart,
                                    int aEnd,
                                    DataAccessType aType,
                                    SecurityContext aContext,
                                    bool aExec)
        {
            var flags = GetMemoryPermissions(aStart, aEnd);

            bool hasFlags;
            if (aType == DataAccessType.Read)
            {
                hasFlags =
                    IsFlagSet(flags, MemoryAccess.R) ||
                    (IsFlagSet(flags, MemoryAccess.PR) &&
                     aContext == SecurityContext.System);
            }
            else if (aType == DataAccessType.Write)
            {
                hasFlags =
                    IsFlagSet(flags, MemoryAccess.W) ||
                    (IsFlagSet(flags, MemoryAccess.PW) &&
                     aContext == SecurityContext.System);
            }
            else
            {
                throw new NotSupportedException($"ValidateAccess: attempted to check a non-valid data access type.");
            }

            if (aExec)
            {
                hasFlags &= 
                    IsFlagSet(flags, MemoryAccess.EX);
            }

            if (!hasFlags)
            {
                throw new MemoryAccessViolationException($"ValidateAccess: attempted to access a register without the correct security context or access flags. Access Type = {aType}, flags = {flags}");
            }
        }
    }
}
