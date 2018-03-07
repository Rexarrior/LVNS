using System;
using System.Threading;
using NLog;
using System.Reflection;
using System.Linq;

namespace SimplifiedCore
{
    /*
     * Every Sender-Receiver connection is
     * sustained by this class - using constructor,
     * we assign it a sender and a receiver.
     * 
     * To open connection, funciton OpenTransfer()
     * is used - it starts the main connection thread,
     * that receives data from sender and then sends
     * it to receiver in a loop.
     * 
     * Use CloseTransfer() function to close the
     * connection.
     * 
     * Function MatchCheck() is called if we want
     * to find out if this is the Connection we are
     * seeking, as an example, when we need to close
     * a connection between certain "points", that
     * we have references for.
     */

    /// <summary>
    /// Represents conenction between an sender and an receiver
    /// </summary>
    class Connection : IDisposable
    {
        #region FIELDS
        /// <summary>
        /// Sender of this connection
        /// </summary>
        private Sender _Sender;

        /// <summary>
        /// Receiver of this connection
        /// </summary>
        private Receiver _Receiver;

        /// <summary>
        /// Thread for data transfer methods running.
        /// </summary>
        private Thread _TransferThread;

        /// <summary>
        /// Token for control transfer loop.
        /// </summary>
        private CancellationTokenSource _TransferLoop;


        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        #endregion


        #region PROPERTIES
        public string ReceiverMID { get => _Receiver.GetMID(); }

        public string SenderMID { get => _Sender.GetMID(); }

        #endregion



        #region METHODS
        //TODO: It is temporary solution. So long. 
        /// <summary>
        /// Checking the given byte array for a valuable data contained. 
        /// </summary>
        /// <param name="data">An Array for checking.</param>
        /// <returns>Return true if the array contains some non zero bytes.</returns>
        private bool CheckDataForZero(byte[] data)
        {
            return data.Any(x=> x != 0);
        }


    

        /// <summary>
        /// Main logic of data transfer
        /// The routine, run by the main thread (_TransferThread)
        /// </summary>
        /// <param name="transferLoopToken"></param>
        private void TransferMain( CancellationToken transferLoopToken )
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                MethodBase.GetCurrentMethod());
#endif

            while (!transferLoopToken.IsCancellationRequested)
            {
#if DEBUG
                byte[] data = new byte[OtherConsts.DATA_BUFFER_SIZE];
                logger.Trace(LogTraceMessages.LIBRARY_METHOD_USE,
                    "GetData");
                logger.Trace(LogTraceMessages.LIBRARY_METHOD_USE,
                    "DispatchData");
#endif
                // UNSAFE code here - this freezes, if
                // DLL's GetData or DispatchData hangs
                _Sender.GetData(data);
                // SNIFF_POINT Point, when we can sniff and analyse messages
                
                _Receiver.DispatchData(data);
                //data = new byte[OtherConsts.DATA_BUFFER_SIZE];
                Thread.Sleep(OtherConsts.CONNECTION_LOOP_SLEEP_TIME);

            }
#if DEBUG
            logger.Trace(LogTraceMessages.TRANSFER_END);
#endif
        }



        /// <summary>
        /// Start data transfer thread
        /// </summary>
        /// <returns>Success status by ErrorCode enum</returns>
        public int OpenTransfer()
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif


            // _TransferThread is null only right after
            // creation of the connection, before we run it
            // for the first time
            if (_TransferThread != null) // if this is fresh, skip this
            {
                if (!_TransferLoop.IsCancellationRequested) // if this is not fresh, and cancellation hasn't been requested - it's now running
                {
                    logger.Error("{0}. Attempt to open transfer twice or more time. ",
                                          ErrorCodes.ALREADY_RUNNING);
                    return ErrorCodes.ALREADY_RUNNING;
                }
                /*
                // we can actually put away else and just
                // put its body here, because the "if" above
                // just returns. but "else" makes it clearer,
                // that we run the code below when cancellation
                // has already been requested.
                */
                else
                {
                    /*
                     * okay, cancellation has been requested.
                     * it's good if everything has terminated
                     * by the moment we run OpenTransfer() again.
                     * But if not, the only thing that is probably
                     * hang is the _TransferThread
                     */
                    if (_TransferThread.IsAlive)
                    {
                        if (!_TransferThread.Join(5000)) // wait 5 seconds
                        {
                            logger.Error("{0}. Attempt to open transfer while previoous transfer is terminating. ",
                                         ErrorCodes.PREVIOUS_RUN_DOESNT_TERMINATE);
                            return ErrorCodes.PREVIOUS_RUN_DOESNT_TERMINATE;
                        }
                    }
                }
            }

            // Same as _TransferThread, _TransferLoop
            // is only null, if this is a "fresh" Connection,
            // meaning we haven't started it yet
            if (_TransferLoop != null)
            {


                _TransferLoop.Dispose();
#if DEBUG
                logger.Trace(LogTraceMessages.TRANSFER_LOOP_DISPOSED);
#endif
            }

