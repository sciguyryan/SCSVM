using System;
using System.Collections.Generic;
using VMCore.VM.Core.Exceptions;
using VMCore.VM.Core.Reg;

namespace VMCore.VM.Core.Mem
{
    public class Memory
    {
        /// <summary>
        /// The byte array representing the system memory.
        /// </summary>
        private byte[] Data { get; }

        /// <summary>
        /// A list of memory regions and their associated permissions.
        /// </summary>
        private List<MemoryRegion> _memoryRegions = 
            new List<MemoryRegion>();

        public Memory(int capacity = 2048)
        {
            // TODO - this needs to be in a try-catch as
            // it can fail.
            Data = new byte[capacity];

            // By default the read and write permissions are set
            // for the entire memory block.
            AddMemoryRegion(0, capacity-1, MemoryAccess.R | MemoryAccess.W);
        }

        /// <summary>
        /// Load a pre-populated memory block into the system memory.
        /// </summary>
        /// <param name="payload">The byte array used to represent the system memory.</param>
        public Memory(byte[] payload)
        {
            Data = payload;
        }

        /// <summary>
        /// Clear the current system memory.
        /// </summary>
        public void Clear()
        {
            new Span<byte>(Data).Fill(0);
        }

        /// <summary>
        /// Add a memory region to the memory region permission list.
        /// </summary>
        /// <param name="start">The starting position of the memory region.</param>
        /// <param name="end">The ending position of the memory region. </param>
        /// <param name="access">The access flags to be applied to the region.</param>
        public void AddMemoryRegion(int start, int end, MemoryAccess access)
        {
            _memoryRegions.Add(new MemoryRegion(start, end, access));
        }

        /// <summary>
        /// Remove a region or regions of memory that contain a given position.
        /// </summary>
        /// <param name="point">The position within memory to target.</param>
        /// <param name="removeAll">A boolean, true if all matching memory regions should be removed, false otherwise.</param>
        public void RemoveMemoryRegion(int point, bool removeAll = false)
        {
            // We want to iterate this list in reverse as
            // we only want to remove the -last- entry only.

            MemoryRegion region;
            var regionID = _memoryRegions.Count - 1;
            while (regionID >= 0)
            {
                region = _memoryRegions[regionID];
                if (point >= region.Start && point <= region.End)
                {
                    _memoryRegions.RemoveAt(regionID);

                    if (!removeAll)
                    {
                        break;
                    }
                }

                --regionID;
            }
        }

        /// <summary>
        /// Gets the permissions for a specified memory region.
        /// </summary>
        /// <param name="start">The start of the memory region range.</param>
        /// <returns>The permissions for the memory region.</returns>
        public MemoryAccess GetMemoryPermissions(int start, int end)
        {
            var regions = _memoryRegions.ToArray();
            var regionID = regions.Length - 1;

            var access = MemoryAccess.R | MemoryAccess.W;

            // We want to iterate this list in reverse as the last entry
            // can override those entered before it.
            while (regionID >= 0)
            {
                var region = regions[regionID];
                if (start <= region.End && region.Start <= end)
                {
                    access = region.Access;
                    break;
                }

                --regionID;
            }

            return access;
        }

