using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog; 
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;

namespace SimplifiedCore
{
    #region DESCRIPTION
    /*
     * ExternalInterface class:
     * 
     * Represents methods for controlling
     * an interface that communicates with
     * external entities (we don't call them
     * "devices", because this would be too
     * narrow for the whole system) by some

     * particular means it supports, say,
     * sockets or USB-connection or filesystem -
     * whatever.
     * 
     * The very interface is supposed to be
     * a Dynamic-Link Library (DLL) that exports
     * a set of functions, essential for us
     * to work.
     * 
     * NEEDTOKNOW: if you are implementing your
     * own .dll, you are gonna make all the functions
     * thread-safe YOURSELF.
     * 
     * _Delegate suffix is for types
     * Delegate suffix is for the delegates to be stored and passed
     * Routine suffix is for the delegates to be invoked
     */
    /*
     * Functions, that a DLL must export
     * to be used by the Core
     * 
     * The IDs passed to these functions are the IDs
     * that the DLL uses to identify entities.
     * So, Core knows absolutely nothing about those
     * numbers - our task is only to save them in
     * corresponding classes and pass inside the DLL
     * when we call the functions.
     * First, we get this ID from AcceptConnection() -
     * its returned value.
     */

    /*
     * First line in those eight commentaries below
     * specifies, how those functions must be declared
     * in the DLL that's gonna be loaded and used.
     * 
     * And the declarations are given not in "C style"
     * or "C++ style", but they are pretty self-describing:
     * 
     * Type Name( Parameters )
     * 
     * uint32 type, as an example, states an unsigned 32-bit
     * integer value.
     * 
     * But one hint really must be given:
     * (type1 -> type2) means, that in the
     * essence, the type is supposed to be
     * type1, but due to some incompatibilities
     * or whatever, type2 serves as an
     * "intermediary".
     */
     
    /*
     * uint32 AcceptConnection()
     * 
     * The function is used to accept
     * those who connect via this interface.
     * It returns the ID that is described
     * in the last paragraph of the previous
     * commentary
     */ 
    #endregion

    #region DELEGATES
    /// <summary>
    /// Delegate for "AcceptConnection" library method.
    /// 
    ///  The function is used to accept
    ///  those who connect via this interface.
    /// </summary>
    /// <returns>ID</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt32 AcceptConnection_Delegate();

    /*
     * (bool -> uint32) IsReceiver( uint32 id )
     * 
     * When someone connects to an interface, we
     * don't know if it's a Sender or a Receiver,
     * so we call this function to find out     
     * type of this function is actually bool, not int, but dll's bool
     * and C#'s bool seem to be incompatible, so we use UInt32
     */

    /// <summary>
    /// Delegate for "IsReceiver" library method.
    /// 
    /// When someone connects to an interface, we
    /// don't know if it's a Sender or a Receiver,
    /// so we call this function to find out.
    /// </summary>
    /// <param name="id">Checking ID</param>
    /// <returns>Is id's entity receiver. Bool represented by UInt32 </returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt32 IsReceiver_Delegate( UInt32 id );

    

    /*
     * void GetMID( uint32 id, char * buffer )
     * 
     * Description of what it is for is given
     * in the description of Core
     * 
     * Managed DLLs should fill matchIDBuffer using .Append() function
     */

    /// <summary>
    ///Delegate for "GetMid" library method. 
    /// </summary>
    /// <param name="id">id of entity</param>
    /// <param name="matchIDBuffer">buffer to store MatchID</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GetMID_Delegate( UInt32 id, StringBuilder matchIDBuffer );


    /*
     * (bool -> uint32) MatchMID( uint32 id, char * matchID )
     * 
     * Look at Core description to find out why it is needed     
     * type of this function is actually bool, not int, but dll's bool
     * and C#'s bool seem to be incompatible, so we use UInt32
    */
    /// <summary>
    /// Delegate for "MatchMid" library method.
    /// </summary>
    /// <param name="id">ID of entity</param>
    /// <param name="matchID">MID to match with MID of entity</param>
    /// <returns>is MIds equal. Bool represented by UInt32</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt32 MatchMID_Delegate( UInt32 id, string matchID );


    /*
     * void GetData( uint32 id, byte * buffer )
     * 
     * When we call this function, DLL receives data
     * from the sender, specified by "id" parameter
     * and stores it in "dataBuffer"
     */
    
    /// <summary>
    /// Delegate for "GetData" library method.
    /// When we call this function, DLL receives data
    /// from the sender, specified by "ID" parameter
    /// and stores it in "dataBuffer"
    /// </summary>
    /// <param name="id">ID of entity</param>
    /// <param name="dataBuffer">Buffer to store data</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GetData_Delegate( UInt32 id, byte[] dataBuffer );


