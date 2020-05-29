using System;
using VMCore.VM.Core;
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
            var result = aAddr switch
            {
                SocketAddresses.ConsoleData => Console.Read(),
                _ => 0
            };

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

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
