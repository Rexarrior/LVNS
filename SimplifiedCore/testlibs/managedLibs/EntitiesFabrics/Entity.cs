using System;
using System.ComponentModel.Composition;
using NLog;

namespace EntitiesFabric
{
    public delegate void Receive_Delegate(byte[] data);
    public delegate void Load_Delegate(byte[] buffer);



    public interface IEntity
    {



        UInt32 ID { get; set; }

        bool IsReceiver { get; }

        String SelfMID { get; }

        String AcceptsMID { get; }

        Receive_Delegate ReceiveRoutine { get; }

        Load_Delegate LoadRoutine { get; }


        void Shutdown();

    }




    public abstract class Entity : IEntity
        { 
            #region fields
        protected UInt32 _id;
        protected bool _isReceiver;
        protected string _selfMID;
        protected string _acceptMID;
        protected Receive_Delegate _receiveDelegate ;
        protected Load_Delegate _loadDelegate; 
        protected static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region methods
        public virtual void Load(byte[] data)
        {

        }


        public virtual void Receive(byte[] data)
        {

        }
        

        #endregion

        #region IEntity

        public uint ID { get { return _id; } set { _id = value; } }

        public bool IsReceiver => _isReceiver;

        public string SelfMID => _selfMID;

        public string AcceptsMID => _acceptMID;

        public  Receive_Delegate ReceiveRoutine => _receiveDelegate;

        public  Load_Delegate LoadRoutine => _loadDelegate;

        public virtual void Shutdown()
        { }
         
        #endregion

        public Entity(string selfMid, string acceptMid, bool isReceiver)
        {
            _selfMID = selfMid;
            _acceptMID = acceptMid;
            _isReceiver = isReceiver;
            _id = 0;
            _receiveDelegate = Receive;
            _loadDelegate = Load;
            
            
        }



}





}