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

        private TcpListener _listener;
        private Task _listenTask;

        private CancellationTokenSource _listenTasckCancellation; 

        private Queue<Speed> _dataBuffer;


        private Task _initTask;


       



        private void _response(TcpClient client, NetworkStream stream)
        {
            Thread.Sleep(3000);
            Byte[] bytes = new Byte[client.Available];

            stream.Read(bytes, 0, bytes.Length);

            //translate bytes of request to string
            String data = Encoding.UTF8.GetString(bytes);
            ConsoleClient.Write(data);
            if (new System.Text.RegularExpressions.Regex("^GET").IsMatch(data))
            {
                const string eol = "\r\n"; // HTTP/1.1 defines the sequence CR LF as the end-of-line marker


                string str = Convert.ToBase64String(
                        System.Security.Cryptography.SHA1.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(
                                new System.Text.RegularExpressions.Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                            )

                        )
                    ) + eol;
                Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + eol
                    + "Upgrade: websocket" + eol
                    + "Connection: Upgrade" + eol
                    + "Sec-WebSocket-Accept: " + str
                    
                    + eol);

                stream.Write(response, 0, response.Length);
            }

        }


/*
        private void _receiveMessageHandler(WebSocketSession session, string message)
        {
            Speed speed = new Speed();
            JsonVec vec = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonVec>(message);

            speed.LeftSpeed = (int)Math.Round(vec.x * 100);
            speed.RightSpeed = (int)Math.Round(vec.y * 100);

            if (speed.LeftSpeed < -100 || speed.RightSpeed > 100 || speed.RightSpeed < -100 || speed.LeftSpeed > 100)
                throw new Exception(string.Format("Wronp data received:  ({0}, {1})", speed.LeftSpeed, speed.RightSpeed));
            lock (_dataBuffer)
            {
                _dataBuffer.Enqueue(speed);
            }
            ConsoleClient.Write(String.Format("Received next speeds: ({0}, {1})", speed.LeftSpeed, speed.RightSpeed));


        }
        */


        private void _listenAction()
        {
            if (!_initTask.IsCompleted)
                Thread.Sleep(10);
              _listener.Start();
              List<Task> clientsTasks = new List<Task>();
              List<CancellationToken> tokens = new List<CancellationToken>();
              
            while (true)
              {
                
                  TcpClient client = _listener.AcceptTcpClient();
                  int next = clientsTasks.Count;
                  clientsTasks.Add(new Task(delegate () {
                      try
                      {

                          int myNum = next; 
                          TcpClient subClient = client;

                          NetworkStream stream = subClient.GetStream();

                          _response(subClient, stream);
                          int i = 0;
                          //while (!stream.DataAvailable) ;
                          int unitNum = 0;
                          byte[] messageBuff = new byte[4096];

                          //stream.Read(twoDoubleBuff, 0, 4);

                          //unitNum = BitConverter.ToInt32(twoDoubleBuff, 0);
                          ConsoleClient.Write(String.Format("Connected to joystic for {0} unit. ", unitNum));

                          while (i < MAXTIMEWITHOUTDATA)
                          {
                              
                              if (tokens[next].IsCancellationRequested)
                              {
                                  stream.Close(1000);
                                  client.Close(); 
                                  return;
                              }
                              while (stream.DataAvailable)
                              {

                                  i = 0;
                                  stream.Read(messageBuff, 0, client.Available);
                                  JsonVec vec =  JsonVec.Parse(messageBuff);
                                  Speed speed = new Speed();
                                  speed.LeftSpeed = (int)Math.Round(vec.x * 100);
                                  speed.RightSpeed = (int)Math.Round(vec.y * 100);

                                  if (speed.LeftSpeed < -100 || speed.RightSpeed > 100 || speed.RightSpeed < -100 || speed.LeftSpeed > 100)
                                      throw new Exception(string.Format("Wronp data received:  ({0}, {1})", speed.LeftSpeed, speed.RightSpeed));
                                  lock (_dataBuffer)
                                  {
                                      _dataBuffer.Enqueue(speed);
                                  }
                                  ConsoleClient.Write(String.Format("Received next vector: ({0}, {1})", speed.LeftSpeed, speed.RightSpeed));

                              }

                              i += SLEEPTIME;
                              Thread.Sleep(SLEEPTIME);
                          }
                      }
                      catch (Exception E)
                      {
                          logger.Error("[WebReceiver]In time of receiving data the error happened: {0}", E.Message);
                      }
                      }));

                  clientsTasks.Last().Start();
                  tokens.Add(_listenTasckCancellation.Token);
                  for (int j = 0; j < clientsTasks.Count; j++)
                  {
                      if (clientsTasks[j].IsCompleted)
                      {
                          tokens.RemoveAt(j);
                          clientsTasks.RemoveAt(j) ;
                      }
                  }
                  

                

          
            }

        }



        public override void Load(byte[] data)
        {
            if (!_initTask.IsCompleted)
                Thread.Sleep(10);
            Speed sp;
            if (_dataBuffer.Count == 0)
                return;

                lock(_dataBuffer)
                {
                    sp = _dataBuffer.Dequeue();
                }
            
            int n = sp.CalculateSize();
            MemoryStream mstream = new MemoryStream(n);
            sp.WriteTo(mstream);
            if (n < data.Count())
                mstream.ToArray().CopyTo(data, 0);
            else
                logger.Error("The Core data buffer size too smal. Nothing have been write to.");
        }



        public override void Shutdown()
        {
             if (!_initTask.IsCompleted)
                Thread.Sleep(10);
             
            _listenTasckCancellation.Cancel();
            _listenTask.Wait(2 * SLEEPTIME);
            _listener.Stop();

        }


        [ImportingConstructor]
        public WebReceiver() : base("WebReceiver", "ControlEntity", false)
        {
            logger.Info("WEBRECEIVER: building web receiver is starting");
            _initTask = new Task(delegate ()
            {
                _dataBuffer = new Queue<Speed>();
                _listenTasckCancellation = new CancellationTokenSource();
                 _listener = new TcpListener(IPAddress.Any, PORT);
                


                _listenTask = new Task(_listenAction);
                _listenTask.Start();
            
            });
            _initTask.Start();
        }
        
    }
}
