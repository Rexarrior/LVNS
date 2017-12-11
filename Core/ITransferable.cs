using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    enum TransferableDataType
    {
        InstructionPacket,
        Command,
        IncomingData,
        Message
    }
    interface ITransferable
    {
        UInt32 DestinationID { get; set; }
        ICollection<object> Data { get; set; }
        TransferableDataType DataType { get; }
    }
}
