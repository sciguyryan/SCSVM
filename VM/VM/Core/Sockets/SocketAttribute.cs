using System;
using VMCore.VM.IO.DeviceSockets;

namespace VMCore.VM.Core.Sockets
{
    public enum SocketAccess 
    {
        Read  = 0x0,
        Write = 0x1
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class SocketAttribute
        : Attribute
    {
        public SocketAccess Access { get; set; }

        public SocketAddresses SocketAddress { get; set; }

        public SocketAttribute(SocketAddresses aDeviceSocketAddr,
                               SocketAccess aAccess)
        {
            SocketAddress = aDeviceSocketAddr;
            Access = aAccess;
        }
    }
}
