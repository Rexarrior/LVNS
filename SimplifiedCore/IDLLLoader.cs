using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplifiedCore
{
    interface IDLLLoader
    {
        /*
         * This function is used to load a DLL.
         * CONSTRUCTOR JUST MAKES AN OBJECT,
         * IT DOES NOT LOAD A DLL
         */
        /// <summary>
        ///  Load the library
        /// </summary>
        /// <param name="dllPath">Path to library</param>
        /// <returns>Success status code by enum ErrorCodes</returns>
        int LoadDLL(String dllPath);


        #region getters for library methods
        /// <summary>
        /// Get delegate for executed method "AcceptConnection"
        /// </summary>
        /// <returns>Delegate for "AcceptConnection"</returns>
        AcceptConnection_Delegate GetAcceptConnectionDelegate();


        /// <summary>
        /// Get delegate for executed method "IsReceiver"
        /// </summary>
        /// <returns>Delegate for "IsReceiver"</returns>
        IsReceiver_Delegate GetIsReceiverDelegate();



        /// <summary>
        /// Get delegate for executed method "GetMID"
        /// </summary>
        /// <returns>Delegate for "Get MID"</returns>
        GetMID_Delegate GetGetMIDDelegate();



        /// <summary>
        /// Get delegate for executed method "MatchMID"
        /// </summary>
        /// <returns>Delegate for "MatchMID"</returns>
        MatchMID_Delegate GetMatchMIDDelegate();



        /// <summary>
        /// Get delegate for executed method "GetData"
        /// </summary>
        /// <returns>Delegate for "GetData"</returns>
        GetData_Delegate GetGetDataDelegate();



        /// <summary>
        /// Get delegate for executed method "DispatchData"
        /// </summary>
        /// <returns>Delegate for "DispatchData"</returns>
        DispatchData_Delegate GetDispatchDataDelegate();



        /// <summary>
        /// Get delegate for executed method "StopAccepting"
        /// </summary>
        /// <returns>Delegate for "StopAccepting"</returns>
        StopAccepting_Delegate GetStopAcceptingDelegate();



        /// <summary>
        /// Get delegate for executed method "Terminate"
        /// </summary>
        /// <returns>Delegate for "Terminate"</returns>
        Terminate_Delegate GetTerminateDelegate();



        /// <summary>
        /// Get delegate for executed method "CloseConnection"
        /// </summary>
        /// <returns>Delegate for "CloseConnection"</returns>
        CloseConnection_Delegate GetCloseConnectionDelegate();
        #endregion


        /// <summary>
        /// Unload linking library
        /// </summary>
        /// <returns>Success status code as ErrorCode enum</returns>
        int UnloadDLL();
    }
}