        /// <summary>
        /// Read a value of a given type from a starting memory location.
        /// </summary>
        /// <typeparam name="T">The type of the data to be read.</typeparam>
        /// <param name="startPos">The position from which the data should be read.</param>
        /// <param name="context">The security context for this request.</param>
        /// <returns>A single byte from memory.</returns>
        /// <exception>MemoryAccessViolationException if the specified permission flag is not set for the memory region.</exception>
        public T GetValueAsType<T>(int startPos, SecurityContext context)
        {
            var t = typeof(T);
            var length = t switch
            {
                Type _ when t == typeof(byte)   => sizeof(byte),
                Type _ when t == typeof(int)    => sizeof(int),
                _                               => throw new NotSupportedException($"GetValueAsType: the type {t} was passed as an argument type, but no support has been provided for that type."),
            };

            var bytes = GetValueRange(startPos, length, context);

            var value = t switch
            {
                Type _ when t == typeof(byte)   => (byte)bytes[0],
                Type _ when t == typeof(int)    => BitConverter.ToInt32(bytes),
                _                               => throw new NotSupportedException($"GetValueAsType: the type {t} was passed as an argument type, but no support has been provided for that type."),
            };

            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Reads a single byte from memory.
        /// </summary>
        /// <param name="pos">The location of the byte to retrieve.</param>
        /// <param name="context">The security context for this request.</param>
        /// <returns>A single byte from memory.</returns>
        /// <exception>MemoryAccessViolationException if the specified permission flag is not set for the memory region.</exception>
        public byte GetValue(int pos, SecurityContext context)
        {
            if (pos < 0 || pos > Data.Length)
            {
                throw new MemoryOutOfRangeException($"GetValue: the specified memory location is negative and is therefore invalid.");
            }

            ValidateAccess(pos, pos, DataAccessType.Read, context);

            return Data[pos];
        }

        /// <summary>
        /// Reads a range of bytes from memory.
        /// </summary>
        /// <param name="pos">The location of the first byte to retrieve.</param>
        /// <param name="length">The number of bytes to retrieve.</param>
        /// <param name="context">The security context for this request.</param>
        /// <returns>An array of bytes from memory.</returns>
        /// <exception>MemoryAccessViolationException if the specified permission flag is not set for the memory region.</exception>
        public byte[] GetValueRange(int pos, int length, SecurityContext context)
        {
            if (pos < 0 || pos > Data.Length)
            {
                throw new MemoryOutOfRangeException($"GetValueRange: the specified memory location is negative and is therefore invalid.");
            }

            ValidateAccess(pos, pos + length, DataAccessType.Read, context);

            return new Span<byte>(Data).Slice(pos, length).ToArray();
        }

        /// <summary>
        /// Sets a single byte in memory.
        /// </summary>
        /// <param name="pos">The location of the byte to set.</param>
        /// <param name="value">The value of the byte to be set.</param>
        /// <param name="context">The security context for this request.</param>
        /// <exception>MemoryAccessViolationException is the specified permission flag is not set for the memory region.</exception>
        public void SetValue(int pos, byte value, SecurityContext context)
        {
            if (pos < 0 || pos > Data.Length)
            {
                throw new MemoryOutOfRangeException($"SetValue: the specified memory location is negative and is therefore invalid.");
            }

            ValidateAccess(pos, pos, DataAccessType.Write, context);

            Data[pos] = value;
        }

        /// <summary>
        /// Sets a range of bytes in memory.
        /// </summary>
        /// <param name="pos">The location of the first byte to be written.</param>
        /// <param name="bytes">The value of the byte to be set.</param>
        /// <param name="context">The security context for this request.</param>
        /// <exception>MemoryAccessViolationException is the specified permission flag is not set for the memory region.</exception>
        public void SetValueRange(int pos, byte[] bytes, SecurityContext context)
        {
            if (pos < 0 || pos > Data.Length)
            {
                throw new MemoryOutOfRangeException($"SetValueRange: the specified memory location is negative and is therefore invalid.");
            }

            ValidateAccess(pos, pos + bytes.Length, DataAccessType.Write, context);

            // Sigh... why couldn't array ranges for writing too :(
            for (var i = 0; i < bytes.Length; i++)
            {
                Data[pos + i] = bytes[i];
            }
        }

        /// <summary>
        /// Checks if a given range of memory has a flag set.
        /// Using a system-level security context will always grant access.
        /// </summary>
        /// <param name="start">The starting location of the memory region.</param>
        /// <param name="end">The ending location of the memory region.</param>
        /// <param name="type">The data access type to check.</param>
        /// <param name="context">The security context for this request.</param>
        /// <exception>MemoryAccessViolationException if the specified permission flag is not set for the memory region.</exception>
        private void ValidateAccess(int start, int end, DataAccessType type, SecurityContext context)
        {
            // A system-level security context is granted
            // automatic access.
            if (context == SecurityContext.System)
            {
                return;
            }

            var flags = GetMemoryPermissions(start, end);

            // We do not need to check for private read
            // and write here as they will either use the
            // system context and fall through above or
            // they will fall through these checks and
            // trigger the MemoryAccessViolationException below.
            bool hasFlags;
            if (type.HasFlag(DataAccessType.Read))
            {
                hasFlags = flags.HasFlag(MemoryAccess.R);

            }
            else if (type.HasFlag(DataAccessType.Write))
            {
                hasFlags = flags.HasFlag(RegisterAccess.W);
            }
            else
            {
                throw new NotSupportedException($"ValidateAccess: attempted to check a non-valid data access type.");
            }

            if (!hasFlags)
            {
                throw new MemoryAccessViolationException($"ValidateAccess: attempted to access a register without the correct security context or access flags. Access Type = {type}, flags = {flags}");
            }
        }
    }
}