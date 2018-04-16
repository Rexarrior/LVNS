using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using System.Threading;
using NLog;
using System.Net;
using System.Net.Sockets;
using EntitiesFabrics;




namespace Messages
{


    /// <summary>
    /// Buffer for data stream between remoute entities and transfer entities
    /// </summary>
    public static class DataBuffer
    {
        /// <summary>
        /// Some messages from remote point to console
        /// </summary>
        public static Stack<byte[]> AavailableMessagesToConsole = new Stack<byte[]>();



        /// <summary>
        /// Some messages from console to remoute point
        /// </summary>
        public static Stack<byte[]> AavailableMessagesToSend = new Stack<byte[]>();

    }




    /// <summary>
    /// Base class for a message entity as message sender or message receiver
    /// </summary>
    [PartNotDiscoverable]
    public class MessageEntity : RemoteEntity
    {
        public const string DEFINED_IP = "127.0.0.1";
        public const int DEFINED_PORT = 5555;

        /// <summary>
        /// Sending action for run in the task. 
        /// </summary>
        protected override void _sendAction()
        {
            try
            {
                if (DataBuffer.AavailableMessagesToSend.Count > 0)
                {
                    lock (DataBuffer.AavailableMessagesToSend)
                    {

                        _socket.SendTo(DataBuffer.AavailableMessagesToSend.Pop(),
                            _ipAddress);
                    }
                }


            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                Shutdown();
            }

        }


        /// <summary>
        /// Receiving action for run in the task.
        /// </summary>
        protected override void _receiveAction()
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
                lock (DataBuffer.AavailableMessagesToConsole)
                {
                    DataBuffer.AavailableMessagesToConsole.Push(endData);
                }
                logger.Info("{0}:{1} ", remoteFullIp.Address.ToString(),
                                                remoteFullIp.Port);


            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                Shutdown();
            }
        }



        
        public MessageEntity(string selfMid, string acceptMid, bool isReceiver) :
            base(selfMid, acceptMid, isReceiver, DEFINED_IP, DEFINED_PORT)
        {




        }
    }





    /// <summary>
    /// Message sender entity
    /// </summary>
    [Export(typeof(IEntity))]
    public class MessageSender : MessageEntity, IEntity
    {

        public override void Load(byte[] data)
        {
            base.Load(data);
        }


        [ImportingConstructor]
        public MessageSender() : base("MessageSender", "MessageReceiver", false)
        {
            _loadDelegate = Load;
            logger.Info("Message sender has created.");
        }
    }




    /// <summary>
    /// Message receiver entity
    /// </summary>
    [Export(typeof(IEntity))]
    public class MessageReceiver : MessageEntity, IEntity
    {
        public override void Receive(byte[] data)
        {
            base.Receive(data);
        }


        [ImportingConstructor]
        public MessageReceiver() : base("MessageReceiver", "MessageSender", true)
        {
            logger.Info("Message receiver has created.");
            _receiveDelegate = Receive;
        }

    }

#region CosoleServerEntities

    /// <summary>
    /// Transfer sender for provide a transfer between messageEntities and ConsoleClients
    /// </summary>
    [Export(typeof(IEntity))]
    public class ConsoleServerSender : TransferEntity, IEntity
    {
      

        [ImportingConstructor]
        public ConsoleServerSender():base("ConsoleServerSender", "ConsoleClientReceiver", false, DataBuffer.AavailableMessagesToConsole)
        {
            logger.Info("Console serer sender has created.");

        }
    }




    /// <summary>
    /// Transfer receiver for provide a transfer between messageEntities and ConsoleClients
    /// </summary>
    [Export(typeof(IEntity))]
    public class ConsoleServerReceiver : TransferEntity, IEntity
    {

        [ImportingConstructor]
        public ConsoleServerReceiver() : base("ConsoleServerReceiver", "ConsoleClientSender", true, DataBuffer.AavailableMessagesToSend)
        {
            logger.Info("Console server receiver has created.");

        }
    }


    #endregion

}





