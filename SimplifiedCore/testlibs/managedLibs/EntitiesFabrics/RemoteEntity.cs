using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using NLog;





namespace EntitiesFabric
{
    public class RemoteEntity : Entity
    {
        #region fields 
        public const int BUFF_SIZE = 1024;

        protected IPEndPoint _ipAddress;
        protected Socket _socket;
        protected Task _task;


        #endregion


        protected virtual void _receiveAction()
        { }



        protected virtual void _sendAction()
        {

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






        public RemoteEntity(string selfMid, string acceptMid, bool isReceiver, string ip, int port) :
           base(selfMid, acceptMid, isReceiver)
        {




            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _ipAddress = new IPEndPoint(IPAddress.Parse(ip), port);

            if (isReceiver)
                _socket.Bind(_ipAddress);

            if (isReceiver)
                _task = new Task(_receiveAction);
            else
                _task = new Task(_sendAction);

            _socket.ReceiveBufferSize = BUFF_SIZE;
            _socket.SendBufferSize = BUFF_SIZE;

            _loadDelegate = this.Load;
            _receiveDelegate = this.Receive;



        }

    }



}





