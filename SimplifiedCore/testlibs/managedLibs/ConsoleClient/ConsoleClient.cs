using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using EntitiesFabrics;
using EntitiesFabrics.ConsoleClient;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace ConsoleClient
{

    /// <summary>
    /// Data buffer for stream between console instance and console entities.  
    /// </summary>
    static class DataBuffer
    {
        public static List<string> AvailableMessagesToSend = new List<string>();
        public static List<string> AvailableMessagesToConsole = new List<string>();

    }






    /// <summary>
    /// Provide data stream between message entities and console. 
    /// </summary>
    #region ENTITIES
    [PartNotDiscoverable]
    public class ConsoleClient : Entity
    {


        public override void Receive(byte[] data)
        {
            try
            {
                string message = Encoding.Unicode.GetString(data).Replace("\0", "");
                if (message != "")
                    lock (DataBuffer.AvailableMessagesToConsole)
                    {
                        DataBuffer.AvailableMessagesToConsole.Add(message);
                    }
            }
            catch (Exception e)
            {
                logger.Error("in time of decoding the message the error was happend. Message: {0}", e.Message);
            }
        }


        public override void Load(byte[] data)
        {
            try
            {
                int i = 0;
                lock (DataBuffer.AvailableMessagesToSend)
                {
                    while (DataBuffer.AvailableMessagesToSend.Count > 0)
                    {
                        i = Encoding.Unicode.GetBytes(
                            DataBuffer.AvailableMessagesToSend[0], 0,
                            DataBuffer.AvailableMessagesToSend[0].Length, data, i);
                        DataBuffer.AvailableMessagesToSend.RemoveAt(0);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("in time of decoding message the error was happend. Message: {0}",
                    e.Message);
            }
        }




        public ConsoleClient(string selfMid, string acceptMid, bool isReceiver) : base(selfMid, acceptMid, isReceiver)
        {
            _receiveDelegate = Receive;
            _loadDelegate = Load;

        }
    }





    [Export(typeof(IEntity))]
    public class ConsoleClientSender : ConsoleClient, IEntity
    {
        [ImportingConstructor]
        public ConsoleClientSender() : base("ConsoleClientSender", "ConsoleServerReceiver", false)
        {
            logger.Info("Console client sender has created.");
        }
    }






    [Export(typeof(IEntity))]
    public class ConsoleClientReceiver : ConsoleClient, IEntity
    {
        public static event EventHandler MessageReceived;

        Task _clientRunnerTask;
        List<Task> _receivingTasks;
        MessageConsoleInstance _consoleReaderClient;
        MessageConsoleInstance _consoleWriterClient;

        private void _receiveAction()
        {
            MessageReceived(this, EventArgs.Empty);
        }


        private void _clearTasks()
        {
            _receivingTasks.RemoveAll(x => x.IsCompleted);
        }



        public override void Receive(byte[] data)
        {
            if (data.Any(x => x != 0))
            {
                base.Receive(data);
                lock (_receivingTasks)
                {
                    _clearTasks();

                    _receivingTasks.Add(new Task(_receiveAction));
                    _receivingTasks.Last().Start();
                }
            }
            //MessageReceived.BeginInvoke(this, EventArgs.Empty, null, null);


        }


        [ImportingConstructor]
        public ConsoleClientReceiver() : base("ConsoleClientReceiver", "ConsoleServerSender", true)
        {
            _receivingTasks = new List<Task>();
            _receiveDelegate = Receive;
            logger.Info("Console client receiver has created.");

            _consoleReaderClient = new MessageConsoleInstance("PipeReadServer");
            _consoleWriterClient = new MessageConsoleInstance("PipeWriteServer");
            _consoleWriterClient.SetMessageHandler();

            _consoleReaderClient.Write("This instance is intended for reading information from user.");
            _consoleWriterClient.Write("This instance is intended for writing information to user. ");

            _clientRunnerTask = new Task(_consoleReaderClient.Run);
            _clientRunnerTask.Start();
        }
    }

#endregion




    public class MessageConsoleInstance: ConsoleInstance
    {

        /// <summary>
        /// Task for the message request loop
        /// </summary>
        protected Task RunTask;
        
        /// <summary>
        /// Timeout between message request
        /// </summary>
        protected const int SLEEP_TIME = 100;


        /// <summary>
        /// Handeeler for ConsoleClient.MessageReceived;
        /// </summary>
        public void MessageHandler(object sender, EventArgs args)
        {
            logger.Info("Message handler invoked.");
            lock (DataBuffer.AvailableMessagesToConsole)
            {
                while (DataBuffer.AvailableMessagesToConsole.Count > 0)
                {
                    string str = DataBuffer.AvailableMessagesToConsole[0];
                    Write(str);
                    DataBuffer.AvailableMessagesToConsole.RemoveAt(0);
                }
            }
        }


        /// <summary>
        /// Set messageHandler to it's event.
        /// </summary>
        public void SetMessageHandler()
        {
            ConsoleClientReceiver.MessageReceived += MessageHandler;

        }



        /// <summary>
        /// initialize run task
        /// </summary>
        private void _initialize()
        {
            
            logger.Info("Console client has initialized.");
            RunTask = new Task(MessageRequestAction);
        }


        /// <summary>
        /// Action of requesting a message from the console instance
        /// </summary>
        public void MessageRequestAction()
        {
            try
            {
              
                logger.Info("New message request.");
                Write("Write any message to send: ");
                string message = Read();
                if (message != "")
                    DataBuffer.AvailableMessagesToSend.Add(message);
                
            }
            catch (Exception)
            {
                Dispose();
            }
        }



        /// <summary>
        /// Run loop
        /// </summary>
        public void Run()
        {
           
            while (true)
            {
                if (RunTask.IsCompleted)
                    RunTask = new Task(MessageRequestAction);

                if (RunTask.Status == TaskStatus.Created)
                {
                    RunTask.Start();
                }
                Thread.Sleep(SLEEP_TIME);

            }
        }



        public MessageConsoleInstance(string pipeServerName):base(pipeServerName)
        {
            _initialize();

        }

    }






}









