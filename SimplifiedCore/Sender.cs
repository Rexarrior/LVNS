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
    /// Imagine any sender entity
    /// </summary>
    class Sender : IDefinedEntity
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        private UInt32 _ID;

        #region delegates for library methods

        /// <summary>
        /// Delegate for extracted from linked library method "GetMID"
        /// </summary>
        private GetMID_Delegate _GetMIDRoutine;



        /// <summary>
        /// Delegate for extracted from linked library method "MatchMID"
        /// </summary>
        private MatchMID_Delegate _MatchMIDRoutine;



        /// <summary>
        /// Delegate for extracted from linked library method "GetData"
        /// </summary>
        private GetData_Delegate _GetDataRoutine;



        /// <summary>
        /// Delegate for extracted from linked library method "CloseConnection"
        /// </summary>
        private CloseConnection_Delegate _CloseConnectionRoutine;
        #endregion

        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();



        /// <summary>
        /// Get matching id of this sender. 
        /// </summary>
        /// <returns>MID</returns>
        public string GetMID()
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                MethodBase.GetCurrentMethod());
            logger.Trace(LogTraceMessages.LIBRARY_METHOD_USE,
                "GetMID");

#endif

            StringBuilder buffer = new StringBuilder(1024);

            _GetMIDRoutine(_ID, buffer);

            return buffer.ToString();
        }



        /// <summary>
        /// Mathc ID with matching id for 
        /// </summary>
        /// <param name="id">ID to match</param>
        /// <returns>is ID equal matchID</returns>
        public bool MatchMID(string id)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                MethodBase.GetCurrentMethod());
            logger.Trace(LogTraceMessages.LIBRARY_METHOD_USE,
                "MatchMID");

#endif
            return (_MatchMIDRoutine(_ID, id) != 0);
        }



        /// <summary>
        /// Set current ID to given ID
        /// </summary>
        /// <param name="id"> New ID</param>
        public void AcceptID(UInt32 id)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            _ID = id;
        }


        #region setters for the delegates
        /// <summary>
        /// Set current "GetMID" and "MatchMID" delegates of extracted
        /// from linking library methods to given delegates. 
        /// </summary>
        /// <param name="getMID"> "GetMID" delegate of extracted from linking library method "GetMID"</param>
        /// <param name="matchMID">"MatchMID" delegate of extracted from linking library method "MatchMID""</param>
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
        /// Set current "GetData" delegate of executed
        /// from linking library method to given delegate. 
        /// </summary>
        /// <param name="getData">"GetData" delegate of extracted from linking library method "GetData"</param>
        public void AcceptGetDataDelegate(GetData_Delegate getData)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            _GetDataRoutine = getData;
        }





        /// <summary>
        /// Set current "CloseConnection" delegate of executed
        /// from linking library method to given delegate. 
        /// </summary>
        /// <param name="closeConnection">"CloseConnection" delegate of extracted from linking library method "CloseConnection"</param>
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
        /// Get data to send.
        /// </summary>
        /// <param name="data">buffer for getting data</param>
        public void GetData(byte[] data)
        {
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                MethodBase.GetCurrentMethod());
            logger.Trace(LogTraceMessages.LIBRARY_METHOD_USE,
                "GetData");

#endif
            _GetDataRoutine( _ID, data );
        }




        /// <summary>
        /// Close connection of this sender.
        /// </summary>
        public void CloseConnection()
        {
#if DEBUG
#if DEBUG
            logger.Trace(LogTraceMessages.METHOD_INVOKED,
                MethodBase.GetCurrentMethod());
            logger.Trace(LogTraceMessages.LIBRARY_METHOD_USE,
                "CloseConnection");

#endif
#endif
            _CloseConnectionRoutine( _ID );
        }
    }
}
