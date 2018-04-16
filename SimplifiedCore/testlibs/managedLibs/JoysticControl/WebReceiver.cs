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
using WebSocketServer; 
namespace JoysticControl
{
    [Export(typeof(IEntity))]
    class WebReceiver : Entity
    {
        private const string IPADDRESS = "100.100.100.100";
        private const int PORT = 5555;
        private const int SLEEPTIME = 10;
        private const int MAXTIMEWITHOUTDATA = 60000;

        WebSocketServer.WebSocketServer _server;

        private List<WebSocketServer.WebSocketConnection> _connections; 


        private Task _initTask;




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

        public override void Load(byte[] data)
        {
            if (!_initTask.IsCompleted)
                Thread.Sleep(10);
          
        }



        public override void Shutdown()
        {
             if (!_initTask.IsCompleted)
                Thread.Sleep(10);
         

        }


        [ImportingConstructor]
        public WebReceiver() : base("WebReceiver", "ControlEntity", false)
        {
            logger.Info("WEBRECEIVER: building web receiver is starting");
            _initTask = new Task(delegate ()
            {
                _connections = new List<WebSocketConnection>();
                _server = new WebSocketServer.WebSocketServer(PORT, @"http://localhost:8080", @"ws://localhost:5555");
               _server.ClientConnected += new ClientConnectedEventHandler(_onClientConnected);
                _server.Start();
            
            });
            _initTask.Start();
        }
        
    }
}