    /*
     * void DispatchData( uint32 id, byte * data )
     * 
     * Sends data from the second argument to the receiver,
     * whose id is passed in the first argument
     */
    /// <summary>
    /// Delegate for "DispatchData" library method.
    /// Sends data from the second argument to the receiver,
    /// whose id is passed in the first argument
    /// </summary>
    /// <param name="id">ID of entity</param>
    /// <param name="data">Data to dispatch</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DispatchData_Delegate( UInt32 id, byte[] data );

    /*
     * void StopAccepting()
     * 
     * This function makes DLL quit from AcceptConnection().
     * We cannot interrupt execution of that code ourselves,
     * because the DLL can be unmanaged, and threads running
     * unmanaged code are not Abort()ed
     */
    /// <summary>
    /// Delegate for "StopAccepting" library method.
    /// This function makes DLL quit from AcceptConnection().
    /// We cannot interrupt execution of that code ourselves,
    /// because the DLL can be unmanaged, and threads running
    /// unmanaged code are not Abort()ed
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void StopAccepting_Delegate();


    /*
     * void Terminate()
     * 
     * This function is called when we close the interface,
     * so we are not going to use it any longer - on DLL side
     * it is supposed to free all the allocated resources,
     * stop all the threads and all that...
     * After we call this functions, we cannot use the DLL
     * any longer (actually we can, but it's gonna be undefined behavior)
     * 
     * NEEDTOKNOW: Don't call this function unless you've ClosedConnenction
     * for every entity (Sender/Receiver).
     */

    /// <summary>
    /// Delegate for "Terminate" library method.
    /// This function is called when we close the interface,
    /// so we are not going to use it any longer - on DLL side
    /// it is supposed to free all the allocated resources,
    /// stop all the threads and all that...
    /// After we call this functions, we cannot use the DLL
    /// any longer (actually we can, but it's gonna be undefined behavior)
    ///  
    /// NEEDTOKNOW: Don't call this function unless you've ClosedConnenction
    /// for every entity (Sender/Receiver).
    /// </summary>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void Terminate_Delegate();

    /*
     * void CloseConnection( uint32 id )
     * 
     * Closes a certain connection, by ID
     */
    /// <summary>
    /// Delegate for "CloseConnection" library method.
    /// Closes a certain connection, by ID
    /// </summary>
    /// <param name="id"> ID of entity</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CloseConnection_Delegate( UInt32 id );


    #endregion




    /// <summary>
    /// Represents an interface (with a corresponding DLL)
    /// that sustains Receivers and Senders functionality
    /// </summary>
    class ExternalInterface : IDisposable
    {
        #region FIELDS
        /// <summary>
        /// Loader for linking library
        /// </summary>
        private IDLLLoader DLLLoader;


        #region LIBRARY METHODS DELEGATES
        /// <summary>
        /// Delegate for extracted from linking library method "AcceptConnection" 
        /// </summary>
        private AcceptConnection_Delegate AcceptConnectionRoutine;

        /// <summary>
        /// Delegate for extracted from linking library method "IsReceiver" 
        /// </summary>
        private IsReceiver_Delegate IsReceiverDelegate;

        /// <summary>
        /// Delegate for extracted from linking library method "GetMID" 
        /// </summary>
        private GetMID_Delegate GetMIDDelegate;

        /// <summary>
        /// Delegate for extracted from linking library method "MatchMID" 
        /// </summary>
        private MatchMID_Delegate MatchMIDDelegate;

        /// <summary>
        /// Delegate for extracted from linking library method "GetData" 
        /// </summary>
        private GetData_Delegate GetDataDelegate;

        /// <summary>
        /// Delegate for extracted from linking library method "DispatchData" 
        /// </summary>        
        private DispatchData_Delegate DispatchDataDelegate;

        /// <summary>
        /// Delegate for extracted from linking library method "StopAccepting" 
        /// </summary>
        private StopAccepting_Delegate StopAcceptingRoutine;

        /// <summary>
        /// Delegate for extracted from linking library method "Terminate" 
        /// </summary>
        private Terminate_Delegate TerminateRoutine;

        /// <summary>
        /// Delegate for extracted from linking library method "CloseConnection" 
        /// </summary>
        private CloseConnection_Delegate CloseConnectionDelegate;

        #endregion


        /// <summary>
        /// Is this interface terminating now
        /// </summary>
        private volatile bool _IsTerminating;

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        #endregion





        #region METHODS
        /// <summary>
        /// Check file by given path for identify it as manage or unmanaged library or make a conclusion about library damage.
        /// </summary>
        /// <param name="dllPath">Path to the library</param>
        /// <returns>Success status by ErrorCode, particularly managed or unmanaged type in case of success</returns>
        private int GetDLLType( String dllPath )
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            string dllName = Path.GetFileName(dllPath);
            if (!File.Exists(dllPath))
            {
                logger.Error("{0}. {1}. Library {2}  file not found   ",
                         ErrorCodes.FILE_NOT_FOUND, dllName);
                return ErrorCodes.FILE_NOT_FOUND;
            }

#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_FILE_OPENED,
                  dllName);
#endif
            Stream fileStream = new FileStream( dllPath, FileMode.Open, FileAccess.Read );
            BinaryReader binaryReader = new BinaryReader( fileStream );

