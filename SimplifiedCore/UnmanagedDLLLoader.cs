using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

using System.IO;
using NLog; 

namespace SimplifiedCore
{
    /// <summary>
    /// DLL loader class for processing unmanaged library.
    /// </summary>
    class UnmanagedDLLLoader : IDLLLoader
    {
        #region Native OS methods
#if LINUX
        /// <summary>
        /// The function that is used to load the library.
        /// </summary>
        /// <param name="dllPath">Path to library</param>
        /// <returns></returns>
        [DllImport(Strings.UnmanagedDLLUsageFunctionsLibrary)]
        private static extern IntPtr LoadLibrary(string dllPath, int flag);
#else
        /// <summary>
        /// The function that is used to load the library.
        /// </summary>
        /// <param name="dllPath">Path to library</param>
        /// <returns></returns>
        [DllImport(Strings.UnmanagedDLLUsageFunctionsLibrary)]
        private static extern IntPtr LoadLibrary(string dllPath);
#endif
        /// <summary>
        /// The function that extract the method from the library.
        /// </summary>
        /// <param name="dllHandle">Handle of already loaded library that contains the method</param>
        /// <param name="procName">Signature of method to execute</param>
        /// <returns></returns>
        [DllImport(Strings.UnmanagedDLLUsageFunctionsLibrary, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr dllHandle, string procName);

        /// <summary>
        /// The function that unload the library.
        /// </summary>
        /// <param name="dllHandle">Handle of the library.</param>
        /// <returns></returns>
        [DllImport(Strings.UnmanagedDLLUsageFunctionsLibrary, SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr dllHandle);
        #endregion


        /// <summary>
        /// Handle of the library
        /// </summary>
        private IntPtr _DllHandle;

        #region pointers of  methods of the library
        /// <summary>
        /// Pointer for executed method for "CloseConnection"
        /// </summary>
        private IntPtr _CloseConnectionPtr;

        /// <summary>
        /// Pointer for executed method for "AcceptConnection"
        /// </summary>
        private IntPtr _AcceptConnectionPtr;

        /// <summary>
        /// Pointer for executed method for "StopAccepting"
        /// </summary>
        private IntPtr _StopAcceptingPtr;

        /// <summary>
        /// Pointer for executed method for "Terminate"
        /// </summary>
        private IntPtr _TerminatePtr;

        /// <summary>
        /// Pointer for executed method for "IsReceiver"
        /// </summary>
        private IntPtr _IsReceiverPtr;

        /// <summary>
        /// Pointer for executed method for "GetData"
        /// </summary>
        private IntPtr _GetDataPtr;

        /// <summary>
        /// Pointer for executed method for "DispatchData"
        /// </summary>
        private IntPtr _DispatchDataPtr;

        /// <summary>
        /// Pointer for executed method for "GetMID"
        /// </summary>
        private IntPtr _GetMIDPtr;
        
        /// <summary>
        /// Pointer for executed method for "MatchMID"
        /// </summary>
        private IntPtr _MatchMIDPtr;
        #endregion

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();




        /// <summary>
        /// Created instanse of UnmanagedDLLLoader without linking with any library
        /// </summary>
        public UnmanagedDLLLoader()
        {
#if DEBUG
            logger.Trace(LogTraceMessages.CONSTRUCTOR_INVOKED);
#endif

            _DllHandle = IntPtr.Zero;
        }



        /// <summary>
        ///  Load the library
        /// </summary>
        /// <param name="dllPath">Path to library</param>
        /// <returns>Success status code by enum ErrorCodes</returns>
        public int LoadDLL(String dllPath)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            string dllName = Path.GetFileName(dllPath);

#region Assertations
            if (_DllHandle != IntPtr.Zero)
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


#region load library
#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_LOADING,
                dllName);
#endif

#if LINUX
            _DllHandle = LoadLibrary(dllPath, OtherConsts.RTLD_NOW );
#else
            _DllHandle = LoadLibrary(dllPath);
#endif
            if (_DllHandle == IntPtr.Zero)
            {
                logger.Error("{0}. Dll couldn't be loaded, ",
                                     ErrorCodes.DLL_COULDNT_BE_LOADED);
                return ErrorCodes.DLL_COULDNT_BE_LOADED;
            }
#endregion


#region Loading functions from the unmanaged DLL 
            
#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_LOADED,
                dllName);
            logger.Trace(LogTraceMessages.LIBRARY_GETTING_DELEGATES,
                dllName);
#endif



