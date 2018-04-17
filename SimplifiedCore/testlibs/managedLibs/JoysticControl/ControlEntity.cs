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
            int length = BitConverter.ToInt32(data.Take(4).ToArray(), 0);
            Speed sp = Speed.Parser.ParseFrom(data.Skip(4).Take(length).ToArray());

            //string str = "" + sp.LeftSpeed + " " + sp.RightSpeed + "\n";
            //byte[] buff = Encoding.UTF8.GetBytes(str);
            byte[] buff = new byte[4];
            
            BitConverter.GetBytes((Int16)sp.LeftSpeed).CopyTo(buff, 0) ;
            BitConverter.GetBytes((Int16)sp.RightSpeed).CopyTo(buff, 2) ;
            buff.Reverse();

            ConsoleClient.Write(string.Format("joystic ->{0}: {1} ;{2}", sp.DestId, sp.LeftSpeed, sp.RightSpeed));

            RemoteController.CommandsToSend.Push(new Command(
                RemoteController.MacAdresses[sp.DestId], buff));


        }



        public override void Shutdown()
        {
            RemoteController.Destroy();
        }
    }
}
