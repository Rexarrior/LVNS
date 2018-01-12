using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplifiedCore
{
    /*
     * An entity, that both can check
     * if another entity matches to itself
     * and can be matched to another entity
     */
    interface IMatchable
    {
        /*
         * Checks if the entity matches
         * to the entity that emitted the
         * ID string. For emitting, GetID()
         * is used.
         */
        bool MatchMID(String MID);

        /*
         * this function is supposed to
         * get an identifying byte array
         * from the external entity it is
         * assigned to and use
         *      Encoding.ASCII.ToString( byte[] )
         * to covert it to a standard .NET String
         */
        string GetMID();
    }
}
