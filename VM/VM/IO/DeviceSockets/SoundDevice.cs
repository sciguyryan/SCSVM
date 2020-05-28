using System;
using VMCore.VM.Core.Sockets;

namespace VMCore.VM.IO.DeviceSockets
{
    [Socket(SocketAddresses.SoundControl, SocketAccess.Write)]
    [Socket(SocketAddresses.SoundData, SocketAccess.Read)]
    public class SoundDevice
        : ISocketDevice
    {
        public enum ControlCodes : int
        {
            None = 0x0,
        }

        public void HandleRead(SocketAddresses aAddr,
                               Registers aReg,
                               VirtualMachine aVm,
                               SecurityContext aContext)
        {
            int result = 0;
            switch (aAddr)
            {
                case SocketAddresses.ConsoleData:
                    result = Console.Read();
                    break;
            }

            aVm.Cpu.Registers[(aReg, aContext)] = result;
        }

        public void HandleWrite(SocketAddresses aAddr,
                                int aControl,
                                VirtualMachine aVm)
        {
            switch ((ControlCodes)aControl)
            {
                case ControlCodes.None:
                    break;
            }
        }
    }
}
