using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Threading;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using EntitiesFabrics; 

namespace SimplifiedCoreExternalInterface
{





    class Methods
    {
        private const int LOCK_TIME = 10000; 


        #region fields
        public const string EXTENSIONS_DIRECTORY = "ExtensionInterfaces";
        private  CompositionContainer _container;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [ImportMany(typeof(IEntity))]
        private List<Lazy<Entity>> _foundedEntities { get; set; }

        private List<IEntity> _activeEntities = new List<IEntity>();

        private static object _HangAcceptRoot = new object();
        private static bool _DoHangAccept = true;

        private static int _AreWeTerminating = 0;

        private static ReaderWriterLock _EntitiesSynchronizer = new ReaderWriterLock();

        private static Methods _instance;

        #endregion



        #region methods

        public static void Initialize()
        {
            logger.Info("Initializing...");
            _instance = new Methods(); 

            if (!Directory.Exists(EXTENSIONS_DIRECTORY))
            {
                Directory.CreateDirectory(EXTENSIONS_DIRECTORY);
                logger.Info("Directory for extensions has been created.");
            }


            //An aggregate catalog that combines multiple catalogs  
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class  
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Methods).Assembly));

            catalog.Catalogs.Add(new DirectoryCatalog(Directory.GetCurrentDirectory() + "\\" + EXTENSIONS_DIRECTORY));



            _instance._container = new CompositionContainer(catalog);

            try
            {
                CompositionBatch batch = new CompositionBatch();
                batch.AddPart(_instance);
                _instance._container.Compose(batch);
                
            }
            catch (Exception e)
            {
                logger.Error("In time of compositioning happend exception with message {0}.",
                    e.Message
                    );
                
            }
            logger.Info("{0} entities finded.", _instance._foundedEntities.Count);
            _instance._activeEntities = new List<IEntity>();
        }



        private static IEntity GetEntity(UInt32 id)
        {

            IEntity entity;

            _EntitiesSynchronizer.AcquireReaderLock(LOCK_TIME );

            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                _EntitiesSynchronizer.ReleaseReaderLock();
                return null; // we are terminating. return.
            }

            if (Methods._instance._activeEntities.Count <= id || (Methods._instance._activeEntities[(int)id] == null))
            {
                entity = null;
            }
            else
            {
                entity = Methods._instance._activeEntities[(int)id];
            }

            _EntitiesSynchronizer.ReleaseReaderLock();
            return entity;
        }



        public static UInt32 AcceptConnection()
        {

           
            if (Methods._instance == null)
                Initialize();

            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return 0; // we are terminating. return.
            }

            UInt32 retval = 0;

            _EntitiesSynchronizer.AcquireWriterLock(LOCK_TIME);

            // this check can pass if we somehow reach
            // the previous line after Terminate acquired
            // the writer lock and before _AreWeTerminating
            // was set to 1
            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                _EntitiesSynchronizer.ReleaseWriterLock();
                return 0; // we are terminating. return
            }

          
            if (Methods._instance._foundedEntities.Count  == 0)
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

                Methods._instance._activeEntities.Add((IEntity)Methods._instance._foundedEntities.First().Value );
                Methods._instance._foundedEntities.RemoveAt(0);
                Methods._instance._activeEntities.Last().ID = (uint)Methods._instance._activeEntities.Count - 1;

                retval = Methods._instance._activeEntities.Last().ID;

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

            IEntity entity = GetEntity(id);

            if (entity != null) // this is possible if connection to the entity was closed
            {
                UInt32 retval = (entity.IsReceiver ? 1U : 0U);
                return retval;
            }
            else
            {
                return 0;
            }
        }



        public static UInt32 MatchMID(UInt32 id, string mid)
        {
          

            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return 0; // we are terminating. return.
            }

            IEntity entity = GetEntity(id);

            if (entity != null) // this is possible if connection to the entity was closed
            {
                UInt32 retval = (entity.AcceptsMID == mid ? 1U : 0U);
                return retval;
            }
            else
            {
                return 0;
            }
        }



        public static void GetMID(UInt32 id, StringBuilder matchIDBuffer)
        {

            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return; // we are terminating. return
            }

            IEntity entity = GetEntity(id);

            if (entity == null)
            {
                matchIDBuffer.Append( "" );
            }
            else
            {
                matchIDBuffer.Append( entity.SelfMID );

            }
        }



        public static void GetData(UInt32 id, byte[] dataBuffer)
        {
            
            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return; // we are terminating. return
            }

            IEntity entity = GetEntity(id);

            if (entity == null)
            {
                return;
            }
            else
            {
                entity.LoadRoutine( dataBuffer );
            }
        }



        public static void DispatchData(UInt32 id, byte[] data)
        {
            
            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return; // we are terminating. return
            }

            IEntity entity = GetEntity(id);

            if (entity == null)
            {
                return;
            }
            else
            {
                entity.ReceiveRoutine(data);
            }
        }



        public static void CloseConnection( UInt32 id )
        {
            //return;
            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                return; // we are terminating. return
            }

            _EntitiesSynchronizer.AcquireWriterLock(LOCK_TIME);

            if (Interlocked.CompareExchange(ref _AreWeTerminating, 1, 1) == 1)
            {
                _EntitiesSynchronizer.ReleaseWriterLock();
                return; // we are terminating. return
            }

            if (Methods._instance._activeEntities.Count <= id)
            {

                Methods._instance._activeEntities[(int)id].Shutdown();
                Methods._instance._activeEntities.RemoveAt((int)id);
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

            _EntitiesSynchronizer.AcquireWriterLock(LOCK_TIME);

            for (uint i = 0; i < Methods._instance._activeEntities.Count; i++)
            {
                Methods._instance._activeEntities[(int)i].Shutdown();
            }
            Methods._instance._activeEntities.Clear();


            Methods._instance._foundedEntities.Clear();

            _EntitiesSynchronizer.ReleaseWriterLock();
        }



        #endregion
    }



