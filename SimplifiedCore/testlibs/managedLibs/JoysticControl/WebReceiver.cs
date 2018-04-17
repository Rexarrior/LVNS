using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesFabrics;
using EntitiesFabrics.Analiziers;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using Google.Protobuf;
using System.ComponentModel.Composition;
using RobotsInterfaces;
using NLog;
using System.Net.WebSockets;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;
namespace JoysticControl
{
    public class JsonVec
    {
        double x;
        double y;

        public JsonVec(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double X { get => x; set => x = value; }
        public double Y { get => y; set => y = value; }
    }



    public class Speeds : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            
            JsonVec vec = JsonConvert.DeserializeObject<JsonVec>(e.Data);
            Speed sp = new Speed();

            vec.X = -vec.X;
            sp.LeftSpeed = (int)Math.Round((vec.Y - vec.X) * 100 );
            sp.RightSpeed = (int)Math.Round((vec.Y + vec.X)* 100 );

            if (sp.LeftSpeed > 100)
                sp.LeftSpeed = 100;


            if (sp.RightSpeed > 100)
                sp.RightSpeed = 100;

            if (sp.LeftSpeed < -100)
                sp.LeftSpeed = -100;


            if (sp.RightSpeed < -100)
                sp.RightSpeed = -100;

            
            WebReceiver.CurrSpeed = sp;
           // Console.WriteLine("x: {2:0.###}; y: {3:0.###}; left: {0}; right: {1}",
           //     sp.LeftSpeed, sp.RightSpeed, vec.X, vec.Y
           //     );
        }
    }


    [Export(typeof(IEntity))]
    class WebReceiver : Entity
    {
        private const string IPADDRESS = "100.100.100.100";
        private const int PORT = 5555;
        private const int SLEEPTIME = 10;
        private const int MAXTIMEWITHOUTDATA = 60000;

        private WebSocketServer _server;
        public static Speed CurrSpeed; 


        private Task _initTask;



        /*
                private void _onClientConnected(WebSocketConnection sender, EventArgs e)
                {
                    _connections.Add(sender );
                    sender.Disconnected += new WebSocketDisconnectedEventHandler(_onClientDisconnected);
                    sender.DataReceived += new DataReceivedEventHandler(_onClientMessage);

                }
                private void _onClientMessage(WebSocketConnection sender, DataReceivedEventArgs e)
                {
                    string data = e.Data;
                    ConsoleClient.Write(data);
                }

                private void _onClientDisconnected(WebSocketConnection sender, EventArgs e)
                {
                    //do nothing
                }
                */

        public override void Load(byte[] data)
        {
            if (!_initTask.IsCompleted)
                Thread.Sleep(10);

            try
            {
                while (CurrSpeed == null)
                    Thread.Sleep(10);

                int length = CurrSpeed.CalculateSize();
                BitConverter.GetBytes(length).CopyTo(data, 0);
                CurrSpeed.ToByteArray().CopyTo(data, 4);

                CurrSpeed = null;
            }
            catch (Exception e)
            { logger.Error("WEBRECEIVER: in time of loading next error happend: {0}", e.Message); }
        }



        public override void Shutdown()
        {
             if (!_initTask.IsCompleted)
                Thread.Sleep(10);
            _server.Stop();
         

        }


        [ImportingConstructor]
        public WebReceiver() : base("WebReceiver", "ControlEntity", false)
        {
            logger.Info("WEBRECEIVER: building web receiver is starting");
            _initTask = new Task(delegate ()
            {
                _server = new WebSocketServer("ws://localhost:5555");
                _server.AddWebSocketService<Speeds>("/speeds");
                _server.Start();
                /*
                _connections = new List<WebSocketConnection>();
                _server = new WebSocketServer.WebSocketServer(PORT, @"http://localhost:8080", @"ws://localhost:5555");
               _server.ClientConnected += new ClientConnectedEventHandler(_onClientConnected);
                _server.Start();
            */



            });
            _initTask.Start();
        }
        
    }
}
