using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using SimplifiedCoreExternalInterface;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Threading; 



namespace ConsoleClient {

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




    public class ConsoleInstance
    {
        /// <summary>
        /// Name of this server for identify by the console instance
        /// </summary>
        protected  string _pipeServerName;

        /// <summary>
        /// File name of the console instance programm.
        /// </summary>
        protected const string PIPE_CLIENT_FILE_NAME = "ConsoleInstance.exe";

       ///
        protected NamedPipeServerStream pipeServer;
        protected Process pipeClient;
        protected StreamWriter Writer;
        protected StreamReader Reader;

        protected Logger logger = LogManager.GetCurrentClassLogger();


        public ConsoleInstance(string pipeServerName)
        {
            _pipeServerName = pipeServerName;
            PipesInitialize();

        }



        /// <summary>
        /// Initialize pipe stream to instance of console. 
        /// </summary>
        public void PipesInitialize()
        {

            pipeServer = new NamedPipeServerStream(_pipeServerName, PipeDirection.InOut, 1);




            Task waitTask = new Task(pipeServer.WaitForConnection);
            waitTask.Start();
            pipeClient = new Process();

            pipeClient.StartInfo.Arguments = _pipeServerName;
            string dir = Directory.GetCurrentDirectory() + "\\" + PIPE_CLIENT_FILE_NAME; ;
            pipeClient.StartInfo.FileName = dir;


            pipeClient.StartInfo.UseShellExecute = true;
            pipeClient.Start();

            while (!waitTask.IsCompleted)
            {
                Thread.Sleep(100);
            }

            Writer = new StreamWriter(pipeServer);
            Reader = new StreamReader(pipeServer);

            //pipeServer.DisposeLocalCopyOfClientHandle();
            logger.Info("Pipes have initialized.");
        }



        /// <summary>
        /// Write message to console instance
        /// </summary>
        /// <param name="message">Message to be written</param>
        /// <param name="isCommand">Is the message a command message? </param>
        public void Write(string message, bool isCommand = false)
        {
            logger.Info("Message {0} will be written to console instance.", message);
            Writer.AutoFlush = true;
            if (!isCommand)
                Writer.WriteLine("SYNC");
            pipeServer.WaitForPipeDrain();
            Writer.WriteLine(message);
            pipeServer.WaitForPipeDrain();
        }



        /// <summary>
        /// Read string from console Instance.
        /// </summary>
        /// <param name="prompt">Prompt to be written before reading.</param>
        /// <returns></returns>
        public string Read(string prompt = "")
        {
            Write("READ" + prompt, true);

            
            string ret = Reader.ReadLine();


            logger.Info("Message {0} was received from console instance.");
            return ret;
        }





        public void Dispose()
        {
            Write("STOP");

            Writer.Close();
            Reader.Close();

            pipeClient.WaitForExit();
            pipeClient.Close();

            pipeServer.Close();
            logger.Info("Client has disposed.");


        }

    }




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
            catch (Exception e)
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









