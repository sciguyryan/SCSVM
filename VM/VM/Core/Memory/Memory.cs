#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Utilities;

namespace VMCore.VM.Core.Memory
{
    public class Memory
    {
        #region Public Properties

        /// <summary>
        /// The total size of the memory in bytes.
        /// </summary>
        public int Length
        {
            get
            {
                return Data.Length;
            }
        }

        /// <summary>
        /// The base size of the main memory block.
        /// This is the size of the memory that does
        /// not include any executable regions.
        /// </summary>
        public int BaseMemorySize { get; private set; }

        /// <summary>
        /// A list of the types of data currently held
        /// within the stack.
        /// </summary>
        public Stack<Type> StackTypes = new Stack<Type>();

        /// <summary>
        /// The starting point (in memory) of the stack memory region.
        /// </summary>
        public int StackStart;

        /// <summary>
        /// The end point (in memory) of the stack memory region.
        /// </summary>
        public int StackEnd;

        /// <summary>
        /// The next point in memory available for writing
        /// data.
        /// </summary>
        public int StackPointer;

        #endregion // Public Properties

        #region Private Properties

        /// <summary>
        /// The byte array representing the system memory.
        /// </summary>
        private byte[] Data { get; set; }

        /// <summary>
        /// A list of memory regions and their associated permissions.
        /// </summary>
        private readonly List<MemoryRegion> _memoryRegions = 
            new List<MemoryRegion>();

        /// <summary>
        /// An internal counter for the memory sequence IDs.
        /// </summary>
        private int _seqId;

#if DEBUG
        private bool IsDebuggingEnabled { get; set; } = true;
#else
        private bool IsDebuggingEnabled { get; set; } = false;
#endif

        #endregion // Private Properties

        public Memory(int aMainMemorySize = 2048,
                      int aStackCapacity = 100)
        {
            var stackSize = (aStackCapacity * sizeof(int));

            // The final memory size is equal to the base memory
            // capacity plus the stack capacity.
            var memoryCapacity =
                aMainMemorySize + stackSize;

            BaseMemorySize = memoryCapacity;

            Data = new byte[memoryCapacity];

            // Read and write permissions are set
            // for the entire root memory block.
            AddMemoryRegion(0,
                            memoryCapacity - 1,
                            MemoryAccess.R | MemoryAccess.W,
                            "Root");

            // The region directly after the main memory
            // is reserved for the stack memory.
            // The stack memory region should be marked
            // as no read/write as the only methods
            // accessing or modifying it should be system only.
            StackStart = aMainMemorySize;
            StackEnd = memoryCapacity;
            AddMemoryRegion(StackStart,
                            StackEnd,
                            MemoryAccess.PR | MemoryAccess.PW,
                            "Stack");

            // We always start working with the stack at
            // the bottom of the memory range.
            StackPointer = StackEnd;
        }

        /// <summary>
        /// Load a pre-populated memory block into the system memory.
        /// </summary>
        /// <param name="aPayload">
        /// The byte array used to represent the system memory.
        /// </param>
        public Memory(byte[] aPayload)
        {
            Data = aPayload;
        }

        /// <summary>
        /// Enable or disable debugging functionality within
        /// this memory instance.
        /// </summary>
        /// <param name="aEnabled">
        /// The debugging state of this memory instance.
        /// </param>
        public void SetDebuggingEnabled(bool aEnabled)
        {
            IsDebuggingEnabled = aEnabled;
        }

        /// <summary>
        /// Clear the current system memory.
        /// </summary>
        public void Clear()
        {
            new Span<byte>(Data).Fill(0);
        }

