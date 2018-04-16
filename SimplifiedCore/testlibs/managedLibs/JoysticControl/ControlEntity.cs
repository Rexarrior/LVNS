using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesFabrics;
using RobotsInterfaces;
using System.ComponentModel.Composition;

namespace JoysticControl
{
    [Export(typeof(IEntity))]
    public class ControlEntity : Entity
    {


        [ImportingConstructor]
        public ControlEntity() : base("ControlEntity", "WebReceiver", true)
        {
            RemoteController.Init();
        }




        public override void Receive(byte[] data)
        {
            if (data.All(x => x == 0))
                return;

            Speed sp = Speed.Parser.ParseFrom(data);
            byte[] twoCharBuff = new byte[4];
            BitConverter.GetBytes((Int16)sp.LeftSpeed).CopyTo(twoCharBuff, 0) ;
            BitConverter.GetBytes((Int16)sp.RightSpeed).CopyTo(twoCharBuff, 2) ;

            ConsoleClient.Write(string.Format("joystic ->{0}: {1} ;{2}", sp.DestId, sp.LeftSpeed, sp.RightSpeed));

            RemoteController.CommandsToSend.Push(new Command(
                RemoteController.MacAdresses[sp.DestId], twoCharBuff));


        }



        public override void Shutdown()
        {
            RemoteController.Destroy();
        }
    }
}
