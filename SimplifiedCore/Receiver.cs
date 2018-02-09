using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Reflection;


namespace SimplifiedCore
{
    /// <summary>
    /// Imagine any receiver entity
    /// </summary>
    class Receiver : IDefinedEntity
    {
        /// <summary>
        /// Uniqual identifier
        /// </summary>
        private UInt32 _ID;

        #region The Library delegates
        /// <summary>
        /// Delegate for extracted from linking library method "GetMID"
        /// </summary>
        private GetMID_Delegate _GetMIDRoutine;

        /// <summary>
        /// Delegate for extracted from linking library method "MatchMid"
        /// </summary>
        private MatchMID_Delegate _MatchMIDRoutine;

        /// <summary>
        /// Delegate for extracted from linking library method "DispatchData"
        /// </summary>
        private DispatchData_Delegate _DispatchDataRoutine;

        /// <summary>
        /// Delegate for extracted from linking library method "CloseConnection"
        /// </summary>
        private CloseConnection_Delegate _CloseConnectionRoutine;
        #endregion

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();

        
        
        /// <summary>
        /// Get matching ID of this receiver
        /// </summary>
        /// <returns>Matching ID</returns>
        public string GetMID()
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                MethodBase.GetCurrentMethod());
            logger.Trace(LogTraceMessages.LIBRARY_METHOD_USE,
                "GetMID");

#endif
            StringBuilder buffer = new StringBuilder(1024);

            _GetMIDRoutine( _ID, buffer );

            return buffer.ToString();
        }


        /// <summary>
        /// Match given ID with current matching ID
        /// </summary>
        /// <param name="id">Id to match</param>
        /// <returns>is IDs equal</returns>
        public bool MatchMID(string id)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                MethodBase.GetCurrentMethod());
            logger.Trace(LogTraceMessages.LIBRARY_METHOD_USE,
                "MatchMID");

#endif
            return (_MatchMIDRoutine( _ID, id ) != 0);
        }



        /// <summary>
        /// Set current ID to given ID. 
        /// </summary>
        /// <param name="id">New ID</param>
        public void AcceptID(UInt32 id)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            _ID = id;
        }


        #region setters for the library delegates
        /// <summary>
        /// Set current "GetMID" and "MatchMID" delegates of
        /// extracted from linking library method to given delegates. 
        /// </summary>
        /// <param name="getMID">"GetMID" delegate of extracted from linking library "GetMID" method</param>
        /// <param name="matchMID">"MatchMID" delegate of extracted from linking library "MatchMID" method</param>
        public void AcceptMIDDelegates(GetMID_Delegate getMID, MatchMID_Delegate matchMID)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            _GetMIDRoutine = getMID;
            _MatchMIDRoutine = matchMID;
        }



        /// <summary>
        /// Set current "DispatchData" delegate of
        /// extracted from linking library method to given delegate. 
        /// </summary>
        /// <param name="dispatchData">"DispatchData" delegate of extracted from linking library "DispatchData" method</param>
        public void AcceptDispatchDataDelegate(DispatchData_Delegate dispatchData)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            _DispatchDataRoutine = dispatchData;
        }



        /// <summary>
        /// Set current "closeConnection" delegate of
        /// extracted from linking library method to given delegate. 
        /// </summary>
        /// <param name="closeConnection">"CloseConnection" delegate of extracted from linking library "CloseConnection" method</param>
        public void AcceptCloseConnectionDelegate(CloseConnection_Delegate closeConnection)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            _CloseConnectionRoutine = closeConnection;
        }
        #endregion


        /// <summary>
        /// Dispatch given data
        /// </summary>
        /// <param name="data">Data to dispatch</param>
        public void DispatchData(byte[] data)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                MethodBase.GetCurrentMethod());
            logger.Trace(LogTraceMessages.LIBRARY_METHOD_USE,
                "DispatchData");

#endif
            _DispatchDataRoutine( _ID, data );
        }



        /// <summary>
        /// Close current connection
        /// </summary>
        public void CloseConnection()
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                MethodBase.GetCurrentMethod());
            logger.Trace(LogTraceMessages.LIBRARY_METHOD_USE,
                "CloseConnection()");

#endif
            _CloseConnectionRoutine( _ID );
        }
    }
}
