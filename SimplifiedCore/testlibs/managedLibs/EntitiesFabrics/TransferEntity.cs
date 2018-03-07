using System.Collections.Generic;
using System.Linq;





namespace EntitiesFabric
{ 
    #region CosoleServerEntities




    public class TransferEntity: Entity
    {
        protected Stack<byte[]> _buffer;


        public override void Load(byte[] data)
        {
            if (_buffer.Count > 0)
            {
                lock (_buffer)
                {
                    _buffer.Pop().CopyTo(data, 0);
                }
            }
        }



        public override void Receive(byte[] data)
        {
            if (data.Any(x => x != 0))
                lock (_buffer)
                {
                    _buffer.Push(data);
                }
        }

        public TransferEntity(string selfMID, string acceptMID, bool isReceiver, Stack<byte[]> buffer) :
            base(selfMID, acceptMID, isReceiver)
        {
            _buffer = buffer;
            this._loadDelegate = this.Load;
            this._receiveDelegate = this.Receive;
        }


    }


    #endregion

}