        /// <summary>
        /// Remove any executable regions of memory that have
        /// been allocated.
        /// </summary>
        public void RemoveExecutableRegions()
        {
            // Resize the memory back to the original.
            // This will get rid of any executable
            // memory space that would usually
            // come after the main memory regions.
            Data = new byte[BaseMemorySize];

            // If we only have the original memory regions
            // then we can fast path return here.
            if (_memoryRegions.Count == 2)
            {
                return;
            }

            // Remove any executable memory regions.
            var tmp = _memoryRegions.ToArray();
            foreach (var item in tmp)
            {
                if (item.Access.HasFlag(MemoryAccess.EX))
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
        /// <param name="aData">
        /// The bytecode data to be loaded into the memory region.
        /// </param>
        /// <returns>
        /// A tuple of the start and end addresses of the executable
        /// region and the unique sequence ID for the memory region.
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
            const MemoryAccess flags = 
                MemoryAccess.R |
                MemoryAccess.W |
                MemoryAccess.EX;

            var seqId = 
                AddMemoryRegion(memLen,
                                newMemLen,
                                flags,
                                $"Executable");

            Array.Copy(aData, 
                       0, 
                       Data, 
                       memLen, 
                       aData.Length);

            ResizeRootMemoryRegion();

            return (memLen, newMemLen, seqId);
        }

        /// <summary>
        /// Add a memory region to the memory region permission list.
        /// </summary>
        /// <param name="aStart">
        /// The starting position of the memory region.
        /// </param>
        /// <param name="aEnd">
        /// The ending position of the memory region.
        /// </param>
        /// <param name="aAccess">
        /// The access flags to be applied to the region.
        /// </param>
        /// <param name="aName">
        /// The name of the memory region.
        /// </param>
        /// <returns>
        /// The sequence ID that uniquely represents the memory region.
        /// </returns>
        public int AddMemoryRegion(int aStart,
                                   int aEnd,
                                   MemoryAccess aAccess,
                                   string aName)
        {
            var region = 
                new MemoryRegion(aStart, aEnd, aAccess, _seqId, aName);
            _memoryRegions.Add(region);

            ResizeRootMemoryRegion();

            ++_seqId;

            return region.SeqID;
        }

        /// <summary>
        /// Get a memory region with a given sequence identifier.
        /// </summary>
        /// <param name="aSeqId">
        /// The sequence identifier to be located.
        /// </param>
        /// <returns>
        /// A MemoryRegion object for the given ID if one exists,
        /// a null otherwise.
        /// </returns>
        public MemoryRegion? GetMemoryRegion(int aSeqId)
        {
            var count = _memoryRegions.Count;
            for (var i = 0; i < count; i++)
            {
                if (_memoryRegions[i].SeqID == aSeqId)
                {
                    return _memoryRegions[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Get the complete list of memory regions.
        /// </summary>
        public MemoryRegion[] GetMemoryRegions()
        {
            return _memoryRegions.ToArray();
        }

        /// <summary>
        /// Remove a region or regions of memory that
        /// intersect with a given position.
        /// The root memory region (seqID == 0) will not be removed.
        /// </summary>
        /// <param name="aPoint">
        /// The position within memory to target.
        /// </param>
        /// <param name="aRemoveAll">
        /// A boolean, true if all matching memory regions
        /// should be removed, false otherwise.
        /// </param>
        public void RemoveMemoryRegion(int aPoint, bool aRemoveAll)
        {
            // We want to iterate this list in reverse as
            // we only want to remove the -last- entry only.

            var regionId = _memoryRegions.Count - 1;

            // We do not want to remove the stack
            // or root memory regions.
            while (regionId >= 2)
            {
                var region = _memoryRegions[regionId];
                if (aPoint >= region.Start && aPoint <= region.End)
                {
                    _memoryRegions.RemoveAt(regionId);

                    if (!aRemoveAll)
                    {
                        break;
                    }
                }

                --regionId;
            }

            ResizeRootMemoryRegion();
        }

        /// <summary>
        /// Remove a memory region with a given sequence identifier.
        /// The root memory region (seqID == 0) cannot be removed.
        /// </summary>
        /// <param name="aSeqId">
        /// The sequence identifier to be checked.
        /// </param>
        public void RemoveMemoryRegion(int aSeqId)
        {
            // We do not want to remove the stack
            // or root memory regions.
            if (aSeqId <= 1)
            {
                return;
            }

            _memoryRegions.RemoveAll(aX => aX.SeqID == aSeqId);

            ResizeRootMemoryRegion();
        }

        /// <summary>
        /// Directly get a range of bytes from memory.
        /// Do not use in anything other than internal
        /// code that does not need to take account of memory permissions!
        /// </summary>
        /// <param name="aStart">
        /// The start of the memory region range.
        /// </param>
        /// <param name="aEnd">
        /// The end of the memory region range.
        /// </param>
        /// <returns>
        /// An array of bytes representing the bytes extracted from memory.
        /// </returns>
        public byte[] DirectGetMemoryRaw(int aStart, int aEnd)
        {
            return Data[aStart..aEnd];
        }

        /// <summary>
        /// Gets the permissions for a specified memory region.
        /// </summary>
        /// <param name="aStart">
        /// The start of the memory region range.
        /// </param>
        /// <param name="aEnd">
        /// The end of the memory region range.
        /// </param>
        /// <returns>
        /// An array of the regions that intersect with the
        /// specified address range.
        /// </returns>
        public MemoryRegion[] GetMemoryPermissions(int aStart, int aEnd)
        {
            var regions = _memoryRegions.ToArray();
            var regionId = regions.Length - 1;

            // We want to iterate this list in reverse as the last entry
            // can override those entered before it.
            var matched = new List<MemoryRegion>();
            while (regionId >= 0)
            {
                var region = regions[regionId];

                if (aStart >= region.Start && aEnd <= region.End)
                {
                    // We have a match where the range is -completely-
                    // within a region. No cross-region permission issues
                    // can arise here.
                    matched.Add(region);
                    break;
                }
                
                if (aStart <= region.End && region.Start <= aEnd)
                {
                    // We have a cross-region match.
                    // We will have to do some additional checks
                    // to ensure that we assign the correct permissions.
                    matched.Add(region);
                }

                --regionId;
            }

            if (matched.Count > 0)
            {
                return matched.ToArray();
            }

            // This cannot happen with any valid address as
            // the root memory region will always match a valid address.
            throw new MemoryAccessViolationException
            (
                "GetMemoryPermissions: attempted to access a memory " +
                "region that does not exist. " +
                $"Start = {aStart}, End = {aEnd}."
            );
        }

        #region Stack Methods

        /// <summary>
        /// Push an integer value to the stack.
        /// </summary>
        /// <param name="aValue">
        /// The value to be pushed to the stack.
        /// </param>
        /// <exception cref="StackOutOfRangeException">
        /// Thrown if there is insufficient space within the stack for
        /// the integer value to be pushed.
        /// </exception>
        public void StackPushInt(int aValue)
        {
            var maxPos = StackPointer;
            var minPos = StackPointer - sizeof(int);

            // We do not have enough room to write this
            // value to the stack.
            if (minPos < StackStart)
            {
                throw new StackOutOfRangeException
                (
                    "StackPushInt: there is insufficient space on " +
                    "the stack for the value to be pushed."
                );
            }

            // Write the value to stack memory region.
            SetInt(minPos, aValue, SecurityContext.System, false);

            if (IsDebuggingEnabled)
            {
                StackTypes.Push(typeof(int));
            }

            // Move the stack pointer to the new location.
            // We move this forwards by the size of an integer.
            // As the stack operates in reverse, we move it closer
            // to the start of the stack memory region.
            StackPointer -= sizeof(int);
        }

        /// <summary>
        /// Pop an integer from the stack.
        /// </summary>
        /// <returns>
        /// The last integer popped from the stack.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if there is no value to be popped from the stack.
        /// </exception>
        public int StackPopInt()
        {
            var maxPos = StackPointer;
            var minPos = StackPointer - sizeof(int);

            // We do not have enough room to read this
            // value from the stack.
            if (maxPos >= StackEnd)
            {
                throw new InvalidOperationException
                (
                    "StackPopInt: there is insufficient space on " +
                    "the stack for the value to be popped."
                );
            }

            // Read the value from the memory region.
            var value = 
                GetInt(maxPos, SecurityContext.System, false);

            if (IsDebuggingEnabled)
            {
                _ = StackTypes.Pop();
            }

            // Move the stack pointer to the new location.
            // We move this forwards by the size of an integer.
            // As the stack operates in reverse, we move it away
            // from the start of the stack memory region.
            StackPointer += sizeof(int);

            return value;
        }

        /// <summary>
        /// Print the contents of the stack.
        /// </summary>
        public void PrintStack()
        {
            // We can do something a bit fancier if
            // we have access to the type information.
            if (IsDebuggingEnabled)
            {
                PrintStackDebug();
                return;
            }

            // If the stack pointer is currently at
            // the end of the stack region then there
            // are no values to be read.
            if (StackPointer == StackEnd)
            {
                return;
            }

            var index = 0;
            var curStackPos = StackPointer;
            while (curStackPos < StackEnd)
            {
                // Things are always reverse here.
                // We need to jump to the "start" of the
                // value before we read it.
                var value =
                    GetInt(curStackPos,
                           SecurityContext.System,
                           false);

                Console.WriteLine("{0,5}{1,10:X8}",
                                  index,
                                  value);

                curStackPos += sizeof(int);
                ++index;
            }
        }

        /// <summary>
        /// Print the contents of the stack, with additional
        /// debugging information.
        /// </summary>
        public void PrintStackDebug()
        {
            if (!IsDebuggingEnabled)
            {
                return;
            }

            // If the stack pointer is currently at
            // the end of the stack region then there
            // are no values to be read.
            if (StackPointer == StackEnd)
            {
                return;
            }

            // Clone stack that specifies which types
            // were stored.
            var types = StackTypes.ToArray();

            var i = 0;
            var curStackPos = StackPointer;
            while (curStackPos < StackEnd)
            {
                var t = types[i];

                // Things are always reverse here.
                // We need to jump to the "start" of the
                // value before we read it.
                object value;
                int offset;
                switch (t)
                {
                    case { } _ when t == typeof(int):
                        value = GetInt(curStackPos,
                                       SecurityContext.System,
                                       false);
                        offset = sizeof(int);
                        break;

                    default:
                        throw new NotSupportedException
                        (
                            $"PrintStackDebug: the type {t} was passed " +
                            "specified as the stack type, but no " +
                            "support has been provided for that type."
                        );
                }

                Console.WriteLine("{0,5}{1,10}{2,10:X8}",
                                  i,
                                  t.GetFriendlyName(),
                                  value);

                curStackPos += offset;
                ++i;
            }
        }

        #endregion // Stack Methods

        #region Integer IO

        /// <summary>
        /// Read an integer from memory.
        /// </summary>
        /// <param name="aStartPos">
        /// The location of the first byte to retrieve.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <param name="aExec">
        /// A boolean indicating if this value must be within
        /// an executable memory region.
        /// </param>
        /// <returns>A single byte from memory.</returns>
        /// <exception cref="MemoryAccessViolationException">
        /// Thrown if the specified permission flag is not
        /// set for the memory region.
        /// </exception>
        /// <exception cref="MemoryOutOfRangeException">
        /// Thrown if the specified position falls outside
        /// of valid memory bounds.
        /// </exception>
        public int GetInt(int aStartPos,
                          SecurityContext aContext,
                          bool aExec)
        {
            var bytes =
                GetValueRange(aStartPos, 
                              sizeof(int), 
                              aContext, 
                              aExec);

            return BitConverter.ToInt32(bytes);
        }

        /// <summary>
        /// Writes an integer to memory.
        /// </summary>
        /// <param name="aStartPos">
        /// The location of the first byte to retrieve.
        /// </param>
        /// <param name="aValue">
        /// The integer value to be written to memory.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <param name="aExec">
        /// A boolean indicating if this value must be within
        /// an executable memory region.
        /// </param>
        /// <returns>A single byte from memory.</returns>
        /// <exception cref="MemoryAccessViolationException">
        /// Thrown if the specified permission flag is not
        /// set for the memory region.
        /// </exception>
        /// <exception cref="MemoryOutOfRangeException">
        /// Thrown if the specified position falls outside
        /// of valid memory bounds.
        /// </exception>
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
        /// <param name="aStartPos">
        /// The location of the first byte to retrieve.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <param name="aExec">
        /// A boolean indicating if this value must be within
        /// an executable memory region.
        /// </param>
        /// <returns>A single byte from memory.</returns>
        /// <exception cref="MemoryAccessViolationException">
        /// Thrown if the specified permission flag is not
        /// set for the memory region.
        /// </exception>
        /// <exception cref="MemoryOutOfRangeException">
        /// Thrown if the specified position falls outside
        /// of valid memory bounds.
        /// </exception>
        public OpCode GetOpCode(int aStartPos,
                                SecurityContext aContext,
                                bool aExec)
        {
            return (OpCode)GetInt(aStartPos, aContext, aExec);
        }

        /// <summary>
        /// Writes an opcode to memory.
        /// </summary>
        /// <param name="aStartPos">
        /// The location of the first byte to retrieve.
        /// </param>
        /// <param name="aValue">
        /// The integer value to be written to memory.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <param name="aExec">
        /// A boolean indicating if this value must be within
        /// an executable memory region.
        /// </param>
        /// <returns>A single byte from memory.</returns>
        /// <exception cref="MemoryAccessViolationException">
        /// Thrown if the specified permission flag is not
        /// set for the memory region.
        /// </exception>
        /// <exception cref="MemoryOutOfRangeException">
        /// Thrown if the specified position falls outside
        /// of valid memory bounds.
        /// </exception>
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
        /// <param name="aStartPos">
        /// The location of the first byte to retrieve.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <param name="aExec">
        /// A boolean indicating if this value must be within
        /// an executable memory region.
        /// </param>
        /// <returns>A single byte from memory.</returns>
        /// <exception cref="MemoryAccessViolationException">
        /// Thrown if the specified permission flag is not
        /// set for the memory region.
        /// </exception>
        /// <exception cref="MemoryOutOfRangeException">
        /// Thrown if the specified position falls outside
        /// of valid memory bounds.
        /// </exception>
        public Registers GetRegisterIdent(int aStartPos,
                                          SecurityContext aContext,
                                          bool aExec)
        {
            return (Registers)GetValue(aStartPos, aContext, aExec);
        }

        /// <summary>
        /// Writes a register identifier to memory.
        /// </summary>
        /// <param name="aStartPos">
        /// The location of the first byte to retrieve.
        /// </param>
        /// <param name="aValue">
        /// The integer value to be written to memory.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <param name="aExec">
        /// A boolean indicating if this value must be within
        /// an executable memory region.
        /// </param>
        /// <returns>A single byte from memory.</returns>
        /// <exception cref="MemoryAccessViolationException">
        /// Thrown if the specified permission flag is not
        /// set for the memory region.
        /// </exception>
        /// <exception cref="MemoryOutOfRangeException">
        /// Thrown if the specified position falls outside
        /// of valid memory bounds.
        /// </exception>
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
        /// <param name="aStartPos">
        /// The location of the first byte to retrieve.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <param name="aExec">
        /// A boolean indicating if this value must be within
        /// an executable memory region.
        /// </param>
        /// <returns>
        /// A tuple giving. The first value indicating how many bytes
        /// in total were read in order to construct the string.
        /// The second value being the string that was read from memory.
        /// </returns>
        /// <exception cref="MemoryAccessViolationException">
        /// Thrown if the specified permission flag is not
        /// set for the memory region.
        /// </exception>
        /// <exception cref="MemoryOutOfRangeException">
        /// Thrown if the specified position falls outside
        /// of valid memory bounds.
        /// </exception>
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
        /// <param name="aStartPos">
        /// The location of the first byte to retrieve.
        /// </param>
        /// <param name="aValue">
        /// The integer value to be written to memory.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <param name="aExec">
        /// A boolean indicating if this value must be within
        /// an executable memory region.
        /// </param>
        /// <exception cref="MemoryAccessViolationException">
        /// Thrown if the specified permission flag is not
        /// set for the memory region.
        /// </exception>
        /// <exception cref="MemoryOutOfRangeException">
        /// Thrown if the specified position falls outside
        /// of valid memory bounds.
        /// </exception>
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
        /// <param name="aPos">
        /// The location of the byte to retrieve.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <param name="aExec">
        /// A boolean indicating if this value must be within
        /// an executable memory region.
        /// </param>
        /// <returns>A single byte from memory.</returns>
        /// <exception cref="MemoryAccessViolationException">
        /// Thrown if the specified permission flag is not
        /// set for the memory region.
        /// </exception>
        /// <exception cref="MemoryOutOfRangeException">
        /// Thrown if the specified position falls outside
        /// of valid memory bounds.
        /// </exception>
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
        /// <param name="aPos">
        /// The location of the first byte to retrieve.
        /// </param>
        /// <param name="aLength">
        /// The number of bytes to retrieve.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <param name="aExec">
        /// A boolean indicating if this value must be within
        /// an executable memory region.
        /// </param>
        /// <returns>An array of bytes from memory.</returns>
        /// <exception cref="MemoryAccessViolationException">
        /// Thrown if the specified permission flag is not
        /// set for the memory region.
        /// </exception>
        /// <exception cref="MemoryOutOfRangeException">
        /// Thrown if the specified position falls outside
        /// of valid memory bounds.
        /// </exception>
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

            return 
                new Span<byte>(Data).Slice(aPos, aLength).ToArray();
        }

        /// <summary>
        /// Sets a single byte in memory.
        /// </summary>
        /// <param name="aPos">
        /// The location of the byte to set.
        /// </param>
        /// <param name="aValue">
        /// The value of the byte to be set.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <param name="aExec">
        /// A boolean indicating if this value must be within
        /// an executable memory region.
        /// </param>
        /// <returns>An array of bytes from memory.</returns>
        /// <exception cref="MemoryAccessViolationException">
        /// Thrown if the specified permission flag is not
        /// set for the memory region.
        /// </exception>
        /// <exception cref="MemoryOutOfRangeException">
        /// Thrown if the specified position falls outside
        /// of valid memory bounds.
        /// </exception>
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
        /// <param name="aPos">
        /// The location of the first byte to be written.
        /// </param>
        /// <param name="aBytes">
        /// The value of the byte to be set.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <param name="aExec">
        /// A boolean indicating if this value must be within
        /// an executable memory region.
        /// </param>
        /// <returns>An array of bytes from memory.</returns>
        /// <exception cref="MemoryAccessViolationException">
        /// Thrown if the specified permission flag is not
        /// set for the memory region.
        /// </exception>
        /// <exception cref="MemoryOutOfRangeException">
        /// Thrown if the specified position falls outside
        /// of valid memory bounds.
        /// </exception>
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
        /// Get a formatted list of the memory regions.
        /// </summary>
        public IEnumerable<string> GetFormattedMemoryRegions()
        {
            var regionCount = _memoryRegions.Count;
            var lines = new string[regionCount + 2];

            for (var i = 0; i < regionCount; i++)
            {
                var r = _memoryRegions[i];

                lines[i + 1] =
                    $"|{$"{r.Start},{r.End}",20} | {r.Access,15} | " +
                    $"{r.SeqID,10} | {r.Name,15}|";
            }

            var lineLen = lines[1].Length;

            lines[0] = new string('-', lineLen);
            lines[^1] = new string('-', lineLen);

            return lines;
        }

        /// <summary>
        /// Checks if a given range of memory has a flag set.
        /// </summary>
        /// <param name="aStart">
        /// The starting location of the memory region.
        /// </param>
        /// <param name="aEnd">
        /// The ending location of the memory region.
        /// </param>
        /// <param name="aContext">
        /// The security context for this request.
        /// </param>
        /// <param name="aType">
        /// The data access type to check.
        /// </param>
        /// <param name="aExec">
        /// A boolean indicating if this value must be within
        /// an executable memory region.
        /// </param>
        /// <returns>An array of bytes from memory.</returns>
        /// <exception cref="MemoryAccessViolationException">
        /// Thrown if the specified permission flag is not
        /// set for the memory region.
        /// </exception>
        /// <exception cref="MemoryOutOfRangeException">
        /// Thrown if the specified position falls outside
        /// of valid memory bounds.
        /// </exception>
        private void ValidateAccess(int aStart,
                                    int aEnd,
                                    DataAccessType aType,
                                    SecurityContext aContext,
                                    bool aExec)
        {
            if (aStart < 0 || aStart > Data.Length ||
                aEnd < 0 || aEnd > Data.Length)
            {
                throw new MemoryOutOfRangeException
                (
                    "ValidateAccess: the specified memory location is " +
                    "outside of the memory bounds."
                );
            }

            // If we have an address range that intersects one or more
            // memory regions then we need to choose the access flags
            // from the region that has the highest permissions of those
            // that were returned.
            // The logic being that the highest permissions will need
            // to be met for access to be granted to any point within
            // the range.

            var hasFlags = true;
            var flags = MemoryAccess.N;
            foreach (var r in GetMemoryPermissions(aStart, aEnd))
            {
                // If we have requested an executable memory
                // region and this region is not executable
                // then we cannot have a match.
                if (aExec && !r.Access.HasFlag(MemoryAccess.EX))
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

            hasFlags &= aType switch
            {
                DataAccessType.Read => 
                    flags.HasFlag(MemoryAccess.R) ||
                    (flags.HasFlag(MemoryAccess.PR) && 
                     aContext == SecurityContext.System),

                DataAccessType.Write => 
                    flags.HasFlag(MemoryAccess.W) ||
                    (flags.HasFlag(MemoryAccess.PW) && 
                     aContext == SecurityContext.System),

                _ => 
                    throw new NotSupportedException
                    (
                        "ValidateAccess: attempted to check a " +
                               "non-valid data access type."
                    )
            };

            if (!hasFlags)
            {
                throw new MemoryAccessViolationException
                (
                    $"ValidateAccess: attempted to access a memory" +
                    $"without the correct security context or access " +
                    $"flags. Access Type = {aType}, Executable = " +
                    $"{aExec}, flags = {flags}."
                );
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
            foreach (var l in GetFormattedMemoryRegions())
            {
                Debug.WriteLine(l);
            }
        }
    }
}
