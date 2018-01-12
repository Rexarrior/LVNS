using System;

namespace SimplifiedCore
{
    /*
     * An entity, not yet identified
     * as a receiver or as a sender
     */
    class ExternalEntity
    {
        /*
         * Used to sustain the ID that
         * interface identifies this entity by.
         */
        private UInt32 _ID;

        /*
         * If this returns true, we register the
         * device (entity) as a receiver. Otherwise,
         * as a sender
         * 
         * This is also supposed to be an external function
         */
        private IsReceiver_Delegate _IsReceiverRoutine;

        public bool IsReceiver()
        {
            return (_IsReceiverRoutine( _ID ) != 0);
        }

        public void PassID( IDefinedEntity definedEntity )
        {
            definedEntity.AcceptID( _ID );
        }

        public ExternalEntity( UInt32 id, IsReceiver_Delegate isReceiver )
        {
            _ID = id;
            _IsReceiverRoutine = isReceiver;
        }
    }
}