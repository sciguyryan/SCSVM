using System;
using VMCore.VM.Core;
using VMCore.VM.Core.Register;
using VMCore.VM.Core.Sockets;

namespace VMCore.VM.IO.DeviceSockets
{
    [Socket(SocketAddresses.ConsoleControl, SocketAccess.Write)]
    [Socket(SocketAddresses.ConsoleData, SocketAccess.Read)]
    public class ConsoleDevice
        : ISocketDevice
    {
        public enum ControlCodes : int
        {
            ClearConsole        = 0x0,
            WriteChar           = 0x1,
            SetForegroundColor  = 0x2,
            SetBackgroundColor  = 0x3,
            ResetColors         = 0x4,
            Beep                = 0x5,
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
                case ControlCodes.ClearConsole:
                    Console.Clear();
                    break;

                case ControlCodes.WriteChar:
                    Console.Write('a'); // TODO - unfinished
                    break;

                case ControlCodes.SetForegroundColor:
                    Console.ForegroundColor = 0; // TODO - unfinished
                    break;

                case ControlCodes.SetBackgroundColor:
                    Console.BackgroundColor = 0; // TODO - unfinished
                    break;

                case ControlCodes.ResetColors:
                    Console.ResetColor();
                    break;

                case ControlCodes.Beep:
                    Console.Beep(0, 0); // TODO - unfinished
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
