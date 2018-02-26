using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Security.Principal;
using System.Linq;
using NLog; 

namespace ConsoleInstance
{
    class PipeClient
    {
        static StreamReader Reader;
        static StreamWriter Writer;

        static Logger loger = LogManager.GetCurrentClassLogger();
        static NamedPipeClientStream pipeClient;


        static void PipeInit(string[] args)
        {
            loger.Info("Initialize pipe client. arguments: {0}",
                args.Aggregate("",(acum, x)=>acum + " " + x));

            try
            {
                pipeClient = new NamedPipeClientStream(".", args[0],
                            PipeDirection.InOut, PipeOptions.None,
                            TokenImpersonationLevel.Impersonation);

                pipeClient.Connect();


                Reader = new StreamReader(pipeClient);
                Writer = new StreamWriter(pipeClient);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR! Message: {0}", e.Message);
                Console.WriteLine("Press any key for exit...");
                Console.ReadKey(true);
                System.Environment.Exit(0);
            }
            loger.Info("Pipes has initialized.");
        }




        static void Dispose()
        {
            Writer.Close();
            Reader.Close();
            pipeClient.Close();
        }


        static void WriteToServer(string message)
        {
            loger.Info("Message {0} will be written to sever. ", message);
            Writer.AutoFlush = true;
            //Writer.WriteLine("SYNC");
            Writer.WriteLine(message);
            pipeClient.WaitForPipeDrain();
        }



        static string ReadFromServer()
        {
            string res = Reader.ReadLine();
            
            loger.Info("Message {0} received from server.", res);
            return res; 

        }



        static void ReadForServer(string prompt)
        {
            loger.Info("Reading for server...");
            Console.Write("[SERVER] {0}", prompt);
            WriteToServer(Console.ReadLine());
            
        }




        static void Exit()
        {
            loger.Info("Closing...");
            Dispose();
            Console.WriteLine("[CLIENT] Server disconnected.");
            Console.Write("[CLIENT] Press Enter to continue...");
            Console.ReadLine();
            loger.Info("Program has end."); ;
            System.Environment.Exit(0);
        }





        static void Main(string[] args)
        {
            loger.Info("Program starting...");
            if (args.Length > 0)
            {
                PipeInit(args);
                Console.WriteLine("[CLIENT] Starting this instance. There will be showed any message from server.");

 
                string temp ="";
                while (true)
                {
                    temp = Reader.ReadLine();
                    if (temp.StartsWith("STOP"))
                        Exit();

                    if (temp.StartsWith("READ"))
                        ReadForServer(temp.Substring(4));

                    if (temp.StartsWith("SYNC"))
                        Console.WriteLine(ReadFromServer());                                                                     
                    
                }

            }

            Console.WriteLine("[CLIENT] Server not specified. Closing....");
            Thread.Sleep(2000);
        }
    }
}