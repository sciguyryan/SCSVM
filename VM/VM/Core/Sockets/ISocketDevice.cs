using VMCore.VM.IO.DeviceSockets;

namespace VMCore.VM.Core.Sockets
{
    public interface ISocketDevice
    {
        /// <summary>
        /// A list of control codes that are valid for use with this socket device.
        /// </summary>
        enum ControlCodes : int
        {
            None
        };

        /// <summary>
        /// Handle reading the value from a socket device of a specified type into a register.
        /// </summary>
        /// <param name="addr">The socket device address.</param>
        /// <param name="reg">The register into which the value read from the device should be placed.</param>
        /// <param name="vm">The virtual machine in which the interrupt should be handled.</param>
        /// <param name="context">The security context to be used when writing this value into the register.</param>
        void HandleRead(DeviceSocketAddresses addr, Registers reg, VirtualMachine vm, SecurityContext context);

        /// <summary>
        /// Handle writing a value to a socket device of a specified type.
        /// </summary>
        /// <param name="addr">The socket device address.</param>
        /// <param name="value">The value to be written to the socket device.</param>
        /// <param name="vm">The virtual machine in which the interrupt should be handled.</param>
        void HandleWrite(DeviceSocketAddresses addr, int value, VirtualMachine vm);
    }
}