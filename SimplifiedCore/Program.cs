using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace SimplifiedCore
{
    class Program
    {
        static int Main(string[] args)
        {
            Core core = new Core();

            Console.WriteLine( "Core has just been initialized\r\nPlease, enter path to the config file:" );

            core.Start(Console.ReadLine());

            string command = Console.ReadLine().ToLower();

            while (command != "stop")
            {
                command = Console.ReadLine().ToLower();
            }

            core.Stop( true );

            return ErrorCodes.ERROR_SUCCESS;
        }
    }
}