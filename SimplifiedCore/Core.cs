using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using NLog;
using System.Reflection;


/*
 * commentary notes to check out:
 * 
 * NEEDTOKNOW - comments with this note ABSOLUTELY
 * HAVE TO BE READ AND UNDERSTOOD by those, who are
 * gonna work somehow with the system (Core).
 * 
 * UNSAFE - (it's not about the code being unmanaged,
 * it's exatcly about it being unsafe) - make it safer
 * TODO - implement/rework
 * SNIFF_POINT - where we can mitm the messages
 * 
 * BALANCE - make code (or comments) look more "balanced" and convenient
 */

    /*
     * TODO:
     * Make all the function explanations
     * use <summary>, so that we could
     * see the description just hovering
     * our mouse cursor over it.
     */
namespace SimplifiedCore
{
    /*
     * When an external entity connects to the core,
     * it is "registered" - i.e put in a corresponding list -
     * either as a receiver (put in list of receivers) or
     * as a sender (put in list of senders).
     * But before getting it into a list, we check if
     * there is a matching entity in the opposite list.
     * If we found one, the current entity is even not gonna
     * get put in its list, because there is no need of it - 
     * no one will connect to it when it is already in a connection.
     * 
     * To match a corresponding entity in a list, MatchMID() function
     * is used. The MID is retreived from a connecting entity using
     * GetMID() function. MID stands for "Match ID" - you see, the ID,
     * that is used to Match.     
     * A VERY IMPORTANT NOTICE is that if we found a one-sided match,
     * like receiver.MatchMID( sender.GetMID() ) returned true,
     * WE DO NOT ESTABLISH THE CONNECTION BETWEEN THEM UNTIL
     * OPPOSITE MATCH IS PERFORMED - speaking about this example,
     * it would be sender.MatchMID( receiver.GetMID ) - this must return true
     * or otherwise we skip the pair (and maybe log this event
     * in a base of interesting events)
     * 
     * CONFIG FILE:
     * Legend:
     * [optinal] - this can be omitted
     * this/that/those/.. - only one must present
     * <template> - this must be replaced with the
     * string that corresponds the word in those
     * angle brackets
     * ... - "and so on" - this 
     *
     * Notes:
     * All the paths, given in the file, that are not full,
     * are evaluated starting with the curent directory.
     * But the very current directory may be specified
     * differently. This is given in the first line of the
     * config file.
     * 
     * File Format:
     * first line - where the current directory is located: configfile/executable/<directory>
     * if it is "configfile", then current directory is set to the location
     * of the config file, same with "executable" - where the executable is located.
     * If none on those met, we try to set it to the given string,
     * assuming it specifies some directory. If we don't succeed,
     * just return an error PATH_NOT_FOUND
     * Current directory is needed to calculate each of the next
     * paths, if they are not full.
     * Next strings just locate the .dll files used as interfaces:
     * [<.dll path>]
     * [<.dll path>]
     * ...
     * 
     * CODE REVIEW POSSIBLE:
     * When we pass IDs or delegates to the
     * connected entities, we use that strange
     * mechanism: Pass{ entity.Accept( field ) },
     * just because we don't want Core to know
     * anything about those fields. But it looks
     * pretty awkward.
     */
    class Core
    {
        #region FIELDS
        /// <summary>
        /// In this tuple we save externalinterface object
        /// and the thread, that calls AcceptConnection for this
        /// interface in loop
        /// </summary>
        private List<Tuple<ExternalInterface, Thread>> _ExternalInterfaces;

        /// <summary>
        ///  All existing connections between senders and receivers
        /// </summary>
        private List<Connection> _Connections;

        /// <summary>
        /// All receivers which are waiting to an sender
        /// </summary>
        private List<Receiver> _WaitingReceivers;

        /// <summary>
        /// All senders which are waiting to an receiver
        /// </summary>
        private List<Sender> _WaitingSenders;

        /// <summary>
        /// Token for control loop of accept connections in threads
        /// </summary>
        private CancellationTokenSource _AcceptConnectionsLoop;

