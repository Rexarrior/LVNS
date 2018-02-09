namespace SimplifiedCore
{
    /// <summary>
    /// Constant string expression
    /// </summary>
    public static class Strings
    {
#if LINUX
        public const string UnmanagedDLLUsageFunctionsLibrary = "libdl.so";
#else
        public const string UnmanagedDLLUsageFunctionsLibrary = "kernel32.dll";
#endif
        public static class ConfigCurrentDirectory
        {
            public const string ConfigFile = "configfile";
            public const string Executable = "executable";
            public const string Path = "dir";
        }

        public const string ManagedDllClassName = "SimplifiedCoreExternalInterface.Methods";

        public static class DLLMethodsNames
        {
            public const string AcceptConnection = "AcceptConnection";
            public const string IsReceiver = "IsReceiver";
            public const string GetMID = "GetMID";
            public const string MatchMID = "MatchMID";
            public const string GetData = "GetData";
            public const string DispatchData = "DispatchData";
            public const string StopAccepting = "StopAccepting";
            public const string Terminate = "Terminate";
            public const string CloseConnection = "CloseConnection";
        }

        
    }
     
    /// <summary>
    /// Success status codes. Basically, errors.
    /// </summary>
    public static class ErrorCodes
    {
        public const int ERROR_SUCCESS = 0;

        // say, when we call Stop() and didn't call Start() before
        public const int NOT_RUNNING = 0x0010;
        public const int ALREADY_RUNNING = 0x0011;
        public const int PREVIOUS_RUN_DOESNT_TERMINATE = 0x0012;
        public const int PENDING_TERMINATION = 0x0013;
        public const int TERMINATION_NOT_REQUESTED = 0x0014;

        public const int UNMANAGED_DLL = 0x0015;
        public const int MANAGED_DLL = 0x0016;

        public const int DLL_NOT_LOADED = 0x0017;
        public const int DLL_ALREADY_LOADED = 0x0018;
        public const int DLL_COULDNT_BE_LOADED = 0x0019;

        // error codes from 0x0100 to 0x01FF
        // are used for file errors
        public const int FILE_NOT_FOUND  = 0x0100;
        public const int FILE_COULDNT_BE_OPEN = 0x0101; // Maybe expand this to ACCESS_DENIED, ALREADY_IN_USE,..
        public const int DLL_DAMAGED = 0x0102;
        public const int PATH_NOT_FOUND = 0x0103;
    }

   

    static class LogTraceMessages
    {
        #region INVOKING
        /// <summary>
        /// Require the method's name or signature as argument
        /// </summary>
        public static string METHOD_INVOKED = "The Method {0} has invoked.";
        public static string CONSTRUCTOR_INVOKED = "The constructor has invoked.";

        /// <summary>
        /// Require the method's name or signature and an reason for finishing the method
        /// </summary>
        public static string METHOD_FINISHED = "The Method {0} finished because {1}";
        #endregion

        #region TRANSFER
        public static string TRANSFER_END = "The Transfer end.";
        public static string TRANSFER_LOOP_DISPOSED = "TransferLoop token has disposed. ";
        public static string TRANSFER_STARTING = " Starting the transfer... ";
        public static string TRANSFER_STARTED_SUCCESFULL = "The transfer has started succesfull.";
        public static string TRANSFER_LOOP_CANCELED = "TransferLoop token has canceled";
        public static string TERMINATED = "The termination ended.";
        #endregion




        #region EXTERNAL INTERFACE
        public static string EXTERNAL_INTERFACE_TERMINATING = "The external Interface is terminating. Method will be finished.";
        public static string EXTERNAL_INTERFACE_CREATING = "Creating the external Interface... ";
        public static string EXTERNAL_INTERFACE_THREAD_CREATING = "Creating the external Interface thread... ";
        public static string EXTERNAL_INTERFACE_CREATED = "The external interface has created. ";
        public static string EXTERNAL_INTERFACE_STOPING = "The external interface is stoping...  ";
        public static string EXTERNAL_INTERFACE_STOPED = "The external interface has stoped  ";
        public static string EXTERNAL_INTERFACE_ALL_STOPED = "The Eexternal interface has stoped  ";
        public static string EXTERNAL_INTERFACE_DISPOSED = "The external interface has disposed. ";
        public static string EXTERNAL_INTERFACE_ALL_DISPOSED = "All external interfaces have disposed. ";
        #endregion


        #region CONNECTION
        public static string CONNECTION_ACCEPT_LOOP_DISPOSED = "AcceptConnectionsLoop has disposed.";
        public static string CONNECTION_CLOSING = "Closing the connection...  ";
        public static string CONNECTION_ALL_CLOSED = "All connections have closed.";
        public static string CONNECTION_CLOSED = "The connection has closed. ";
        public static string CONNECTION_ACCEPTING = "A new connection accepting... ";
        public static string CONNECTION_BOTH__CLOSED = "The both connections have closed.";
        #endregion


        #region RECEIVER
        public static string RECEIVER_CONNECTION_STOPING = "The receiver's connection is closing...  ";
        public static string RECEIVER_CONNECTION_STOPED = "The receiver's connection is closed.  ";
        public static string RECEIVER_CONNECTIONS_ALL_STOPED = "All receiver's connection is closed.  ";
        public static string RECEIVER_ADDING = "Adding a new receiver... ";
        public static string RECEIVER_MATCHING_SENDER_FOUNDED = "The matching sender has founded.Adding.";
        public static string RECEIVER_MATCHING_SENDER_NOT_FOUNDED = "Some matching senders have not founded";
        #endregion


        #region SENDER
        public static string SENDER_CONNECTION_STOPING = "The Sender's connection is closing...";
        public static string SENDER_CONNECTION_STOPED = "The Sender's connection is closed.";
        public static string SENDER_CONNECTIONS_ALL_STOPED = "All sender's connection is closed.";
        public static string SENDER_ADDING = "Adding a new sender... ";
        public static string SENDER_MATCHING_RECEIVER_NOT_FOUNDED = "Some matching receivers have not founded";
        public static string SENDER_MATCHING_RECEIVER_FOUNDED = "The matching receiver has founded.Adding.";
        #endregion


        #region CORE
        public static string CORE_WAIT_TERMINATION = "Wait for the previous termination ";

        /// <summary>
        /// Require 'dir' string of the config file as argument
        /// </summary>
        public static string CORE_CURRENT_DIR_IDENTIFING = "The current directory will identify by {0}";
        public static string CORE_CLEANUP_THREAD_CREATING = "Creating cleandUpThread...";
        public static string CORE_CLEANUP_THREAD_CREATED = "CleandUpThread has started.";
        public static string CORE_STOPED = "The Core stoped. ";
#endregion


        #region CONFIG FILE
        /// <summary>
        /// Require the  config file name as argument
        /// </summary>
        public static string CONFIG_FILE_OPENING = "The config file {0} is opening... ";

        /// <summary>
        /// Require the config file name as argument
        /// </summary>
        public static string CONFIG_FILE_READ_DIR = "Read the current directory from the config file {0} ";

        public static string CONFIG_FILE_REGISTERING_INTERFACES = "Reading the config and registering the interfaces...";
        #endregion


        #region TOKEN
        public static string TOKEN_CREATED = "{0} token has created.";
        public static string TOKEN_CANCELED = "{0} token has canceled.";
        #endregion


        #region LIBRARY
        /// <summary>
        /// Require   library file name as argument
        /// </summary>
        public static string LIBRARY_FILE_OPENED = "{0} library opened.";

        /// <summary>
        /// Require library file name and library type as argument
        /// </summary>
        public static string LIBRARY_TYPE_IDENTIFIED = "Library {0} type has identified: {1}";

        /// <summary>
        /// Require library file name as argument
        /// </summary>
        public static string LIBRARY_TYPE_REQUESTED = "Library {0} type was requested.";

        /// <summary>
        /// Require library file name and GetDLLType returned value as argument
        /// </summary>
        public static string LIBRARY_TYPE_RESPONSED = "'Get type' method for the library {0} returned the value: {1}";
        
        /// <summary>
        /// Require library file name as argument
        /// </summary>
        public static string LIBRARY_LOADING = "Library {0} is loading...";

        /// <summary>
        /// Require library file name as argument
        /// </summary>
        public static string LIBRARY_LOADED = "Library {0} has loaded.";

        /// <summary>
        /// Require library file name as argument
        /// </summary>
        public static string LIBRARY_GETTING_DELEGATES = "Getting delegates from the library {0}";

        /// <summary>
        /// Require library file name as argument
        /// </summary>
        public static string LIBRARY_DELEGATES_RECEIVED = "Delegates from the library {0} received successfull.";

        /// <summary>
        /// Require library file name as argument
        /// </summary>
        public static string LIBRARY_UNLOADING = " {0} Library unloading...";

        /// <summary>
        /// Require library file name as argument
        /// </summary>
        public static string LIBRARY_UNLOADED= " {0} library unloaded successfull.";
        
        /// <summary>
        /// Require method name or signature as argument
        /// </summary>
        public static string LIBRARY_METHOD_USE = " Use the libraries function {0} ";
        #endregion


        public static string SYNK_ROOT_LOCKED = "_{0}SynkRoot has loked";
        public static string WAITING_FOR_TERMINATION = "We are waiting for termination...";


    }




    static class LogInfoMessages
    {

        public static string CORE_STARTED = "The Core has started.";
       
        /// <summary>
        /// Requaire current directoruy string as argument
        /// </summary>
        public static string CORE_CURRENT_DIR_SETED = "The current directory has seted to  {0}. ";
        
        /// <summary>
        /// Require currentString as argument
        /// </summary>
        public static string EXTERNAL_INTERFACE_REGISTERED = "The interface defined by currentString {0} has registered.";

        /// <summary>
        /// Require config file name as argument
        /// </summary>
        public static string CONFIG_FILE_CLOSED = "The config file {0} has been procesed succesfull and now closed.";

        public static string TRANSFER_CONNECTION_OPENED = "New transfer connection has opened.";

        public static string RECEIVER_EXPECTED_REGISTRED = "The expected receiver was registered. New  Transfer connection has opened.";

        public static string RECEIVER_REGISTRED = "A new receiver was registred and now is waiting for smem senders.";

        public static string SENDER_EXPECTED_REGISTRED = "The expected sender was registered. New transfer connection has opened.";

        public static string SENDER_REGISTRED = "A new sender was registred and now is waiting for some receivers.";

        public static string CORE_STOPING = "The Core is now stoping....";

        public static string CORE_STOPED = "The Core has stoped.";
    }






    static class OtherConsts
    {
        public  const int RTLD_NOW = 2;
    }
}
