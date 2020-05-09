using System;
using VMCore.VM.Core.Sockets;

namespace VMCore.VM.IO.DeviceSockets
{
    [Socket(DeviceSocketAddresses.SoundControl, SocketAccess.Write)]
    [Socket(DeviceSocketAddresses.SoundData, SocketAccess.Read)]
    public class SoundDevice : ISocketDevice
    {
        public enum ControlCodes : int
        {
            None = 0x0,
        }

        public void HandleRead(DeviceSocketAddresses addr, Registers reg, VirtualMachine vm, SecurityContext context)
        {
            int result = 0;
            switch (addr)
            {
                case DeviceSocketAddresses.ConsoleData:
                    result = Console.Read();
                    break;
            }

            vm.CPU.Registers[(reg, context)] = result;
        }

        public void HandleWrite(DeviceSocketAddresses addr, int control, VirtualMachine vm)
        {
            switch ((ControlCodes)control)
            {
                case ControlCodes.None:
                    break;
            }
        }
    }
}