        /// <summary>
        /// Dirrectory for calculate relative path
        /// </summary>
        private string _CurrentDirectory;

        // because many connections may be accepted
        // simultaneously, we need to control access
        // to the Lists (above) -> so we do lock(<The SyncRoot Below>)
        /// <summary>
        /// Object for lock for the synchronization in the context of adding new connection
        /// </summary>
        private object _NewConnectionSyncRoot = new object();

        /*
            this syncroot is used to hang Start() function
            until everything from the previous time we
            run the Core is disposed. this is possibly
            needed when we call Stop(false) - so we don't
            wait until all the resources are disposed, but
            before the next Start(), we actually NEED them
            to be released
        */
        /// <summary>
        /// Object for lock for provide safely start\stop instance of  Core
        /// </summary>
        private object _CleanUpSyncRoot = new object();

        /// <summary>
        /// Thread for execute methods in context of stoping of Core.
        /// </summary>
        private Thread _CleanUpThread;

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        #endregion


        #region METHODS
        /// <summary>
        /// Add a connection between sender and receiver throw Core
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="receiver">Receiver</param>
        private void AddTransferConnection(Sender sender, Receiver receiver)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED ,
                MethodBase.GetCurrentMethod());
#endif
            _Connections.Add(new Connection(sender, receiver));
            _Connections.Last().OpenTransfer();
            logger.Info(LogInfoMessages.TRANSFER_CONNECTION_OPENED);
        }



        /// <summary>
        /// Register new receiver
        /// </summary>
        /// <param name="receiver">Receiver to be registred</param>
        private void RegisterReceiver(Receiver receiver)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED ,
                  MethodBase.GetCurrentMethod());
#endif
            for (int i = 0; i < _WaitingSenders.Count; i++)
            {
                if (_WaitingSenders[i].MatchMID(receiver.GetMID()))
                {
                    // both-sided check
                    if (!receiver.MatchMID(_WaitingSenders[i].GetMID()))
                    {
                        continue;
                    }
#if DEBUG
                    logger.Trace(LogTraceMessages.RECEIVER_MATCHING_SENDER_FOUNDED);
#endif
                    AddTransferConnection(_WaitingSenders[i], receiver);
                    _WaitingSenders.RemoveAt(i);

                    logger.Info(LogInfoMessages.SENDER_EXPECTED_REGISTRED);
                    return;
                }
            }
#if DEBUG
            logger.Trace(LogTraceMessages.RECEIVER_MATCHING_SENDER_NOT_FOUNDED);
#endif
            // matching sender not found

            _WaitingReceivers.Add(receiver);
            logger.Info(LogInfoMessages.RECEIVER_REGISTRED) ; 
        }



        /// <summary>
        /// Register new Sender
        /// </summary>
        /// <param name="sender">Sender to be registred</param>
        private void RegisterSender(Sender sender)
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif

            for (int i = 0; i < _WaitingReceivers.Count; i++)
            {
                if (_WaitingReceivers[i].MatchMID(sender.GetMID()))
                {
                    // both-sided check
                    if (!sender.MatchMID(_WaitingReceivers[i].GetMID()))
                    {
                        continue;
                    }
#if DEBUG
                    logger.Trace(LogTraceMessages.SENDER_MATCHING_RECEIVER_FOUNDED);
#endif
                    AddTransferConnection(sender, _WaitingReceivers[i]);
                    _WaitingReceivers.RemoveAt(i);
                    logger.Info(LogInfoMessages.RECEIVER_EXPECTED_REGISTRED);
                    return;
                }
            }

            // matching receiver not found
#if DEBUG
            logger.Trace(LogTraceMessages.RECEIVER_MATCHING_SENDER_NOT_FOUNDED);
