using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesFabrics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Google.Protobuf;
using System.ComponentModel.Composition;

namespace RobotsInterfaces
{
    [PartNotDiscoverable()]
    [Export(typeof(IEntity))]
    class ProtoBuffReceiver : Entity, IDisposable
    {
        public const string IP = "";
        public const int PORT = 55555; 
        Socket _socket;
        Task _task;

        byte[] _data; 

        private void _listenAction()
        {
            IPEndPoint remoteIP = null;
            while (true)
            {
                byte[] tmp = new byte[256]; 
                 _socket.Receive(tmp);
                lock(_data)
                {
                    _data = tmp; 
                }
            }
        }



        private Position _parsePosition (byte[] dataInProto)
        {

            SSL_DetectionFrame frame = SSL_DetectionFrame.Parser.ParseFrom(dataInProto);
            List<Unit> units = new List<Unit>(frame.RobotsBlue.Count + frame.RobotsYellow.Count);

            foreach (var item in frame.RobotsBlue)
            {

                units.Add(new Unit());
                units[units.Count - 1].X = item.X;
                units[units.Count - 1].Y = item.Y;
                units[units.Count - 1].Orientation = item.Orientation;
                units[units.Count - 1].Color = Unit.Types.UnitColor.Blue;

            }
            foreach (var item in frame.RobotsYellow)
            {
                units.Add(new Unit());
                units[units.Count - 1].X = item.X;
                units[units.Count - 1].Y = item.Y;
                units[units.Count - 1].Orientation = item.Orientation;
                units[units.Count - 1].Color = Unit.Types.UnitColor.Yellow;
                units[units.Count - 1].Id = item.RobotId;



            }

            Position pos = new Position();
            pos.Count =(uint) units.Count;
            pos.Units.Add(units);

            return pos;

        }



       public void Dispose()
        {
            ((IDisposable)_socket).Dispose();
            ((IDisposable)_task).Dispose();

        }



        public override void Load(byte[] data)
        {
           
               
            while (_data == null)
                Thread.Sleep(100);
            Position pos;
            lock (_data)
            {
              pos = _parsePosition(_data);
            }
            int n = pos.CalculateSize();
            if (n > data.Count())
                throw new ArgumentException("Wrong Core buffer size. So small.");
            MemoryStream stream = new MemoryStream(n);
            pos.WriteTo(stream);
            byte[] tmp = stream.GetBuffer();
            for (int i = 0; i < n; i++)
            {
                data[i] = tmp[i];
            }
        }


        [ImportingConstructor]
        public ProtoBuffReceiver() : base("ProtoBuffReceiver", "Matlab", false)
        {
            logger.Info("ProtoBuff receiver is creating...");
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            _socket.MulticastLoopback = true;
            

            _task = new Task(_listenAction);
            _task.Start();


        }
    }
}
