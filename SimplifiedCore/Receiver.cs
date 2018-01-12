using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplifiedCore
{
    class Receiver : IMatchable, IDefinedEntity
    {
        private UInt32 _ID;

        private GetMID_Delegate _GetMIDRoutine;
        private MatchMID_Delegate _MatchMIDRoutine;

        private DispatchData_Delegate _DispatchDataRoutine;

        public string GetMID()
        {
            StringBuilder sb = new StringBuilder( 1024 );

            _GetMIDRoutine( _ID, sb );

            return sb.ToString();
        }

        public bool MatchMID(string id)
        {
            return (_MatchMIDRoutine( _ID, id ) != 0);
        }

        public void AcceptID(UInt32 id)
        {
            _ID = id;
        }

        public void AcceptMIDDelegates(GetMID_Delegate getMID, MatchMID_Delegate matchMID)
        {
            _GetMIDRoutine = getMID;
            _MatchMIDRoutine = matchMID;
        }

        public void AcceptDispatchDataDelegate(DispatchData_Delegate dispatchData)
        {
            _DispatchDataRoutine = dispatchData;
        }

        /*
         * Sends data to the Receiver
         * 
         * THIS FUNCTION IS SUPPOSED TO BE
         * OUTSIDE OF THE "SimplifiedCore" -
         * IN A SEPARATE DLL
         * 
         * And the id parameter does not belong
         * to this SimplifiedCore system - we
         * just send it to the DLL and it finds out
         * which device it exactly is
         */
        public void DispatchData(byte[] data)
        {
            _DispatchDataRoutine( _ID, data );
        }
    }
}
