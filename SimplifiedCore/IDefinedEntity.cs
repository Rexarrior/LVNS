using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplifiedCore
{
    interface IDefinedEntity
    {
        /// <summary>
        /// Set current ID to given ID
        /// 
        /// Note:
        /// Function called by either
        /// Receiver of Sender to get
        /// ID of the connected entity
        /// (and again, this ID only
        /// has sense to DLL)
        /// </summary>
        /// <param name="id">New ID</param>
        void AcceptID(UInt32 id);



        /// <summary>
        /// Set current "GetMID" and "MatchMID" delegates of extracted
        /// from linking library methods to given delegates. 
        /// </summary>
        /// <param name="getMID"> "GetMID" delegate of extracted from linking library method "GetMID"</param>
        /// <param name="matchMID">"MatchMID" delegate of extracted from linking library method "MatchMID""</param>
        void AcceptMIDDelegates(GetMID_Delegate getMID, MatchMID_Delegate matchMID);



        /// <summary>
        /// Set current "GetData" delegate of executed
        /// from linking library method to given delegate. 
        /// </summary>
        /// <param name="getData">"GetData" delegate of extracted from linking library method "GetData"</param>
        void AcceptCloseConnectionDelegate(CloseConnection_Delegate closeConnection);
    }
}