            _GetDataPtr = GetProcAddress(_DllHandle, Strings.DLLMethodsNames.GetData);
            _DispatchDataPtr = GetProcAddress(_DllHandle, Strings.DLLMethodsNames.DispatchData);
            _GetMIDPtr = GetProcAddress(_DllHandle, Strings.DLLMethodsNames.GetMID);
            _MatchMIDPtr = GetProcAddress(_DllHandle, Strings.DLLMethodsNames.MatchMID);
            _AcceptConnectionPtr = GetProcAddress(_DllHandle, Strings.DLLMethodsNames.AcceptConnection);
            _StopAcceptingPtr = GetProcAddress( _DllHandle, Strings.DLLMethodsNames.StopAccepting );
            _TerminatePtr = GetProcAddress(_DllHandle, Strings.DLLMethodsNames.Terminate);
            _CloseConnectionPtr = GetProcAddress(_DllHandle, Strings.DLLMethodsNames.CloseConnection);
#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_DELEGATES_RECEIVED,
                dllName);
#endif
#endregion


            return ErrorCodes.ERROR_SUCCESS;
        }



        #region getters for library methods
        /// <summary>
        /// Get delegate for executed method "AcceptConnection"
        /// </summary>
        /// <returns>Delegate for "AcceptConnection"</returns>
        public AcceptConnection_Delegate GetAcceptConnectionDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return (AcceptConnection_Delegate)
                Marshal.GetDelegateForFunctionPointer(_AcceptConnectionPtr, typeof(AcceptConnection_Delegate));
        }



        /// <summary>
        /// Get delegate for executed method "IsReceiver"
        /// </summary>
        /// <returns>Delegate for "IsReceiver"</returns>
        public IsReceiver_Delegate GetIsReceiverDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return (IsReceiver_Delegate)
                Marshal.GetDelegateForFunctionPointer(_IsReceiverPtr, typeof(IsReceiver_Delegate));
        }



        /// <summary>
        /// Get delegate for executed method "GetMID"
        /// </summary>
        /// <returns>Delegate for "Get MID"</returns>
        public GetMID_Delegate GetGetMIDDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return (GetMID_Delegate)
                Marshal.GetDelegateForFunctionPointer(_GetMIDPtr, typeof(GetMID_Delegate));
        }



        /// <summary>
        /// Get delegate for executed method "MatchMID"
        /// </summary>
        /// <returns>Delegate for "MatchMID"</returns>
        public MatchMID_Delegate GetMatchMIDDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return (MatchMID_Delegate)
                Marshal.GetDelegateForFunctionPointer(_MatchMIDPtr, typeof(MatchMID_Delegate));
        }



        /// <summary>
        /// Get delegate for executed method "GetData"
        /// </summary>
        /// <returns>Delegate for "GetData"</returns>
        public GetData_Delegate GetGetDataDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return (GetData_Delegate)
                Marshal.GetDelegateForFunctionPointer(_GetDataPtr, typeof(GetData_Delegate));
        }



        /// <summary>
        /// Get delegate for executed method "DispatchData"
        /// </summary>
        /// <returns>Delegate for "DispatchData"</returns>
        public DispatchData_Delegate GetDispatchDataDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return (DispatchData_Delegate)
                Marshal.GetDelegateForFunctionPointer(_DispatchDataPtr, typeof(DispatchData_Delegate));
        }



        /// <summary>
        /// Get delegate for executed method "StopAccepting"
        /// </summary>
        /// <returns>Delegate for "StopAccepting"</returns>
        public StopAccepting_Delegate GetStopAcceptingDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return (StopAccepting_Delegate)
                Marshal.GetDelegateForFunctionPointer( _StopAcceptingPtr, typeof(StopAccepting_Delegate) );
        }



        /// <summary>
        /// Get delegate for executed method "Terminate"
        /// </summary>
        /// <returns>Delegate for "Terminate"</returns>
        public Terminate_Delegate GetTerminateDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return (Terminate_Delegate)
                Marshal.GetDelegateForFunctionPointer(_TerminatePtr, typeof(Terminate_Delegate));
        }



        /// <summary>
        /// Get delegate for executed method "CloseConnection"
        /// </summary>
        /// <returns>Delegate for "CloseConnection"</returns>
        public CloseConnection_Delegate GetCloseConnectionDelegate()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return (CloseConnection_Delegate)
                Marshal.GetDelegateForFunctionPointer(_CloseConnectionPtr, typeof(CloseConnection_Delegate));
        }
        #endregion



        /// <summary>
        /// Unload linking library
        /// </summary>
        /// <returns>Success status code as ErrorCode enum</returns>
        public int UnloadDLL()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());                        
#endif

            if (_DllHandle == IntPtr.Zero)
            {
                logger.Error("{0}. Attempt to unload dll before it was load  ",
                      ErrorCodes.DLL_NOT_LOADED);
                return ErrorCodes.DLL_NOT_LOADED;
            }


            FreeLibrary(_DllHandle);
#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_UNLOADED, 
                "The");
#endif
            return ErrorCodes.ERROR_SUCCESS;
        }
    }
}