#if DEBUG
            logger.Trace(LogTraceMessages.TRANSFER_STARTING);
#endif
            _TransferThread = new Thread( delegate() { TransferMain(_TransferLoop.Token); } );
            _TransferLoop = new CancellationTokenSource();

            _TransferThread.Start();
#if DEBUG
            logger.Trace(LogTraceMessages.TRANSFER_STARTED_SUCCESFULL);
#endif
            return ErrorCodes.ERROR_SUCCESS;
        }





        // TODO: Add safe closing
        // I mean, it is unsafe, because
        // DispatchData and GetData may hang
        // Or not.
        /// <summary>
        /// Close data transfer thread
        /// </summary>
        /// <returns>Success status by ErrorCode enum</returns>
        public int CloseTransfer()
        {
            // _TransferThread is null only right after
            // creation of the connection, before we run it
            // for the first time
            if (_TransferThread == null)
            {

                logger.Error("{0}.  Attempt to close transfer withour starting.",
                                      ErrorCodes.NOT_RUNNING);
                return ErrorCodes.NOT_RUNNING; // haven't started yet
            }
            else
            {
                if (!_TransferThread.IsAlive)
                {
                    logger.Error("{0}. Attempt to close transfer twice or more time.",
                                          ErrorCodes.NOT_RUNNING);
                    return ErrorCodes.NOT_RUNNING; // already terminated
                }
                else
                {
                    if (_TransferLoop.IsCancellationRequested)
                    {

                        logger.Error("{0}. Attempt to close transfer while it is closing.   ",
                                              ErrorCodes.PENDING_TERMINATION);
                        return ErrorCodes.PENDING_TERMINATION; // already asked for termination
                    }
                }
            }

            _TransferLoop.Cancel();

#if DEBUG
            logger.Trace(LogTraceMessages.TRANSFER_LOOP_CANCELED);
#endif

            return ErrorCodes.ERROR_SUCCESS;
        }



        /// <summary>
        /// Check that given pair form this connection
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="receiver">Receiver</param>
        /// <returns>is this connection between given pair? </returns>
        public bool MatchCheck(Sender sender, Receiver receiver)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif

            return ((sender == _Sender) && (receiver == _Receiver));
        }

        
        /// <summary>
        ///  This function is needed when you
        ///  have to be sure that the transfer
        ///  has stopped before moving on
        /// </summary>
        /// <returns>Success status by ErrorCode enum</returns>
        public int WaitForTermination()
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif

            if (_TransferThread == null)
            {

                logger.Error("{0}. Attempt to begin waiting for termination not started transfer.",
                                      ErrorCodes.NOT_RUNNING);
                return ErrorCodes.NOT_RUNNING;
            }
            else
            {
                if (!_TransferLoop.IsCancellationRequested)
                {

                    logger.Error("{0}. Attempt to begin waiting for not initializing termination.",
                                          ErrorCodes.TERMINATION_NOT_REQUESTED);
                    return ErrorCodes.TERMINATION_NOT_REQUESTED;
                }
                else
                {
#if DEBUG
                    logger.Trace(LogTraceMessages.WAITING_FOR_TERMINATION);
#endif
                    _TransferThread.Join();
#if DEBUG
                    logger.Trace(LogTraceMessages.TERMINATED);
#endif
                    return ErrorCodes.ERROR_SUCCESS;
                }
            }
        }


        /// <summary>
        /// Invoke CloseConnection method for both participants of this connection
        /// </summary>
        public void CloseBothConnections()
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif

            // Close connections point -
            // here we call .dll functions
            _Sender.CloseConnection();
            _Receiver.CloseConnection();
#if DEBUG
            logger.Trace(LogTraceMessages.CONNECTION_BOTH__CLOSED);
#endif
        }


        /// <summary>
        /// Dispose this connection
        /// </summary>
        public void Dispose()
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            _TransferThread = null;

            _TransferLoop.Dispose();

            _Sender = null;
            _Receiver = null;
        }
        #endregion



        #region CONSTRUCTORS
        /// <summary>
        /// Create representing of connection between the sender and the receiver
        /// without starting a data transfer.
        /// </summary>
        /// <param name="sender">Sender of connection</param>
        /// <param name="receiver">Receiver of connection</param>
        public Connection(Sender sender, Receiver receiver)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.CONSTRUCTOR_INVOKED);
                 
#endif
            _Sender = sender;
            _Receiver = receiver;

            _TransferThread = null;

            _TransferLoop = null;
        }
        #endregion
    }
}
