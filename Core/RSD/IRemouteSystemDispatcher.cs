#region  description
/*

 */
#endregion

#region  includes
using System; 
#endregion

namespace RSDispatcher
{

  interface IRemouteSystemDispatcher: Ilaggabl, IControlable, IEventBaseUser
  {
    ICollection<IDevices> devices;
    ICollection<IRemouteInterfaces> interfaces; 
    void sendControlPoint(); 
      
  }


}