            if (fileStream == null || binaryReader == null)
            {
                logger.Error("{0}. {1}. Library {2} file couldn't be open.",
                      ErrorCodes.FILE_COULDNT_BE_OPEN, dllName);
                return ErrorCodes.FILE_COULDNT_BE_OPEN;
            }

            if (fileStream.Length < 64)
            {
                logger.Error("{0}. {1}. Library {2} file has been damaged.",
                      ErrorCodes.DLL_DAMAGED, dllName);
                return ErrorCodes.DLL_DAMAGED;
            }

            fileStream.Position = 0x3C;
            uint peHeaderOffset = binaryReader.ReadUInt32();

            if (peHeaderOffset == 0)
            {
                peHeaderOffset = 0x80;
            }

            if (peHeaderOffset > fileStream.Length - 256)
            {
                logger.Error("{0}. {1}.  Possibly Library {2} file has been damaged. Couldn't continue to work with.",
                      ErrorCodes.DLL_DAMAGED, dllName);
                return ErrorCodes.DLL_DAMAGED; // TODO: Damaged or Not?..
            }

            fileStream.Position = peHeaderOffset;
            uint peHeaderSignature = binaryReader.ReadUInt32();
            if (peHeaderSignature != 0x00004550)
            {
                logger.Error("{0}. {1}. False header signature.  Library {2} file has been damaged.",
                         ErrorCodes.DLL_DAMAGED, dllName);
                return ErrorCodes.DLL_DAMAGED;
            }

            fileStream.Position += 20;

            const ushort PE32 = 0x10b;
            const ushort PE32Plus = 0x20b;

            ushort peFormat = binaryReader.ReadUInt16();
            if (peFormat != PE32 && peFormat != PE32Plus)
            {
                logger.Error("{0}. {1}. False peFormat.  Library {2} file has been damaged.",
                           ErrorCodes.DLL_DAMAGED, dllName);
                return ErrorCodes.DLL_DAMAGED;
            }

            ushort dataDictionaryOffset = (ushort)(peHeaderOffset + (peFormat == PE32 ? 232 : 248));
            fileStream.Position = dataDictionaryOffset;

            uint cliHeaderRva = binaryReader.ReadUInt32();
            if (cliHeaderRva != 0)
            {
#if DEBUG
                logger.Trace(LogTraceMessages.LIBRARY_TYPE_IDENTIFIED,
                      dllName, "Managed");
#endif
                return ErrorCodes.MANAGED_DLL;
            }
            else
            {
#if DEBUG
                logger.Trace(LogTraceMessages.LIBRARY_TYPE_IDENTIFIED,
                      dllName, "UnManaged");
#endif
                return ErrorCodes.UNMANAGED_DLL;
            }
        }


        
         /// <summary>
         /// Supposed to call a DLL function, that
         /// accepts connection and returns an id
         /// that the dll uses to identify the entity
         /// and then
         /// </summary>
         /// <returns>new externalEntity for the connection</returns>
        public ExternalEntity AcceptConnection()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            // if we are terminating, quit right now
            if (_IsTerminating)
            {
#if DEBUG
                logger.Trace(LogTraceMessages.METHOD_FINISHED,
                      MethodBase.GetCurrentMethod(), "Interface terminating");
#endif
                return null;
            }

            UInt32 newID = AcceptConnectionRoutine();

            // if we are terminating, quit right now
            if (_IsTerminating)
            {
#if DEBUG
                logger.Trace(LogTraceMessages.METHOD_FINISHED,
                     MethodBase.GetCurrentMethod(), "Interface terminating");
#endif
                return null;

            }

            return new ExternalEntity( newID, IsReceiverDelegate );
        }



        #region DELEGATES SETTERS
        /// <summary>
        /// Set "GetMID" and "MatchMID" delegates of entity to delegates of this interface
        /// </summary>
        /// <param name="entity">entity to setting</param>
        public void PassMIDDelegates( IDefinedEntity entity )
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            entity.AcceptMIDDelegates( GetMIDDelegate, MatchMIDDelegate );
        }



        /// <summary>
        /// Set "GetData" delegate of sender to delegate of this interface
        /// </summary>
        /// <param name="sender">Sender to setting</param>
        public void PassGetDataDelegateToSender( Sender sender )
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            sender.AcceptGetDataDelegate( GetDataDelegate );
        }



        /// <summary>
        /// Set "CloseConnection" delegate of entity to delegate of this interface
        /// </summary>
        /// <param name="entity">Entity to setting</param>
        public void PassCloseConnectionDelegate( IDefinedEntity entity )
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            entity.AcceptCloseConnectionDelegate( CloseConnectionDelegate );
        }



        /// <summary>
        /// Set "DispatchData" delegate of receiver to delegate of this interface
        /// </summary>
        /// <param name="receiver">Receiver to setting</param>
        public void PassDispatchDataDelegateToReceiver( Receiver receiver )
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            receiver.AcceptDispatchDataDelegate( DispatchDataDelegate );
        }

        #endregion



        /// <summary>
        /// Terminating the interface
        /// </summary>
        public void CloseAccepting()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            _IsTerminating = true;

            StopAcceptingRoutine();
        }

        /// <summary>
        /// Disposed the interface
        /// </summary>
        public void Dispose()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            TerminateRoutine();
