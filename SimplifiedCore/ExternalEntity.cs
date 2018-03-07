using System;
using NLog;
using System.Reflection;
namespace SimplifiedCore
{
    /*
     * An entity, not yet identified
     * as a receiver or as a sender
     */
    class ExternalEntity
    {
        #region FIELDS
        /// <summary>
        /// Unique identifier
        /// Used to sustain the ID that
        /// interface (DLL) identifies this entity by.
        /// </summary>
        private UInt32 _ID;

        /// <summary>
        /// Delegate for extracted from linking library method "isReceiver"
        /// </summary>
        private IsReceiver_Delegate _IsReceiverRoutine;

        
        private static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        #endregion


        #region PROPERTIES
        public uint ID { get => _ID;  }

        public IsReceiver_Delegate IsReceiverRoutine { get => _IsReceiverRoutine; set => _IsReceiverRoutine = value; }

        #endregion


        #region METHODS
        /// <summary>
        /// Identify the entity as receiver or sender.
        /// </summary>
        /// <returns>is this entity receiver?</returns>
        public bool IsReceiver()
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            return (_IsReceiverRoutine( _ID ) != 0);
        }


        /// <summary>
        /// Set definedEntitie's ID to id of this entity
        /// </summary>
        /// <param name="definedEntity">definedEntity to setting</param>        
        public void PassID( IDefinedEntity definedEntity )
        {
#if DEBUG
            logger.Trace( LogTraceMessages.METHOD_INVOKED,
                  MethodBase.GetCurrentMethod());
#endif
            definedEntity.AcceptID( _ID );
        }


        #endregion



        #region CONSTRUCTORS
        /// <summary>
        /// Create instatce of ExternalEntity
        /// </summary>
        /// <param name="id">ID of new Entity</param>
        /// <param name="isReceiver">is new Entity receiver?</param>
        public ExternalEntity( UInt32 id, IsReceiver_Delegate isReceiver )
        {
#if DEBUG
            logger.Trace(LogTraceMessages.CONSTRUCTOR_INVOKED);
#endif
            _ID = id;
            _IsReceiverRoutine = isReceiver;
        }
        #endregion
    }

}