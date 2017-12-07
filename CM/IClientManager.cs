using System;

namespace Core
{
    namespace CM
    {
        interface IClientManager : IControler, IControlable, ILoggable, IEventBaseUser, IPositionBaseReader
        {
            ICollection<IClient> clientsBase;
            void SendCommand(IControlable target, ICommand command);
            IPositionBaseReader positionBase;
            IEventBase eventsBase;
            
        }
    }
}