#if DEBUG
            logger.Trace( LogTraceMessages.LIBRARY_UNLOADING,
                "The");
#endif
            DLLLoader.UnloadDLL();

            DLLLoader = null;
#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_UNLOADED,
                "The");
#endif
        }

        #endregion





        #region CONSTRUCTORS
        /// <summary>
        /// Explore given path, check and load the library,  
        /// and create instance of ExternalInterface
        /// </summary>
        /// <param name="dllPath">Path to the library</param>
        public ExternalInterface(String dllPath)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.CONSTRUCTOR_INVOKED);

#endif
            string dllName = Path.GetFileName(dllPath);
#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_TYPE_REQUESTED,
                dllName);
#endif
            int statusGetDLLType = GetDLLType(dllPath);
#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_TYPE_RESPONSED,
                  dllName, statusGetDLLType);
#endif
            switch (statusGetDLLType)
            {
                case ErrorCodes.UNMANAGED_DLL:

#if DEBUG
                    logger.Trace(LogTraceMessages.LIBRARY_TYPE_IDENTIFIED,
                          dllName, "UnManaged");
#endif

                    DLLLoader = new UnmanagedDLLLoader();
                    break;
                case ErrorCodes.MANAGED_DLL:
#if DEBUG
                    logger.Trace(LogTraceMessages.LIBRARY_TYPE_IDENTIFIED,
                          dllName, "Managed");
#endif
                    DLLLoader = new ManagedDLLLoader();
                    break;
                case ErrorCodes.DLL_DAMAGED:
                    logger.Error("{0}. {1}. DLL {2} file is damaged (maybe it's not even a DLL)  ",
                      ErrorCodes.DLL_DAMAGED, dllName);
                    throw new Exception(dllPath + " DLL file is damaged (maybe it's not even a DLL)");
                case ErrorCodes.FILE_NOT_FOUND:
                    logger.Error("{0}. {1}. Library {2} file not found.  ",
                           ErrorCodes.FILE_NOT_FOUND, dllName);
                    throw new Exception(dllPath + " DLL file not found");
                case ErrorCodes.FILE_COULDNT_BE_OPEN:
                    logger.Error("{0}. {1}. Library {2} file couldn't.  ",
                           ErrorCodes.FILE_NOT_FOUND, dllName);
                    throw new Exception("Couldn't open " + dllPath);
                default:
                    logger.Error("{0}. {1}. Library {2} type not recognized. DLLType() returned some stupid shit  ",
                          ErrorCodes.FILE_NOT_FOUND, dllName);
                    throw new Exception("Internal Error: DLLType() returned some stupid shit\r\nDLL file: " + dllPath);
            }


#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_LOADING,
                  dllName);
#endif
            DLLLoader.LoadDLL(dllPath);

#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_LOADED,
                  dllName);

            logger.Trace(LogTraceMessages.LIBRARY_GETTING_DELEGATES,
                  dllName);
#endif
            AcceptConnectionRoutine = DLLLoader.GetAcceptConnectionDelegate();
            IsReceiverDelegate = DLLLoader.GetIsReceiverDelegate();
            GetMIDDelegate = DLLLoader.GetGetMIDDelegate();
            MatchMIDDelegate = DLLLoader.GetMatchMIDDelegate();
            GetDataDelegate = DLLLoader.GetGetDataDelegate();
            DispatchDataDelegate = DLLLoader.GetDispatchDataDelegate();
            StopAcceptingRoutine = DLLLoader.GetStopAcceptingDelegate();
            TerminateRoutine = DLLLoader.GetTerminateDelegate();
            CloseConnectionDelegate = DLLLoader.GetCloseConnectionDelegate();

#if DEBUG
            logger.Trace(LogTraceMessages.LIBRARY_DELEGATES_RECEIVED,
                  dllName);
#endif

            _IsTerminating = false;

        }
        #endregion

    }
}
