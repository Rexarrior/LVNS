#region  description
/*
Containt description for model of any remoute intrface. 
 */
#endregion

#region  includes
using System; 
using ZeroGenerationInterfaces;
using System.Collection;

#endregion


namespace Core
{
    namespace RSDispatcher
    {
        interface IRemouteInterface: IControlable
        {
            string name; 
            ICollections acceptingCommands; 
            
        }
    }
}