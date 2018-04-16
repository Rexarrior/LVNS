using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using System.ComponentModel.Composition;
using System.Net.Sockets;
using System.Net;
using EntitiesFabrics;
using EntitiesFabrics.ConsoleClient;

namespace JsonServer
{

    


    public class Unit
    {
        public Unit(int iD, int angle, int x, int y)
        {
            ID = iD;
            Angle = angle;
            X = x;
            Y = y;
        }

        /// <summary>
        /// Identifier for the unit
        /// </summary>
        public int ID { get; set; }



        /// <summary>
        /// Angle of orientation of the unit
        /// </summary>
        public int Angle {get; set;}


        /// <summary>
        /// X coorditane of the unit
        /// </summary>
        public int X { get; set; }


        /// <summary>
        /// X coorditane of the unit
        /// </summary>
        public int Y { get; set; }


    }





    static class DataBuffer
    {
        public static List<string> UnitsToMatlab = new List<string>();
        public static List<string> UnitsToSend = new List<string>();
        public static List<string> UnitsToConsole = new List<string>();

    }







    [PartNotDiscoverable]
    public class JsonRemouteEntity: Entity
    {


        #region fields
        public const string DEFINED_IP = "127.0.0.1";
        public const int PORT = 5555;
        private const int BUFFER_SIZE = 1024;

        private Socket _socket;

        private Task _task;

        #endregion




        private void _sendAction()
        {
            try
            {
                if (DataBuffer.UnitsToSend.Count > 0)
                {
                    lock (DataBuffer.UnitsToSend)
                    {
                        byte[] data = new byte[BUFFER_SIZE];

                        Encoding.Unicode.GetBytes(
                           DataBuffer.UnitsToSend[0], 0,
                           DataBuffer.UnitsToSend[0].Length, data, 0);

                        DataBuffer.UnitsToSend.RemoveAt(0);

                        _socket.SendTo(data,
                            new IPEndPoint(IPAddress.Parse(DEFINED_IP), PORT));

                        DataBuffer. UnitsToSend.RemoveAt(0);
                    }
                }


            }
            catch (Exception ex)
            {
                logger.Error("ERROR:" + ex.Message);
                Shutdown();
            }

        }




        private void _receiveAction()
        {

            try
            {

                int bytes = 0;
                byte[] data = new byte[1024];
                List<byte[]> allData = new List<byte[]>();
                List<int> sizes = new List<int>();
                EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

                do
                {
                    bytes = _socket.ReceiveFrom(data, ref remoteIp);
                    allData.Add(data);
                    sizes.Add(bytes);
                }
                while (_socket.Available > 0);

                IPEndPoint remoteFullIp = remoteIp as IPEndPoint;
                int allSize = sizes.Aggregate(0, (x, y) => (x + y));

                byte[] endData = new byte[allSize];
                int k = 0;
                for (int j = 0; j < allData.Count; j++)
                {
                    for (int i = 0; i < sizes[j]; i++)
                    {
                        endData[k++] = allData[j][i];

                    }
                }

                string unitStr = Encoding.Unicode.GetString(endData);
                lock (DataBuffer.UnitsToMatlab)
                {
                    
                    DataBuffer.UnitsToMatlab.Add(unitStr);
                }
                logger.Info("From {0}:{1} received {2}.", remoteFullIp.Address.ToString(),
                                                remoteFullIp.Port, unitStr);


            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                Shutdown();
            }
        }


        public override void Load(byte[] data)
        {

            if (_task.IsCompleted)
                _task = new Task(_sendAction);

            if (
               _task.Status == TaskStatus.Created
                )
            {
                _task.Start();
            }
        }




        public override void Receive(byte[] data)
        {

            if (_task.IsCompleted)
                _task = new Task(_receiveAction);

            if (
               _task.Status == TaskStatus.Created
                )
            {
                _task.Start();
            }


        }



        public override void Shutdown()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();
            logger.Info("Entity id {0} has shutdowned.", _id);
        }




        public JsonRemouteEntity(string selfMid, string acceptMid, bool isReceiver) :
            base(selfMid, acceptMid, isReceiver)
        {




            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            if (isReceiver)
                _socket.Bind(new IPEndPoint(IPAddress.Parse(DEFINED_IP), PORT));

            if (isReceiver)
                _task = new Task(_receiveAction);
            else
                _task = new Task(_sendAction);

            _socket.ReceiveBufferSize = BUFFER_SIZE;
            _socket.SendBufferSize = BUFFER_SIZE;

            _loadDelegate = this.Load;
            _receiveDelegate = this.Receive;

        }
    }







    [PartNotDiscoverable]
    public class MatlabServerEntity : Entity
    {

        public override void Load(byte[] data)
        {
            if (DataBuffer.UnitsToMatlab.Count > 0)
            {
                lock (DataBuffer.UnitsToMatlab)
                {
                    Encoding.Unicode.GetBytes(
                       DataBuffer.UnitsToMatlab[0], 0,
                       DataBuffer.UnitsToMatlab[0].Length, data, 0);

                    DataBuffer.UnitsToMatlab.RemoveAt(0);
                    
                }
            }
        }



        public override void Receive(byte[] data)
        {
            if (data.Any(x => x != 0))
            {
                string unitStr = Encoding.Unicode.GetString(data);

                lock (DataBuffer.UnitsToMatlab)
                {
                    DataBuffer.UnitsToMatlab.Add(unitStr);
                }

                lock (DataBuffer.UnitsToConsole)
                {
                    DataBuffer.UnitsToConsole.Add(unitStr);
                }
            }
        }

        public MatlabServerEntity(string selfMID, string acceptMID, bool isReceiver) :
            base(selfMID, acceptMID, isReceiver)
        {
            this._loadDelegate = this.Load;
            this._receiveDelegate = this.Receive;
        }
    }





    public class ConsoleServer : Entity
    {

        ConsoleInstance console;

        public override void Receive(byte[] data)
        {
            {
                lock (DataBuffer.UnitsToConsole)
                {
                    while (DataBuffer.UnitsToConsole.Count > 0)
                    {
                        console.Write(DataBuffer.UnitsToConsole[0]);
                        DataBuffer.UnitsToConsole.RemoveAt(0);
                    }
                }
            }
        }

        



        public ConsoleServer(string selfMid, string acceptMid, bool isReceiver) : base(selfMid, acceptMid, isReceiver)
        {

            console = new ConsoleInstance("Json server");
            
        }
    }












    










}
