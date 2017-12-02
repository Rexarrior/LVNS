#region  description
    /*
     */
#endregion 


#region includes

#endregion

namespace Core
{
    namespace Kernel
    {
        interface IKernel: IEventBaseUser, ILoggable, IControlable, IController
        {
            IPositionSystemDispatcher psd{set; get;}
            IRemouteSystemDispatcher  rsd{set; get;}
            IClientManager            cm{set; get;}
            ICommandSystemDispatcer   csd{set; get;}

            ICollection<>
        }
    }
}