#if SAMPLES

    [Export(typeof(IEntity))]
    class SampleReceiver : Entity
    {
    #region fields
        public const string LISTEN_IP = "127.0.0.1";
        public const int PORT = 5555;
        
        private Socket _socket;
        private Task _listenTask;

    #endregion


    #region methods
        public override void Load(byte[] data)
        {
           
            if (_listenTask.IsCompleted)
                _listenTask = new Task(_listenAction);

            if (
               _listenTask.Status == TaskStatus.Created
                )
            {
                _listenTask.Start();
            }
        }


        private void _listenAction()
        {

            try
            {

                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                byte[] data = new byte[256];


                EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

                do
                {
                    bytes = _socket.ReceiveFrom(data, ref remoteIp);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (_socket.Available > 0);

                IPEndPoint remoteFullIp = remoteIp as IPEndPoint;


                logger.Info("{0}:{1} - {2}", remoteFullIp.Address.ToString(),
                                                remoteFullIp.Port, builder.ToString());
                Thread.Sleep(60000);

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                Shutdown();
            }
        }





        public override void  Shutdown()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();
            logger.Info("Entity id {0} has shutdowned.", _id);
        }

    #endregion



        [ImportingConstructor]
        public SampleReceiver(): base("SampleReceiver", "SampleSender", false)
        {
           
            logger.Info("Sample receiver screated");

            _listenTask = new Task(_listenAction);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse(LISTEN_IP), PORT));

            _loadDelegate = this.Load;
            
        }
    }



    [Export(typeof(IEntity))]
     class SampleSender : Entity
    {
    #region fields
        public const string TARGET_IP = "127.0.0.1";
        public const int PORT = 5555;


       
        private Socket _socket;
        private Task _sendTask;
    #endregion



    #region methods
     


        private void _sendAction()
        { 
            try
            {

                _socket.SendTo(
                    Encoding.Unicode.GetBytes(
                    DateTime.Now.ToShortTimeString()),
                    new IPEndPoint(IPAddress.Parse(TARGET_IP), PORT));

                Thread.Sleep(1000);

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                Shutdown();
            }

        }


        public  override void Receive(byte[] data)
        {
          
            if (_sendTask.IsCompleted)
                _sendTask = new Task(_sendAction);
            if (
               _sendTask.Status == TaskStatus.Created
                )
            {
                _sendTask.Start();
            }


        }


        public override void Shutdown()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();
            logger.Info("Entity id {0} has shutdowned.", _id);
        }

    #endregion




        [ImportingConstructor]
        public SampleSender():base("SampleSender", "SampleReceiver", true)
        {
            logger.Info("Sample sender created");
            _sendTask = new Task(_sendAction);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ///_socket.Bind(new IPEndPoint(IPAddress.Parse(TARGET_IP), PORT));

            _receiveDelegate = Receive;
            
        }
    }


#endif

}
