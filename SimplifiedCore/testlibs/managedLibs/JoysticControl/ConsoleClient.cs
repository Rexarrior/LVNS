using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesFabrics.ConsoleClient;
namespace JoysticControl
{
    static class ConsoleClient
    {
        private static bool _isInitialized = false;


        private static ConsoleInstance _consoleInstance;

        private static void _checkInit()
        {
            if (!_isInitialized)
                _init();
        }


        public static void Write(string s)
        {
            _checkInit();

            _consoleInstance.Write(s); 
        }


        private static void _init()
        {

            _consoleInstance = new ConsoleInstance("JoysticControlStream");

            _isInitialized = true;
        }
    }
}