#endif
            _WaitingSenders.Add(sender);
            logger.Info(LogInfoMessages.SENDER_REGISTRED);
        }

        
        /// <summary>
        /// Remove transfer connection between Sender and Receiver
        /// 
        /// This is only used to remove
        /// Connections while the whole
        /// system is runing.
        /// </summary>
        /// <param name="sender">Sender of removing connection</param>
        /// <param name="receiver">Receiver of removing connection</param>
        private void RemoveTransferConnection(Sender sender, Receiver receiver)
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif

            lock (_NewConnectionSyncRoot)
            {
#if DEBUG
                logger.Trace(LogTraceMessages.SYNK_ROOT_LOCKED,
                    "NewConnection");
#endif
                for (int i = 0; i < _Connections.Count(); i++)
                {
                    if (_Connections[i].MatchCheck(sender, receiver))
                    {
#if DEBUG
                        logger.Trace(LogTraceMessages.CONNECTION_CLOSED);
#endif
                        _Connections[i].CloseTransfer();
                        _Connections.RemoveAt(i);
                        i--; // assuming that there may be more than one match
                    }
                }
            }
        }



        /// <summary>
        /// Accept new connection - register new entity as receiver or as Sender
        /// </summary>
        /// <param name="externalInterface">ExternalInterface that provide connection</param>
        private void AcceptConnection(ExternalInterface externalInterface)
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif

            ExternalEntity newComer = externalInterface.AcceptConnection();

            if (newComer == null)
            {

#if DEBUG
                logger.Trace(LogTraceMessages.EXTERNAL_INTERFACE_TERMINATING);
#endif
                return;
            }

            lock (_NewConnectionSyncRoot)
            {
#if DEBUG
                logger.Trace(LogTraceMessages.SYNK_ROOT_LOCKED,
                    "NewConnection");
#endif
                if (newComer.IsReceiver())
                {

#if DEBUG
                    logger.Trace(LogTraceMessages.RECEIVER_ADDING);
#endif
                    Receiver newReceiver = new Receiver();

                    newComer.PassID(newReceiver);
                    externalInterface.PassMIDDelegates(newReceiver);
                    externalInterface.PassDispatchDataDelegateToReceiver(newReceiver);
                    externalInterface.PassCloseConnectionDelegate(newReceiver);

                    RegisterReceiver(newReceiver);
                }
                else
                {
#if DEBUG
                    logger.Trace(LogTraceMessages.SENDER_ADDING);
#endif
                    Sender newSender = new Sender();

                    newComer.PassID(newSender);
                    externalInterface.PassMIDDelegates(newSender);
                    externalInterface.PassGetDataDelegateToSender(newSender);
                    externalInterface.PassCloseConnectionDelegate(newSender);

                    RegisterSender(newSender);
                }

            }
        }



        /// <summary>
        /// Wrapper for the AcceptConnection method. Need to sustenance loop.
        /// </summary>
        /// <param name="externalInterface">ExternalInterface that provide connection</param>
        /// <param name="acceptConnectionsLoopToken">Using cancelelation token</param>
        private void AcceptConnectionsMain( ExternalInterface externalInterface, CancellationToken acceptConnectionsLoopToken )
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif

            while (!acceptConnectionsLoopToken.IsCancellationRequested)
            {

#if DEBUG
                logger.Trace(LogTraceMessages.CONNECTION_ACCEPTING);
#endif
                AcceptConnection(externalInterface);
            }
        }




        /// <summary>
        /// Register new ExternalInterface from given library
        /// </summary>
        /// <param name="dllPath">Path to the library</param>
        private void RegisterInterface(String dllPath)
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif

            string dllName = Path.GetFileName(dllPath);
            ExternalInterface externalInterface = null;

            try
            {
#if DEBUG
                logger.Trace(LogTraceMessages.EXTERNAL_INTERFACE_CREATING);
#endif
                externalInterface = new ExternalInterface(MakePath(dllPath));
            }
            catch ( Exception ex )
            {
#if DEBUG
                logger.Error("Error: {0} ",
                       ex.Message);
#endif
                Console.WriteLine( ex.Message );
                return;
            }
#if DEBUG
            logger.Trace(LogTraceMessages.EXTERNAL_INTERFACE_THREAD_CREATING);
#endif
            Thread interfaceThread = new Thread(
                delegate ()
                {
                    AcceptConnectionsMain( externalInterface, _AcceptConnectionsLoop.Token );
                }
                );

            interfaceThread.Start();
