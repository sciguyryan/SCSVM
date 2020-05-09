using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VMCore.VM.IO.DeviceSockets;

namespace VMCore.VM.Core.Sockets
{
    public static class SocketDeviceManager
    {
        /// <summary>
        /// A dictionary of the identified readable socket devices.
        /// </summary>
        public static Dictionary<DeviceSocketAddresses, ISocketDevice> ReadSockets { get; set; } = 
            new Dictionary<DeviceSocketAddresses, ISocketDevice>();

        /// <summary>
        /// A dictionary of the identified writable socket devices.
        /// </summary>
        public static Dictionary<DeviceSocketAddresses, ISocketDevice> WriteSockets { get; set; } = 
            new Dictionary<DeviceSocketAddresses, ISocketDevice>();

        /// <summary>
        /// Handle reading the value from a socket device of a specified type into a register.
        /// </summary>
        /// <param name="addr">The socket device address.</param>
        /// <param name="reg">The register into which the value read from the device should be placed.</param>
        /// <param name="vm">The virtual machine instance in which the interrupt should be handled.</param>
        /// <param name="context">The security context to be used when writing this value into the register.</param>
        public static void Read(DeviceSocketAddresses addr, Registers reg, VirtualMachine vm, SecurityContext context)
        {
            if (ReadSockets.TryGetValue(addr, out ISocketDevice device))
            {
                device.HandleRead(addr, reg, vm, context);
                return;
            }

            throw new Exception($"Read: unmapped socket address 0x{addr:X}.");
        }

        /// <summary>
        /// Handle writing a value to a socket device of a specified type.
        /// </summary>
        /// <param name="addr">The socket device address.</param>
        /// <param name="value">The value to be written to the socket device.</param>
        /// <param name="vm">The virtual machine instance in which the interrupt should be handled.</param>
        public static void Write(DeviceSocketAddresses addr, int value, VirtualMachine vm)
        {
            if (WriteSockets.TryGetValue(addr, out ISocketDevice device))
            {
                device.HandleWrite(addr, value, vm);
                return;
            }

            throw new Exception($"Write: unmapped socket address 0x{addr:X}.");
        }
    }
}