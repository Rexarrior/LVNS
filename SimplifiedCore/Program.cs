using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using NLog; 
namespace SimplifiedCore
{

    

    public enum Command
    {
        AddConfigFile,
        RemoveConfigFile, 
        ShowConfigList,
        StartCore,
        StopCore,
        ShowConnectedEntities,
        Exit,
        None
        
        
    }

    public static class StringDefinitions
    {
        public const string DEFAULT_CONFIG_NAME = "Core.config"; 

        public static  string GetStrCommand(Command command)
        {
            switch (command)
            {
                case Command.AddConfigFile:
                    return "add path: Add a new config file to list. ";
                case Command.RemoveConfigFile:
                    return "rem number: Remove existing congfig file from list by number";
                case Command.ShowConfigList:
                    return "list: Show config files list. "; 
                case Command.ShowConnectedEntities:
                    return "show: Show connected entities. If the core aren't running, show nothing ";
                case Command.StartCore:
                    return "start [configFileNum]: Start the core. Specify a config file if needed. If the core already running, the command do nothing";
                case Command.StopCore:
                    return "stop: Stop core. Run it if the core are running.";
                case Command.Exit:
                    return "exit: Exit programm. If the core are running, it will be stoped.";
                case Command.None:
                    return "none: not a command. Shouldn't show.";
                
            }
            return "Imposible"; 

        }

        public const string CONFIG_FILE_DOESNT_EXISTS = "Config file doesn't exists.";

        public const string WRONG_COMMAND = "Was writed wrong command.";

        public const string WRONG_NUMBER_CONFIG = "Was specified the wrong number.";

        /// <summary>
        /// Require the config file name.
        /// </summary>
        public const string CONFIG_FILE_DELETED = " Config file {0} was deleted";


        public const string CORE_HASNT_STARTED = "The core hasn't started.";
        public const string CORE_ALREADY_STARTED = "The core already has started. ";
        public const string CORE_STARTED = "The core has started. ";
    }



    public static class Sayer
    {
        const int COUNT_OF_COMMANDS = 6;


        public static void ShowCommandList()
        {

            Console.WriteLine("Command list:");
            for (int i = 0; i < COUNT_OF_COMMANDS; i++)
            {
                Console.WriteLine("{0}) {1}", i + 1, StringDefinitions.GetStrCommand((Command)i));
            }
        }




        public static void SayHello()
        {
            Console.WriteLine("Hello! This is Core command line interface.");
        }



        public static Command GetCommand(out string arg)
        {
            Console.Write("Please, write a command: ");
            string[] enter = Console.ReadLine().Split();
            string command = enter[0];
            arg = ""; 
            switch (command)
            {
                case "add":
                    if (enter.Length != 2)
                        return Command.None;
                    arg = enter[1];
                    return Command.AddConfigFile;
                case "rem":
                    if (enter.Length != 2)
                        return Command.None;
                    arg = enter[1];
                    return Command.RemoveConfigFile;
                case "show":
                    if (enter.Length != 1)
                        return Command.None;
                    return Command.ShowConnectedEntities;
                case "list":
                    if (enter.Length != 1)
                        return Command.None;
                    return Command.ShowConfigList;
                case "start":
                    if (enter.Length > 2)
                        return Command.None;
                    if (enter.Length == 2)
                        arg = enter[1];
                    return Command.StartCore;
                case "stop":
                    if (enter.Length != 1)
                        return Command.None;
                    return Command.StopCore;
                case "exit":
                    if (enter.Length != 1)
                        return Command.None;
                    return Command.Exit; 

            }

            return Command.None;
        }

    }




    class Program
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static List<string> ConfigurationFiles = new List<string>();

        private static Core core = new Core();


        private static Task runningCoreTask;








