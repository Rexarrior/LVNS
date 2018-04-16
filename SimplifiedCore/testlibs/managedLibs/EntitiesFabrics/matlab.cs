using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


using NLog;


namespace EntitiesFabrics.Analiziers
{
    public static class MatlabWrapper
    {
        private static bool _isInitialized = false;

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static MLApp.MLApp _app;

        private static string _dirr; 

        private static Dictionary<string, string> _scripts;


        public static bool AddScript(string scriptName, string scriptPath)
        {
            try
            {
                if (!_isInitialized)
                    _init();

                if (!File.Exists(scriptPath))
                    throw new ArgumentException(String.Format("Wrong path to the script: ", scriptPath));

                Random rnd = new Random();
                string temp_name = "temp_" + scriptName + "_" + rnd.Next();

                File.Move(scriptPath, _dirr + temp_name);

                _scripts.Add(scriptName, scriptPath);
            }
            catch (Exception e)
            {
                logger.Error("ERROR!In time of adding script the next error has happened:{0}", e.Message);
                return false;
            }
            return true;

        }


        private static void _init()
        {
            _app = new MLApp.MLApp();
            _app.Visible = 0;
            _scripts = new Dictionary<string, string>(); 

            _dirr = Directory.GetCurrentDirectory() + "\\runtimeScripts\\";
            if (!Directory.Exists(_dirr))
                Directory.CreateDirectory(_dirr);
            _app.Execute(_dirr );


            logger.Info("Matlab wrapper has initialized");

            _isInitialized = true;
        }



        #region invoke

        public static bool Invoke(string scriptName, out object result,  object arg1)
        {
            if (!_isInitialized)
                _init();
            if (!_scripts.ContainsKey(scriptName))
            {
                logger.Error("ERROR! In time of invoking the {0} script  the next error happened: script non added");
                result = null;
                return false;
            }
            _app.Feval(_scripts[scriptName],1, out result, arg1);
            return true;

        }




        public static bool Invoke(string scriptName, out object result, object arg1, object arg2)
        {
            if (!_isInitialized)
                _init();
            if (!_scripts.ContainsKey(scriptName))
            {
                logger.Error("ERROR! In time of invoking the {0} script  the next error happened: script non added");
                result = null;
                return false;
            }
            _app.Feval(_scripts[scriptName], 2, out result, arg1, arg2);
            return true;

        }
        



        public static bool Invoke(string scriptName, out object result, object arg1, object arg2, object arg3)
        {
            if (!_isInitialized)
                _init();
            if (!_scripts.ContainsKey(scriptName))
            {
                logger.Error("ERROR! In time of invoking the {0} script  the next error happened: script non added");
                result = null;
                return false;
            }
            _app.Feval(_scripts[scriptName], 3, out result, arg1, arg2, arg3);
            return true;

        }



        public static bool Invoke(string scriptName, out object result, object arg1, object arg2, object arg3, object arg4)
        {
            if (!_isInitialized)
                _init();
            if (!_scripts.ContainsKey(scriptName))
            {
                logger.Error("ERROR! In time of invoking the {0} script  the next error happened: script non added");
                result = null;
                return false;
            }
            _app.Feval(_scripts[scriptName], 4, out result, arg1, arg2, arg3, arg4);
            return true;

        }




        public static bool Invoke(string scriptName, out object result, object arg1, object arg2, object arg3, object arg4, object arg5)
        {
            if (!_isInitialized)
                _init();
            if (!_scripts.ContainsKey(scriptName))
            {
                logger.Error("ERROR! In time of invoking the {0} script  the next error happened: script non added");
                result = null;
                return false;
            }
            _app.Feval(_scripts[scriptName], 4, out result, arg1, arg2, arg3, arg4, arg5);
            return true;

        }




        public static bool Invoke(string scriptName, out object result, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
        {
            if (!_isInitialized)
                _init();
            if (!_scripts.ContainsKey(scriptName))
            {
                logger.Error("ERROR! In time of invoking the {0} script  the next error happened: script non added");
                result = null;
                return false;
            }
            _app.Feval(_scripts[scriptName], 4, out result, arg1, arg2, arg3, arg4, arg5, arg6);
            return true;

        }




        public static bool Invoke(string scriptName, out object result, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
        {
            if (!_isInitialized)
                _init();
            if (!_scripts.ContainsKey(scriptName))
            {
                logger.Error("ERROR! In time of invoking the {0} script  the next error happened: script non added");
                result = null;
                return false;
            }
            _app.Feval(_scripts[scriptName], 4, out result, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            return true;

        }

        #endregion



        public static void Dispose()
        {
            _app.Quit();
            foreach (var script in _scripts.Values)
            {
                File.Delete(script);
            }
            Directory.Delete(_dirr);

        }

    }
}
