using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using System.Diagnostics;

namespace SimplifiedCoreExternalInterface
{
    public delegate void Receive_Delegate( byte[] data );
    public delegate void Load_Delegate( byte[] buffer );

    public static class Methods
    {
        private class Entity
        {
            public UInt32 ID;
            public bool IsReceiver;

            public String SelfMID;
            public String AcceptsMID;

            public Receive_Delegate ReceiveRoutine;
            public Load_Delegate LoadRoutine;

            public Entity(
                UInt32 id,
                bool isReceiver,
                String selfMid,
                String acceptsMid,
                Receive_Delegate receiveDelegate,
                Load_Delegate loadDelegate
                )
            {
                ID = id;
                IsReceiver = isReceiver;
                SelfMID = selfMid;
                AcceptsMID = acceptsMid;

                ReceiveRoutine = receiveDelegate;
                LoadRoutine = loadDelegate;
            }
        }

        private static List<Entity> _Entities = new List<Entity>();

        private static object _HangAcceptRoot = new object();
        private static bool _DoHangAccept = true;

        private static int _AreWeTerminating = 0;

        private static ReaderWriterLock _EntitiesSynchronizer = new ReaderWriterLock();

        private static void Receive0( byte[] data )
        {
            // DON
        }

        private static void Load0( byte[] buffer )
        {
            int bufferIterator = 0;
            byte[] huy = Encoding.ASCII.GetBytes("C:\\Feast\\Music\\Eskobar.wav");
            foreach (byte c in huy)
            {
                buffer[bufferIterator++] = c;
            }
        }

        private static void Receive1( byte[] data )
        {
            if (data[0] == 1)
            {
                Process.Start( "C:\\Windows\\system32\\notepad.exe" );
            }
        }

        private static void Load1( byte[] buffer )
        {
            // DON
        }

        private static void Receive2( byte[] data )
        {
            // DON
        }

        private static void Load2( byte[] buffer )
        {
            buffer[1] = 32;
            buffer[2] = 54;
            buffer[3] = 85;
            buffer[4] = 108;
            buffer[5] = 91;
        }

        private static Entity GetEntity(UInt32 id)
        {
            Entity entity;

            _EntitiesSynchronizer.AcquireReaderLock( 10 );

            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return null; // we are terminating. return.
            }

            if (_Entities.Count <= id || (_Entities[(int)id] == null))
            {
                entity = null;
            }
            else
            {
                entity = _Entities[(int)id];
            }

            return entity;
        }

        public static UInt32 AcceptConnection()
        {
            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return 0; // we are terminating. return.
            }

            UInt32 retval = 0;

            _EntitiesSynchronizer.AcquireWriterLock(10);

            // this check can pass if we somehow reach
            // the previous line after Terminate acquired
            // the writer lock and before _AreWeTerminating
            // was set to 1
            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                _EntitiesSynchronizer.ReleaseWriterLock();
                return 0; // we are terminating. return
            }

            // this check is error-safe
            // because CloseConnction only
            // assigns a corresponding reference
            // to null

            if (_Entities.Count == 3)
            {
                _EntitiesSynchronizer.ReleaseWriterLock();
                // hang & wait for pulse from termination
                lock (_HangAcceptRoot)
                {
                    if (_DoHangAccept)
                    {
                        Monitor.Wait( _HangAcceptRoot );
                    }
                }

                return 0;
            }
            else
            {
                int count = _Entities.Count;

                Entity newEntity;

                switch (count)
                {
                    case 0:
                        newEntity = new Entity(
                            0,
                            false,
                            "Master's level savvy",
                            "Keel Haul",
                            Receive0,
                            Load0
                            );
                        break;
                    case 1:
                        newEntity = new Entity(
                            1,
                            true,
                            "I know the ropes",
                            "Word of mouth",
                            Receive1,
                            Load1
                            );
                        break;
                    case 2:
                        newEntity = new Entity(
                            2,
                            false,
                            "You're off to the races",
                            "Put your ass on the line",
                            Receive2,
                            Load2
                            );
                        break;
                    default:
                        newEntity = null;
                        break;
                }

                _Entities.Add( newEntity );

                retval = newEntity.ID;

                _EntitiesSynchronizer.ReleaseWriterLock();
            }

            return retval;
        }

        public static UInt32 IsReceiver(UInt32 id)
        {
            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return 0; // we are terminating. return
            }

            Entity entity = GetEntity(id);

            if (entity != null) // this is possible if connection to the entity was closed
            {
                UInt32 retval = (entity.IsReceiver ? 1U : 0U);
                _EntitiesSynchronizer.ReleaseReaderLock();
                return retval;
            }
            else
            {
                _EntitiesSynchronizer.ReleaseReaderLock();
                return 0;
            }
        }

        public static UInt32 MatchMID(UInt32 id, string mid)
        {
            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return 0; // we are terminating. return.
            }

            Entity entity = GetEntity(id);

            if (entity != null) // this is possible if connection to the entity was closed
            {
                UInt32 retval = (entity.AcceptsMID == mid ? 1U : 0U);
                _EntitiesSynchronizer.ReleaseReaderLock();
                return retval;
            }
            else
            {
                _EntitiesSynchronizer.ReleaseReaderLock();
                return 0;
            }
        }

        public static void GetMID(UInt32 id, StringBuilder matchIDBuffer)
        {
            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return; // we are terminating. return
            }

            Entity entity = GetEntity(id);

            if (entity == null)
            {
                _EntitiesSynchronizer.ReleaseReaderLock(); // tut vashe pohuy, do ili posle
                matchIDBuffer.Append( "" );
            }
            else
            {
                matchIDBuffer.Append( entity.SelfMID );

                _EntitiesSynchronizer.ReleaseReaderLock();
            }
        }

        public static void GetData(UInt32 id, byte[] dataBuffer)
        {
            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return; // we are terminating. return
            }

            Entity entity = GetEntity(id);

            if (entity == null)
            {
                _EntitiesSynchronizer.ReleaseReaderLock();
                return;
            }
            else
            {
                entity.LoadRoutine( dataBuffer );
                _EntitiesSynchronizer.ReleaseReaderLock();
            }
        }

        public static void DispatchData(UInt32 id, byte[] data)
        {
            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return; // we are terminating. return
            }

            Entity entity = GetEntity(id);

            if (entity == null)
            {
                _EntitiesSynchronizer.ReleaseReaderLock();
                return;
            }
            else
            {
                entity.ReceiveRoutine(data);
                _EntitiesSynchronizer.ReleaseReaderLock();
            }
        }

        public static void CloseConnection( UInt32 id )
        {
            return;
            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return; // we are terminating. return
            }

            _EntitiesSynchronizer.AcquireWriterLock(10);

            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                _EntitiesSynchronizer.ReleaseWriterLock();
                return; // we are terminating. return
            }

            if (_Entities.Count <= id)
            {
                _Entities[(int)id] = null;
            }

            _EntitiesSynchronizer.ReleaseWriterLock();
        }

        public static void StopAccepting()
        {
            lock (_HangAcceptRoot)
            {
                _DoHangAccept = false;
                Monitor.Pulse(_HangAcceptRoot);
            }
        }

        public static void Terminate()
        {
            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 0) == 0)
            {
                return; // already terminating
            }

            _EntitiesSynchronizer.AcquireWriterLock(10);

            for (uint i = 0; i < _Entities.Count; i++)
            {
                _Entities[(int)i] = null;
            }

            _Entities.Clear();

            _EntitiesSynchronizer.ReleaseWriterLock();
        }
    }
}