        private static void DoCommand(Command command, string arg)
        {
            switch (command)
            {
                case Command.AddConfigFile:
                    if (!File.Exists(arg))
                    {
                        Console.WriteLine(StringDefinitions.CONFIG_FILE_DOESNT_EXISTS);
                        return;
                    }
                    ConfigurationFiles.Add(arg);
                    break;


                case Command.Exit:
                    if (core.IsRunning)
                        core.Stop(true, 60000);
                    System.Environment.Exit(ErrorCodes.ERROR_SUCCESS);
                    break;


                case Command.None:
                    Console.WriteLine(StringDefinitions.WRONG_COMMAND);
                    return;


                case Command.RemoveConfigFile:
                    int num;
                    if (!int.TryParse(arg, out num))
                    {
                        Console.WriteLine(StringDefinitions.WRONG_COMMAND);
                        return;
                    }


                    if (ConfigurationFiles.Count < num)
                    {
                        Console.WriteLine(StringDefinitions.WRONG_NUMBER_CONFIG);
                        return;
                    }

                    string name = Path.GetFileName(ConfigurationFiles[num]);
                    ConfigurationFiles.RemoveAt(num);
                    Console.WriteLine(StringDefinitions.CONFIG_FILE_DELETED, name);
                    return;


                case Command.ShowConfigList:
                    Console.WriteLine("Configuration files list: ");
                    foreach (var str in ConfigurationFiles)
                    {
                        Console.WriteLine(str);
                    }
                    return;


                case Command.ShowConnectedEntities:

                    if (!core.IsRunning)
                    {
                        Console.WriteLine(StringDefinitions.CORE_HASNT_STARTED);
                        return;
                    }

                    Console.WriteLine("Entities will written by it's mid.");

                    Console.WriteLine("Connections: ");
                    foreach(var x in core.Connections)
                    {
                        Console.WriteLine("{0} -> {1}", x.SenderMID, x.ReceiverMID);

                    }

                    Console.WriteLine("Waiting entities: ");
                    foreach (var x in core.WaitingReceivers)
                    {
                        Console.WriteLine("Receiver {0}", x.GetMID());
                    }

                    foreach (var x in core.WaitingSenders)
                    {
                        Console.WriteLine("Sender {0}", x.GetMID());
                    }

                    return;

                case Command.StartCore:
                    
                        if (core.IsRunning)
                        {
                            Console.WriteLine(StringDefinitions.CORE_ALREADY_STARTED);
                            return;

                        }
                        string fname;
                        if (arg == "")
                            fname = StringDefinitions.DEFAULT_CONFIG_NAME;
                        else
                            fname = arg; 
                        if (!File.Exists(fname))
                        {
                            Console.WriteLine(StringDefinitions.CONFIG_FILE_DOESNT_EXISTS);
                            return; 
                        }

                    runningCoreTask = new Task(new Action(() => core.Start(fname)));
                        core.Start(fname);
                        Console.WriteLine(StringDefinitions.CORE_STARTED);
                    
                        return;




                case Command.StopCore:
                    if (!core.IsRunning)
                    {
                        Console.WriteLine(StringDefinitions.CORE_HASNT_STARTED);
                        return;
                    }

                    core.Stop(true, 60000);

                    return;
            }

            throw new ArgumentException("Imposible command");
        }



        private static void Session()
        {
            Sayer.SayHello();
            Sayer.ShowCommandList();


            while (true)
            {
                string arg; 
                Command command = Sayer.GetCommand(out arg);
                DoCommand(command, arg);

            }

            
        }




        static int Main(string[] args)
        {
            Session();

            /*
            Core core = new Core();

            Console.WriteLine( "Core has just been initialized\r\nPlease, enter path to the config file:" );

            core.Start(Console.ReadLine());

            string command = Console.ReadLine().ToLower();

            while (command != "stop")
            {
                command = Console.ReadLine().ToLower();
            }

            core.Stop( true );

    */
            return ErrorCodes.ERROR_SUCCESS;
        }
    }
}