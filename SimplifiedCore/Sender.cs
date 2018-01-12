using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplifiedCore
{
    class Sender : IMatchable, IDefinedEntity
    {
        private UInt32 _ID;

        private GetMID_Delegate _GetMIDRoutine;
        private MatchMID_Delegate _MatchMIDRoutine;

        private GetData_Delegate _GetDataRoutine;

        public string GetMID()
        {
            StringBuilder sb = new StringBuilder(1024);

            _GetMIDRoutine(_ID, sb);

            return sb.ToString();
        }

        public bool MatchMID(string id)
        {
            return (_MatchMIDRoutine(_ID, id) != 0);
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

        public void AcceptGetDataDelegate(GetData_Delegate getData)
        {
            _GetDataRoutine = getData;
        }

        /*
        * Retreives data from the Sender
        * 
        * THIS FUNCTION IS SUPPOSED TO BE
        * OUTSIDE OF THE "SimplifiedCore" -
        * IN A SEPARATE DLL
        */
        public void GetData(byte[] data)
        {
            _GetDataRoutine( _ID, data );
        }
    }
}
