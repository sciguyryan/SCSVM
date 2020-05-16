using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            ResizeRootMemoryRegion();
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
        public (int start, int end, int seqID) AddExMemory(byte[] aData)
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
                                newMemLen,
                                flags);

            Array.Copy(aData, 0, Data, memLen, aData.Length);

            ResizeRootMemoryRegion();

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

            ResizeRootMemoryRegion();

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
        /// The root memory region (seqID == 0) will not be removed.
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

            // We never want to remove the root memory region.
            while (regionID >= 1)
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

            ResizeRootMemoryRegion();
        }

        /// <summary>
        /// Remove a memory region with a given sequence identifier.
        /// The root memory region (seqID == 0) cannot be removed.
        /// </summary>
        /// <param name="aSeqID">The sequence identifier to be checked.</param>
        public void RemoveMemoryRegion(int aSeqID)
        {
            if (aSeqID == 0)
            {
                return;
            }

            _memoryRegions.RemoveAll(x => x.SeqID == aSeqID);

            ResizeRootMemoryRegion();
        }

        /// <summary>
        /// Directly get a range of bytes from memory. Do not use in anything other than 
        /// internal code that does not need to take account of memory permissions!
        /// </summary>
        /// <param name="aStart">The start of the memory region range.</param>
        /// <param name="aEnd">The end of the memory region range.</param>
        /// <returns>An array of bytes representing the bytes extracted from memory.</returns>
        public byte[] DirectGetMemoryRaw(int aStart, int aEnd)
        {
            return Data[aStart..aEnd];
        }

        /// <summary>
        /// Gets the permissions for a specified memory region.
        /// </summary>
        /// <param name="aStart">The start of the memory region range.</param>
        /// <param name="aEnd">The end of the memory region range.</param>
        /// <returns>An array of the regions within which the memory range will fall.</returns>
        public MemoryRegion[] GetMemoryPermissions(int aStart, int aEnd)
        {
            var regions = _memoryRegions.ToArray();
            var regionID = regions.Length - 1;

            // We want to iterate this list in reverse as the last entry
            // can override those entered before it.
            var matched = new List<MemoryRegion>();
            while (regionID >= 0)
            {
                var region = regions[regionID];

                if (aStart >= region.Start && aEnd <= region.End)
                {
                    // We have a match where the range is -completely-
                    // within a region. No cross-region permission issues
                    // can arise here.
                    matched.Add(region);
                    break;
                }
                else if (aStart <= region.End && region.Start <= aEnd)
                {
                    // We have a cross-region match.
                    // We will have to do some additional checks
                    // to ensure that we assign the correct permissions.
                    matched.Add(region);
                }

                --regionID;
            }

            if (matched.Count > 0)
            {
                return matched.ToArray();
            }

            // This cannot happen with any valid address as
            // the root memory region will always match a valid address.
            throw new MemoryAccessViolationException($"GetMemoryPermissions: attempted to access a memory region that does not exist. Start = {aStart}, End = {aEnd}.");
        }

        #region Integer IO
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
        /// Writes an integer to memory.
        /// </summary>
        /// <param name="aStartPos">The location of the first byte of data to be written.</param>
        /// <param name="aValue">The integer value to be written to memory.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        public void SetInt(int aStartPos,
                           int aValue,
                           SecurityContext aContext,
                           bool aExec)
        {
            var bytes =
                BitConverter.GetBytes(aValue);

            SetValueRange(aStartPos, bytes, aContext, aExec);
        }
        #endregion

        #region OpCode IO
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
        /// Writes an opcode to memory.
        /// </summary>
        /// <param name="aStartPos">The location of the first byte of data to be written.</param>
        /// <param name="aValue">The opcode to be written to memory.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        public void SetOpCode(int aStartPos,
                              OpCode aValue,
                              SecurityContext aContext,
                              bool aExec)
        {
            var bytes =
                BitConverter.GetBytes((int)aValue);

            SetValueRange(aStartPos, bytes, aContext, aExec);
        }
        #endregion

        #region Register Identifier IO
        /// <summary>
        /// Read a register identifier from memory.
        /// </summary>
        /// <param name="aStartPos">The location of the first byte of data to be read.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        /// <returns>A register identifier derived from the value read from memory.</returns>
        public Registers GetRegisterIdent(int aStartPos,
                                          SecurityContext aContext,
                                          bool aExec)
        {
            return (Registers)GetValue(aStartPos, aContext, aExec);
        }

        /// <summary>
        /// Writes a register identifier to memory.
        /// </summary>
        /// <param name="aStartPos">The location of the first byte of data to be written.</param>
        /// <param name="aValue">The register identifier to be written to memory.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        public void SetRegisterIdent(int aStartPos,
                                     Registers aValue,
                                     SecurityContext aContext,
                                     bool aExec)
        {
            SetValue(aStartPos, (byte)aValue, aContext, aExec);
        }
        #endregion

        #region String IO
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
        /// Writes a string to memory.
        /// </summary>
        /// <param name="aStartPos">The location of the first byte of data to be written.</param>
        /// <param name="aValue">The string to be written to memory.</param>
        /// <param name="aContext">The security context for this request.</param>
        /// <param name="aExec">A boolean indicating if this value must be within an executable memory region.</param>
        public void SetString(int aStartPos,
                              string aValue,
                              SecurityContext aContext,
                              bool aExec)
        {
            // Write the length of the string first.
            var bytes = BitConverter.GetBytes(aValue.Length);

            SetValueRange(aStartPos, bytes, aContext, aExec);

            // Write the string directly afterwards.
            bytes = Encoding.UTF8.GetBytes(aValue);

            SetValueRange(aStartPos + sizeof(int),
                          bytes,
                          aContext,
                          aExec);
        }
        #endregion

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
        /// <exception>MemoryAccessViolationException if the specified permissions are not valid to perform the operation on the memory region.</exception>
        /// <exception>MemoryOutOfRangeException if the specified position falls outside of the valid memory bounds.</exception>
        private void ValidateAccess(int aStart,
                                    int aEnd,
                                    DataAccessType aType,
                                    SecurityContext aContext,
                                    bool aExec)
        {
            if (aStart < 0 || aStart > Data.Length ||
                aEnd < 0 || aEnd > Data.Length)
            {
                throw new MemoryOutOfRangeException($"ValidateAccess: the specified memory location is outside of the memory bounds.");
            }

            // If we have an address range that intersects one or more
            // memory regions then we need to choose the access flags
            // from the region that has the highest permissions of those
            // that were returned.
            // The logic being that the highest permissions will need
            // to be met for access to be granted to any point within
            // the range.

            bool hasFlags = true;
            MemoryAccess flags = MemoryAccess.N;
            foreach (var r in GetMemoryPermissions(aStart, aEnd))
            {
                // If we have requested an executable memory
                // region and this region is not executable
                // then we cannot have a match.
                if (aExec && !IsFlagSet(r.Access, MemoryAccess.EX))
                {
                    // In this instance we will now allow the
                    // operation to continue.
                    // We cannot permit writing or reading
                    // from non-executable memory into executable
                    // memory and vice versa.
                    hasFlags = false;
                    break;
                }

                if (r.Access > flags)
                {
                    flags = r.Access;
                }
            }

            if (aType == DataAccessType.Read)
            {
                hasFlags &=
                    IsFlagSet(flags, MemoryAccess.R) ||
                    (IsFlagSet(flags, MemoryAccess.PR) &&
                     aContext == SecurityContext.System);
            }
            else if (aType == DataAccessType.Write)
            {
                hasFlags &=
                    IsFlagSet(flags, MemoryAccess.W) ||
                    (IsFlagSet(flags, MemoryAccess.PW) &&
                     aContext == SecurityContext.System);
            }
            else
            {
                throw new NotSupportedException($"ValidateAccess: attempted to check a non-valid data access type.");
            }

            if (!hasFlags)
            {
                throw new MemoryAccessViolationException($"ValidateAccess: attempted to access a memory without the correct security context or access flags. Access Type = {aType}, Executable = {aExec}, flags = {flags}");
            }
        }

        /// <summary>
        /// Resize the root memory region to equal the maximum
        /// memory bound.
        /// </summary>
        private void ResizeRootMemoryRegion()
        {
            var maxEnd = 0;
            foreach (var r in _memoryRegions)
            {
                if (r.End > maxEnd)
                {
                    maxEnd = r.End;
                }
            }

            _memoryRegions[0].End = maxEnd;
        }

        /// <summary>
        /// Debugging function to view the list of memory regions,
        /// their bounds and associated permission flags.
        /// </summary>
        private void DebugMemoryRegions()
        {
            Debug.WriteLine(new string('-', 68));
            foreach (var r in _memoryRegions)
            {
                var s =
                    String.Format("|{0,20} | {1,20} | {2,20}|",
                                  $"{r.Start},{r.End}",
                                  r.Access,
                                  r.SeqID);
                Debug.WriteLine(s);
            }
            Debug.WriteLine(new string('-', 68));
        }
    }
}
