#region  description
/*
Main interface of remoute system dispatcher
 */
#endregion

#region  includes
using System; 
using ZeroGenerationInterfaces;
#endregion

namespace Core
{
    namespace RSDispatcher
    {
        /// <summary>
        /// RSD in it's basic view.
        /// </summary>
        interface IRemouteSystemDispatcher: Iloggable, IControlable, IEventBaseUser
        {
            ICollection<IDevices> Devices {get; set;}
            ICollection<IRemouteInterfaces> Interfaces {get; set;} 
            
            void sendControlPoint(IDevice target, IControlPoint controlPoint ); 
                
        }


    }
}