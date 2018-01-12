using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SimplifiedCore
{
    class Transferer
    {
        private Sender _Sender;
        private Receiver _Receiver;

        private Thread _TransferThread;

        private bool _IsRunning;
        private ReaderWriterLock _IsRunningRWLock;

        private void _TransferRoutine()
        {
            byte[] data = new byte[1024];

            _IsRunningRWLock.AcquireReaderLock(100);
            while (_IsRunning)
            {
                _IsRunningRWLock.ReleaseReaderLock();

                _Sender.GetData(data);
                _Receiver.DispatchData(data);

                _IsRunningRWLock.AcquireReaderLock(100);
            }
            _IsRunningRWLock.ReleaseReaderLock();
        }

        public bool OpenTransfer()
        {
            if (_TransferThread.IsAlive)
            {
                return false;
            }

            _IsRunningRWLock.AcquireWriterLock(100);
            _IsRunning = true;
            _IsRunningRWLock.ReleaseWriterLock();

            _TransferThread.Start();

            return true;
        }

        // TODO: Add safe closing
        // It would be like change
        // _IsRunning variable to false,
        // give it a second or less
        // time limit and if it does not
        // stop after that, close forcefully.
        // Or not.
        public bool CloseTransfer()
        {
            if (!_TransferThread.IsAlive)
            {
                return false;
            }

            _IsRunningRWLock.AcquireWriterLock(100);
            _IsRunning = false;
            _IsRunningRWLock.ReleaseWriterLock();

            _TransferThread.Abort();

            return true;
        }

        public bool MatchCheck(Sender sender, Receiver receiver)
        {
            return ((sender == _Sender) && (receiver == _Receiver));
        }

        public Transferer(Sender sender, Receiver receiver)
        {
            _Sender = sender;
            _Receiver = receiver;

            _TransferThread = new Thread(_TransferRoutine);

            _IsRunning = false;
            _IsRunningRWLock = new ReaderWriterLock();
        }
    }
}
