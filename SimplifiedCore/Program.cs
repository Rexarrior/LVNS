using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using NLog; 
namespace SimplifiedCore
{
    class Program
    {
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
        {
#if DEBUG
            logger.Trace("Program has started.");
#endif

            Core core = new Core();
#if DEBUG
            logger.Trace(" Core has created. ");
#endif
            Console.WriteLine( "Core has just been initialized\r\nPlease, enter path to the config file:" );

            core.Start(Console.ReadLine());
#if DEBUG
            logger.Trace(" Core has started. ");
#endif
            string command = Console.ReadLine().ToLower();

#if DEBUG
            logger.Trace(" Waiting for stop command.");
#endif
            while (command != "stop")
            {
                command = Console.ReadLine().ToLower();
            }

            core.Stop( true );
#if DEBUG
            logger.Trace(" Core has Stoped. ");
#endif
#if DEBUG
            logger.Trace(" Program ended. ");
#endif
            return ErrorCodes.ERROR_SUCCESS;
        }
    }
}