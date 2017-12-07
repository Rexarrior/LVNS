using System;
using System.Collections.Generic;

namespace Core
{
    interface ICommand
    {
        UInt32 DestinationID { get; set; }
        ICollection<object> Command { get; set; }
    }
}
