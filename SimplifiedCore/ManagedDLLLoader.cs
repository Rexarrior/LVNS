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

    /// <summary>
    /// DLL loader class for processing managed dll.
    /// Notes:
    /// We want to properly unload
    /// DLLs when closing an interface,
    /// so every IDLLLoader must do this
    /// in UnloadDLL function.
    ///   
    /// But with managed DLLs, that are
    /// loaded as assemblies, it's a little
    /// tricky: an assembly cannot be unloaded
    /// manually - it gets removed automatically
    /// when the AppDomain they were loaded to,
    /// gets unloaded.
    ///
    /// So, we create an app domain, so to say,
    /// "local" for this particular DLLLoader.
    /// In this case, when UnloadDLL is called,
    /// we can just use AppDomain.Unload(_LocalDomain )
    /// </summary>
    class ManagedDLLLoader : IDLLLoader
    {


        #region FIELDS
        //private AppDomain _LocalDomain;

        /// <summary>
        /// Imagine of assembly of managed library
        /// </summary>
        private Assembly _DllAssembly;
        
        ///<summary>
        /// The class, specified in the dll,
        /// that contains needed function
        /// </summary>
        private Type _DllMainClass;


        /// <summary>
        /// Full path to the DLL 
        /// </summary>
        private string _DLLpath;

        /// <summary>
        /// Dll name is the part of dll path whish is path relative to the current folder
        /// </summary>
        private string _DLLname;


        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();


        #endregion



        #region PROPERTIES
        public string DLLpath { get => _DLLpath;  }

        public string DLLname { get => _DLLname;  }
        #endregion



        #region METHODS

        /// <summary>
        /// Load library by it's path and linking this loader with it.
        /// </summary>
        /// <param name="dllPath"></param>
        /// <returns></returns>
        public int LoadDLL(string dllPath)
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            string dllName = Path.GetFileName(dllPath);

            #region ASSERTATIONS
            if (_DllAssembly != null)
            {
                logger.Error("{0}. Attempt to load dll twice in one instance of DllLoader. ",
                    ErrorCodes.DLL_ALREADY_LOADED);
                return ErrorCodes.DLL_ALREADY_LOADED;
            }

            if (!File.Exists(dllPath))
            {
                logger.Error("{0}. File of dllPath not found",
                     ErrorCodes.FILE_NOT_FOUND);
                return ErrorCodes.FILE_NOT_FOUND;
            }
            #endregion

            _DLLpath = Path.GetFullPath(dllPath);
            _DLLname = dllName;


            //System.Security.Policy.Evidence evd = new System.Security.Policy.Evidence();
            /*
            _LocalDomain = AppDomain.CreateDomain(
                dllPath,
                evd,
                Path.GetDirectoryName( dllPath ),
                "",
                false
                );
                */
            _DllAssembly =
                Assembly.LoadFile( dllPath );
#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_LOADING,
                dllName);
#endif
            if (_DllAssembly == null)
            {
                logger.Error("{0}. Dll couldn't be loaded, ",
                                        ErrorCodes.DLL_COULDNT_BE_LOADED);
                return ErrorCodes.DLL_COULDNT_BE_LOADED;
            }

            _DllMainClass = _DllAssembly.GetType( Strings.ManagedDllClassName );
#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_LOADED,
                dllName);
