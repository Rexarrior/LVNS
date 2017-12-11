using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    delegate void SendToKernelDelegate(ITransferable data);
    interface IKernelCommunicator
    {
        SendToKernelDelegate SendToKernel { get; }
    }
}