#if DEBUG
            logger.Trace(LogTraceMessages.EXTERNAL_INTERFACE_CREATED);
#endif
            _ExternalInterfaces.Add( new Tuple<ExternalInterface, Thread>(externalInterface, interfaceThread) );
        }




        /// <summary>
        /// This function is used as a
        /// Thread startup routine for
        /// _CleanUpThread
        /// </summary>
        private void CleanUp()
        {
            // first thing we should do is to pervent accepting
            // new entities and adding them into corresponding lists

            // UNSAFE, because ExternalInterface doesn't handle
            // exceptions, that may be raised when we interrupt
            // connections to those waiting receivers and senders
            // OR NOT UNSAFE because we call CloseConnection() routines
            // from all the Receivers and Senders?
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif

            foreach (Tuple<ExternalInterface, Thread> externalInterface in _ExternalInterfaces)
            {
#if DEBUG
                logger.Trace(LogTraceMessages.EXTERNAL_INTERFACE_STOPING);
#endif
                externalInterface.Item1.CloseAccepting();
                externalInterface.Item2.Join();
#if DEBUG
                logger.Trace(LogTraceMessages.EXTERNAL_INTERFACE_STOPED);
#endif
            }
#if DEBUG
            logger.Trace(LogTraceMessages.EXTERNAL_INTERFACE_ALL_STOPED);
#endif
            // its tokens are used only in the threads
            // that accept external connections in
            // the loop. When all the threads are terminated,
            // we can Dispose the CancellationTokenSource
            _AcceptConnectionsLoop.Dispose();
#if DEBUG
            logger.Trace(LogTraceMessages.CONNECTION_ACCEPT_LOOP_DISPOSED);
#endif
            // now we need to close all the connections

            foreach (Connection connection in _Connections)
            {
#if DEBUG
                logger.Trace(LogTraceMessages.CONNECTION_CLOSING);
#endif
                connection.CloseTransfer();
                // ya, we CloseConnections before Wait(ing)ForTermination
                // that's because GetData() and DispatchData() in Connection
                // may hang
                connection.CloseBothConnections();
                // wait for Connection main thread
                // to terminate properly
                connection.WaitForTermination();
                // finally, we can Dispose all the
                // resources that were allocated
                connection.Dispose();
#if DEBUG
                logger.Trace(LogTraceMessages.CONNECTION_CLOSED);
#endif
            }

            // all the connections are closed.
            // clear the list.
            _Connections.Clear();

#if DEBUG
            logger.Trace(LogTraceMessages.CONNECTION_ALL_CLOSED);
#endif
            // remove all the remaining entities
            foreach (Receiver receiver in _WaitingReceivers)
            {
#if DEBUG
                logger.Trace(LogTraceMessages.RECEIVER_CONNECTION_STOPING);
#endif
                receiver.CloseConnection();
#if DEBUG
                logger.Trace(LogTraceMessages.CONNECTION_CLOSED);
#endif

            }
            _WaitingReceivers.Clear();
#if DEBUG
            logger.Trace(LogTraceMessages.RECEIVER_CONNECTIONS_ALL_STOPED);
#endif
            foreach (Sender sender in _WaitingSenders)
            {
#if DEBUG
                logger.Trace(LogTraceMessages.SENDER_CONNECTION_STOPING);
#endif
                sender.CloseConnection();
#if DEBUG
                logger.Trace(LogTraceMessages.SENDER_CONNECTION_STOPED);
#endif
            }

            _WaitingSenders.Clear();
#if DEBUG
            logger.Trace(LogTraceMessages.SENDER_CONNECTIONS_ALL_STOPED);
#endif
            foreach (Tuple<ExternalInterface, Thread> externalInterface in _ExternalInterfaces)
            {

                externalInterface.Item1.Dispose();
#if DEBUG
                logger.Trace(LogTraceMessages.EXTERNAL_INTERFACE_DISPOSED);
#endif
            }

            // Now, nothing vivid left in this list -
            // so, clear it
            _ExternalInterfaces.Clear();
#if DEBUG
            logger.Trace(LogTraceMessages.EXTERNAL_INTERFACE_ALL_DISPOSED);
#endif
            logger.Info(LogInfoMessages.CORE_STOPED);
        }




        /// <summary>
        /// Construct finally path by givenPath and currentDirectory.
        /// 
        /// Every path found in config file
        /// must be passed through this function,
        /// execpt the one, that is at the first line,
        /// if the first line represents a path of course.
        ///  
        /// So, this is gonna be like "currentString = MakePath( currentString )"
        /// </summary>
        /// <param name="givenPath"></param>
        /// <returns></returns>
        private string MakePath(string givenPath)
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            // Full path are complete
            if (Path.IsPathRooted(givenPath))
            {
                return givenPath;
            }

            return Path.Combine(_CurrentDirectory, givenPath);
        }



        /*
         * TODO: We do not handle the situation,
         * when a single DLL met more than once
         * in a config file BUT MAYBE... we don't
         * even need to handle this case...
         */

        /// <summary>
        /// Start the Core and configurated it by given config
        /// </summary>
        /// <param name="configFilePath">Path to the config File</param>
        /// <returns>Success status by ErrorCode</returns>
        public int Start(String configFilePath)
        {
            // supposing now, that there are only DLL paths
            // in the config file
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            logger.Info(LogInfoMessages.CORE_STARTED);

            string configName = Path.GetFileName(configFilePath); 

            if (_AcceptConnectionsLoop != null && !_AcceptConnectionsLoop.IsCancellationRequested)
            {
                logger.Error("{0}. Attempt to start core when core already running.    ",
                         ErrorCodes.ALREADY_RUNNING);
                return ErrorCodes.ALREADY_RUNNING;
            }

            // wait for the previous termination
            if (_CleanUpThread != null)
            {
#if DEBUG
                logger.Trace(LogTraceMessages.CORE_WAIT_TERMINATION);
#endif
                _CleanUpThread.Join();
            }

            // Create CancellationToken Exactly Here
            _AcceptConnectionsLoop = new CancellationTokenSource();
#if DEBUG
            logger.Trace(LogTraceMessages.TOKEN_CREATED,
                "AcceptConnection");
#endif
            if (!File.Exists(configFilePath))
            {
                logger.Error("{0}. {1}. Config file {2} not found.   ",
                         ErrorCodes.FILE_NOT_FOUND,
                       Path.GetFileName(configFilePath));
                return ErrorCodes.FILE_NOT_FOUND;
            }
#if DEBUG
            logger.Trace(LogTraceMessages.CONFIG_FILE_OPENING,
                configName);
#endif
            FileStream configFile = new FileStream(configFilePath, FileMode.Open,
                FileAccess.Read, FileShare.None);
            StreamReader configReader = new StreamReader(configFile);

            String currentString;
#if DEBUG
            logger.Trace(LogTraceMessages.CONFIG_FILE_READ_DIR,
                  configName);
#endif
            currentString = configReader.ReadLine();
#if DEBUG
            logger.Trace(LogTraceMessages.CORE_CURRENT_DIR_IDENTIFING,
                  currentString);
#endif
            // first actually we should check if configFilePath is rooted
            if (currentString == Strings.ConfigCurrentDirectory.ConfigFile)
            {

                _CurrentDirectory = Path.GetDirectoryName(
                    Path.GetFullPath(configFilePath));

            }
            else
            {
                if (currentString == Strings.ConfigCurrentDirectory.Executable)
                {
                    _CurrentDirectory = Directory.GetCurrentDirectory();
                }
                else
                {
                    if (currentString == Strings.ConfigCurrentDirectory.Path)
                    {
                        currentString = configReader.ReadLine();
                        if (Directory.Exists(currentString))
                        {
                            _CurrentDirectory = currentString;
                        }
                        else
                        {
                            logger.Error("{0}. Config file path not found.  ",
                         ErrorCodes.PATH_NOT_FOUND);
                            return ErrorCodes.PATH_NOT_FOUND;
                        }
                    }
                }
            }

            logger.Info(LogInfoMessages.CORE_CURRENT_DIR_SETED,
                _CurrentDirectory);

#if DEBUG
            logger.Trace(LogTraceMessages.CONFIG_FILE_REGISTERING_INTERFACES);
#endif
            while (!configReader.EndOfStream)
            {
                currentString = configReader.ReadLine();
                RegisterInterface(currentString);

                logger.Info(LogInfoMessages.EXTERNAL_INTERFACE_REGISTERED,
                      currentString);

            }

            configReader.Close();
            configFile.Close();

            logger.Info(LogInfoMessages.CONFIG_FILE_CLOSED, 
                configName);

            return ErrorCodes.ERROR_SUCCESS;
        }

        /*If waitForCompleteTermination is false, the function
        //  quits, leaving _CleanUpThread running.If it's not,
        //  it waits for _CleanUpThread to terminate as many
        //  milliseconds as specified by the second parameter.
        //  If waitMillisecs is zero, the waiting can only be
        //  finished by _CleanUpThread terminating.
        //  So that, waitMillisecs matters if only the first
        //  parameter is true. Since it is optional and its
        //  default value is 0, if you want to wait for certain
        //  completion, just call Stop(true).
         *
         */



        /// <summary>
        ///  Stop the Core: begining the termination of Core. 
        /// </summary>
        /// <param name="waitForCompleteTermination">Is method wait for end of terminating
        /// all entity that need to be terminated</param>
        /// <param name="waitMillisecs">How long method should wait for terminating</param>
        /// <returns>Success status by ErrorCode enum</returns>
        public int Stop(bool waitForCompleteTermination, int waitMillisecs = 0)
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif

            logger.Info(LogInfoMessages.CORE_STOPING);

            if (_AcceptConnectionsLoop == null)
            {
                logger.Error("{0}. Attempt to stop Core without start it.",
                         ErrorCodes.NOT_RUNNING);
                return ErrorCodes.NOT_RUNNING;
            }
            else
            {
                if (_AcceptConnectionsLoop.IsCancellationRequested)
                {
                    if (_CleanUpThread.IsAlive)
                    {
                        logger.Error("{0}. Attempt to stop Core while Core stopping.",
                         ErrorCodes.NOT_RUNNING);
                        return ErrorCodes.PENDING_TERMINATION; // BALANCE not exactly "pending", because _CleanUpThread terminates the Core
                    }
                    else
                    {
                        logger.Error("{0}. Attempt to stop Core without start it.",
                               ErrorCodes.NOT_RUNNING);
                        return ErrorCodes.NOT_RUNNING;
                    }
                }
            }

            _AcceptConnectionsLoop.Cancel();