#endif
            return ErrorCodes.ERROR_SUCCESS;
        }


        #region Getters for the library methods
        /// <summary>
        /// Get delegate for "AcceptConnection" method of linking library
        /// </summary>
        /// <returns>Delegate for extracted "AcceptConnection method"</returns>
        public AcceptConnection_Delegate GetAcceptConnectionDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return
                (AcceptConnection_Delegate)
                _DllMainClass.GetMethod(Strings.DLLMethodsNames.AcceptConnection)
                .CreateDelegate(typeof(AcceptConnection_Delegate));
        }



        /// <summary>
        /// Get delegate for "CloseConnection" method of linking library
        /// </summary>
        /// <returns>Delegate for extracted "CloseConnection" method</returns>
        public CloseConnection_Delegate GetCloseConnectionDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return
                (CloseConnection_Delegate)
                _DllMainClass.GetMethod(Strings.DLLMethodsNames.CloseConnection)
                .CreateDelegate(typeof(CloseConnection_Delegate));
        }



        /// <summary>
        /// Get delegate for "DispatchData" method of linking library
        /// </summary>
        /// <returns>Delegate for extracted "DispatchData" method</returns>
        public DispatchData_Delegate GetDispatchDataDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return
                (DispatchData_Delegate)
                _DllMainClass.GetMethod(Strings.DLLMethodsNames.DispatchData)
                .CreateDelegate(typeof(DispatchData_Delegate));
        }



        /// <summary>
        /// Get delegate for "GetData" method of linking library
        /// </summary>
        /// <returns>Delegate for extracted "GetData" method</returns>
        public GetData_Delegate GetGetDataDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return
                (GetData_Delegate)
                _DllMainClass.GetMethod(Strings.DLLMethodsNames.GetData)
                .CreateDelegate(typeof(GetData_Delegate));
        }




        /// <summary>
        /// Get delegate for "GetMID" method of linking library
        /// </summary>
        /// <returns>Delegate for extracted "GetMID" method</returns>
        public GetMID_Delegate GetGetMIDDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return
                (GetMID_Delegate)
                _DllMainClass.GetMethod(Strings.DLLMethodsNames.GetMID)
                .CreateDelegate(typeof(GetMID_Delegate));
        }



        /// <summary>
        /// Get delegate for "IsReceiver" method of linking library
        /// </summary>
        /// <returns>Delegate for extracted "IsReceiver" method</returns>
        public IsReceiver_Delegate GetIsReceiverDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return
                (IsReceiver_Delegate)
                _DllMainClass.GetMethod(Strings.DLLMethodsNames.IsReceiver)
                .CreateDelegate(typeof(IsReceiver_Delegate));
        }




        /// <summary>
        /// Get delegate for "MatchMID" method of linking library
        /// </summary>
        /// <returns>Delegate for extracted "MatchMID" method</returns>
        public MatchMID_Delegate GetMatchMIDDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return
                (MatchMID_Delegate)
                _DllMainClass.GetMethod(Strings.DLLMethodsNames.MatchMID)
                .CreateDelegate(typeof(MatchMID_Delegate));
        }




        /// <summary>
        /// Get delegate for "StopAccepting" method of linking library
        /// </summary>
        /// <returns>Delegate for extracted "StopAccepting" method</returns>
        public StopAccepting_Delegate GetStopAcceptingDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return
                (StopAccepting_Delegate)
                _DllMainClass.GetMethod(Strings.DLLMethodsNames.StopAccepting)
                .CreateDelegate(typeof(StopAccepting_Delegate));
        }




        /// <summary>
        /// Get delegate for "Terminate" method of linking library
        /// </summary>
        /// <returns>Delegate for extracted "Terminate" method</returns>
        public Terminate_Delegate GetTerminateDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return
                (Terminate_Delegate)
                _DllMainClass.GetMethod(Strings.DLLMethodsNames.Terminate)
                .CreateDelegate(typeof(Terminate_Delegate));
        }
        #endregion


        /// <summary>
        /// Unload linking library
        /// </summary>
        /// <returns>Success status as ErrorCode enum </returns>
        public int UnloadDLL()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            if (_DllAssembly == null)
            {
                logger.Error("{0}. Attempt to unload dll before it was load  ",
                      ErrorCodes.DLL_NOT_LOADED);

        
                return ErrorCodes.DLL_NOT_LOADED;
            }

            _DllAssembly = null;
#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_UNLOADED,
                _DLLname );
#endif
            _DLLpath = "Undefined";
            _DLLname = "Undefined";
            return ErrorCodes.ERROR_SUCCESS;
        }

        #endregion



        #region CONSTRUCTORS

        /// <summary>
        /// Create instance of ManagedDLLLoader without linking with any library
        /// </summary>
        public ManagedDLLLoader()
        {
#if DEBUG
            logger.Trace(LogTraceMessages.CONSTRUCTOR_INVOKED);
#endif
            _DllAssembly = null;
            _DLLpath = "Undefined";
            _DLLname = "Undefined";
        }
        #endregion

    }
}
