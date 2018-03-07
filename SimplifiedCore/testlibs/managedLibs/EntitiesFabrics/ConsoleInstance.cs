using System.Threading.Tasks;
using NLog;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Threading;



namespace EntitiesFabric.ConsoleClient
    {




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






}