#if DEBUG
            logger.Trace(LogTraceMessages.TOKEN_CANCELED,
                "AcceptConnectionLoop");
#endif

#if DEBUG
            logger.Trace(LogTraceMessages.CORE_CLEANUP_THREAD_CREATING);
#endif
            _CleanUpThread = new Thread( CleanUp );

            _CleanUpThread.Start();
#if DEBUG
            logger.Trace(LogTraceMessages.CORE_CLEANUP_THREAD_CREATED);
#endif
            if (waitForCompleteTermination)
            {
                if (waitMillisecs == 0)
                {
                    _CleanUpThread.Join();
                }
                else
                {
                    _CleanUpThread.Join( waitMillisecs );
                }
            }

#if DEBUG
            logger.Trace(LogTraceMessages.CORE_STOPED);
#endif
            return ErrorCodes.ERROR_SUCCESS;
        }
        #endregion



        /// <summary>
        /// Initialize Core without any setting.
        /// </summary>
        public Core()
        {
#if DEBUG
            logger.Trace(LogTraceMessages.CONSTRUCTOR_INVOKED);               
#endif
            _Connections = new List<Connection>();

            _WaitingReceivers = new List<Receiver>();
            _WaitingSenders = new List<Sender>();

            _ExternalInterfaces = new List<Tuple<ExternalInterface, Thread>>();

            _AcceptConnectionsLoop = null;
        }
    }
}
