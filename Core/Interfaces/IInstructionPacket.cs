using System;

namespace Core
{
    interface IInstructionPacket
    {
        UInt32 DeviceID { get; }
        UInt32 PacketSize { get; }
        Byte[] Data { get; }
    }
}
