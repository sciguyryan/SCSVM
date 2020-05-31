using System;
using System.Collections.Generic;
using VMCore.VM.Core.Register;
using VMCore.VM.IO.DeviceSockets;

namespace VMCore.VM.Core.Sockets
{
    public static class SocketDeviceManager
    {
        /// <summary>
        /// A dictionary of the identified readable socket devices.
        /// </summary>
        public static Dictionary<SocketAddresses, ISocketDevice> ReadSockets { get; set; } = 
            new Dictionary<SocketAddresses, ISocketDevice>();

        /// <summary>
        /// A dictionary of the identified writable socket devices.
        /// </summary>
        public static Dictionary<SocketAddresses, ISocketDevice> WriteSockets { get; set; } = 
            new Dictionary<SocketAddresses, ISocketDevice>();

        /// <summary>
        /// Handle reading the value from a socket device of a
        /// specified type into a register.
        /// </summary>
        /// <param name="aAddr">The socket device address.</param>
        /// <param name="aReg">
        /// The register into which the value read from the device
        /// should be placed.
        /// </param>
        /// <param name="aVm">
        /// The virtual machine instance in which the interrupt
        /// should be handled.
        /// </param>
        /// <param name="aContext">
        /// The security context to be used when writing this
        /// value into the register.
        /// </param>
        public static void Read(SocketAddresses aAddr,
                                Registers aReg,
                                VirtualMachine aVm,
                                SecurityContext aContext)
        {
            if (!ReadSockets.TryGetValue(aAddr, out var device))
            {
                throw new Exception($"Read: unmapped socket address 0x{aAddr:X}.");
            }

            device.HandleRead(aAddr, aReg, aVm, aContext);
        }

        /// <summary>
        /// Handle writing a value to a socket device of a specified type.
        /// </summary>
        /// <param name="aAddr">The socket device address.</param>
        /// <param name="aValue">
        /// The value to be written to the socket device.
        /// </param>
        /// <param name="aVm">
        /// The virtual machine instance in which the interrupt
        /// should be handled.
        /// </param>
        public static void Write(SocketAddresses aAddr,
                                 int aValue,
                                 VirtualMachine aVm)
        {
            if (!WriteSockets.TryGetValue(aAddr, out var device))
            {
                throw new Exception($"Write: unmapped socket address 0x{aAddr:X}.");
            }

            device.HandleWrite(aAddr, aValue, aVm);
        }
    }
}
