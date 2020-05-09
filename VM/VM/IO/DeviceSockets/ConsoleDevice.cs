using System;
using VMCore.VM.Core.Sockets;

namespace VMCore.VM.IO.DeviceSockets
{
    [Socket(DeviceSocketAddresses.ConsoleControl, SocketAccess.Write)]
    [Socket(DeviceSocketAddresses.ConsoleData, SocketAccess.Read)]
    public class ConsoleDevice : ISocketDevice
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
                case ControlCodes.ClearConsole:
                    Console.Clear();
                    break;

                case ControlCodes.WriteChar:
                    Console.Write('a'); // TODO - use the stack
                    break;

                case ControlCodes.SetForegroundColor:
                    Console.ForegroundColor = (ConsoleColor)0; // TODO - use the stack
                    break;

                case ControlCodes.SetBackgroundColor:
                    Console.BackgroundColor = (ConsoleColor)0; // TODO - use the stack
                    break;

                case ControlCodes.ResetColors:
                    Console.ResetColor();
                    break;

                case ControlCodes.Beep:
                    Console.Beep(0, 0); // TODO - use the stack
                    break;
            }
        }
    }
}