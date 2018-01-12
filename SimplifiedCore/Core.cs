using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace SimplifiedCore
{
    /*
     * When an external entity connects to the core,
     * it is "registered" - i.e put in a corresponding list -
     * either as a receiver (put in list of receivers) or
     * as a sender (put in list of senders).
     * But before getting it into a list, we check if
     * there is a matching entity in the opposite list.
     * If we found one, the current entity is even not gonna
     * get put in its list, because there is no need of it - 
     * no one will connect to it when it is already in a connection.
     * 
     * To match a corresponding entity in a list, MatchMID() function
     * is used. The id is retreived from a connecting entity using
     * GetMID() function.
     * A VERY IMPORTANT NOTICE is that if we found a one-sided match,
     * like receiver.MatchMID( sender.GetMID() ) returned true,
     * WE DO NOT ESTABLISH THE CONNECTION BETWEEN THEM UNTIL
     * OPPOSITE MATCH IS PERFORMED - speaking about this example,
     * it would be sender.MatchMID( receiver.GetMID ) - this must return true
     * or otherwise we skip the pair (and maybe log this event
     * in a base of interesting events)
     */
    class Core
    {
        private List<Tuple<ExternalInterface, Thread>> _ExternalInterfaces;

        private List<Transferer> _Connections;

        private List<Receiver> _WaitingReceivers;
        private List<Sender> _WaitingSenders;

        private bool _IsCoreRunning;

        // because many connections may be accepted
        // simultaneously, we need to control access
        // to the Lists (above) -> so we do lock(_SyncRoot)
        private object _SyncRoot = new object();

        // it is said to be best if
        // we want to write rarely and
        // by single thread and read often
        // by multiple threads
        // So, we write to _IsCoreRunning only three times - 
        // first, when we create an instance of Core, second -
        // when we call Start() (in both of these cases we
        // don't event need the lock) and third - when Core is
        // running and we call Stop() - in this case some thread
        // that Accepts Connections from an external interface
        // may be checking if Core is stopped or not (this is a loop check)
        // so, we need to call AcquireWriterBlock(), change the
        // value to false and call ReleaseWriterBlock()
        private ReaderWriterLock _IsCoreRunningRWLock;

        private void AddTransferConnection(Sender sender, Receiver receiver)
        {
            _Connections.Add(new Transferer(sender, receiver));
            _Connections.Last().OpenTransfer();
        }

        private void RegisterReceiver(Receiver receiver)
        {
            for (int i = 0; i < _WaitingSenders.Count; i++)
            {
                if (_WaitingSenders[i].MatchMID(receiver.GetMID()))
                {
                    // both-sided check
                    if (!receiver.MatchMID(_WaitingSenders[i].GetMID()))
                    {
                        continue;
                    }

                    AddTransferConnection(_WaitingSenders[i], receiver);
                    _WaitingSenders.RemoveAt(i);
                    return;
                }
            }

            // matching sender not found

            _WaitingReceivers.Add(receiver);
        }

        private void RegisterSender(Sender sender)
        {
            for (int i = 0; i < _WaitingReceivers.Count; i++)
            {
                if (_WaitingReceivers[i].MatchMID(sender.GetMID()))
                {
                    // both-sided check
                    if (!sender.MatchMID(_WaitingReceivers[i].GetMID()))
                    {
                        continue;
                    }

                    AddTransferConnection(sender, _WaitingReceivers[i]);
                    _WaitingReceivers.RemoveAt(i);
                    return;
                }
            }

            // matching receiver not found

            _WaitingSenders.Add(sender);
        }

        private void RemoveTransferConnection(Sender sender, Receiver receiver)
        {
            lock (_SyncRoot)
            {
                for (int i = 0; i < _Connections.Count(); i++)
                {
                    if (_Connections[i].MatchCheck(sender, receiver))
                    {
                        _Connections[i].CloseTransfer();
                        _Connections.RemoveAt(i);
                        i--; // assuming that there may be more than one match
                    }
                }
            }
        }

        private void AcceptConnection(ExternalInterface externalInterface)
        {
            ExternalEntity newComer = externalInterface.AcceptConnection();

            if (newComer == null)
            {
                return;
            }

            lock (_SyncRoot)
            {
                if (newComer.IsReceiver())
                {
                    Receiver newReceiver = new Receiver();

                    newComer.PassID(newReceiver);
                    externalInterface.PassMIDDelegates(newReceiver);
                    externalInterface.PassDispatchDataDelegateToReceiver(newReceiver);

                    RegisterReceiver(newReceiver);
                }
                else
                {
                    Sender newSender = new Sender();

                    newComer.PassID(newSender);
                    externalInterface.PassMIDDelegates(newSender);
                    externalInterface.PassGetDataDelegateToSender(newSender);

                    RegisterSender(newSender);
                }

            }
        }

        private void RegisterInterface(String dllPath)
        {
            ExternalInterface externalInterface = new ExternalInterface(dllPath);
            Thread interfaceThread = new Thread(
                delegate ()
                {
                    _IsCoreRunningRWLock.AcquireReaderLock(100);

                    while (_IsCoreRunning)
                    {
                        _IsCoreRunningRWLock.ReleaseReaderLock();

                        AcceptConnection(externalInterface);

                        _IsCoreRunningRWLock.AcquireReaderLock(100);
                    }

                    _IsCoreRunningRWLock.ReleaseReaderLock();
                }
                );

            interfaceThread.Start();

            _ExternalInterfaces.Add( new Tuple<ExternalInterface, Thread>(externalInterface, interfaceThread) );
        }

        public int Start(String configFilePath)
        {
            // supposing now, that there are only DLL paths
            // in the config file

            // We've just started - no need to lock this yet
            _IsCoreRunning = true;

            FileStream configFile = new FileStream(configFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
            StreamReader configReader = new StreamReader(configFile);

            String currentString;

            while (!configReader.EndOfStream)
            {
                currentString = configReader.ReadLine();
                RegisterInterface(currentString);
            }

            configReader.Close();
            configFile.Close();

            return ErrorCodes.ERROR_SUCCESS;
        }

        public int Stop()
        {
            _IsCoreRunningRWLock.AcquireReaderLock(100);

            if (!_IsCoreRunning)
            {
                _IsCoreRunningRWLock.ReleaseReaderLock();

                return ErrorCodes.NOT_RUNNING;
            }

            _IsCoreRunningRWLock.UpgradeToWriterLock(100);

            _IsCoreRunning = false;

            _IsCoreRunningRWLock.ReleaseWriterLock();

            foreach (Tuple<ExternalInterface, Thread> ei in _ExternalInterfaces)
            {
                ei.Item1.Terminate();
            }

            foreach (Transferer transferer in _Connections)
            {
                transferer.CloseTransfer();
            }

            foreach (Tuple<ExternalInterface, Thread> ei in _ExternalInterfaces)
            {
                ei.Item1.Dispose();
            }

            return ErrorCodes.ERROR_SUCCESS;
        }

        public Core()
        {
            _IsCoreRunning = false;

            _Connections = new List<Transferer>();

            _WaitingReceivers = new List<Receiver>();
            _WaitingSenders = new List<Sender>();

            _ExternalInterfaces = new List<Tuple<ExternalInterface, Thread>>();

            _IsCoreRunningRWLock = new ReaderWriterLock();
        }
    }
}
