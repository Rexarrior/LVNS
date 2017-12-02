#region  description
/*
Contain a interface for imagine a remoute device
 */
#endregion

#region  includes
using System; 
#endregion

namespace Core
{
    namespace RSDispatcher
    {
        /// <summary>
        /// Any device which can be controlled by remoute interface. 
        /// </summary>
        interface IDevice
        {
            
            int ID; 
            ICollection<string> acceptingConrtolPoints;
            ICollection<string> supportedInterfaces ;
            I
        }
    }
}