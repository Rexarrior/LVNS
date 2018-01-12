using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.Threading;

namespace SimplifiedCore
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate UInt32 AcceptConnection_Delegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    // this is actually bool, not int, but dll's bool and C#'s bool
    // seem to be incompatible, so we use UInt32
    public delegate UInt32 IsReceiver_Delegate( UInt32 id );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GetMID_Delegate( UInt32 id, StringBuilder matchIDBuffer );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    // this is actually bool, not int, but dll's bool and C#'s bool
    // seem to be incompatible, so we use UInt32
    public delegate UInt32 MatchMID_Delegate( UInt32 id, string matchID );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GetData_Delegate( UInt32 id, byte[] dataBuffer );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DispatchData_Delegate( UInt32 id, byte[] data );

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void Close_Delegate();

    /*
     * Represents an interface (with a corresponding DLL)
     * that sustains Receivers and Senders functionality
     * 
     * TODO: Make it IExternalInterface, implying that
     * there are two kinds of ExternalInterfaces, since
     * there are two kinds of .dlls - managed and unmanaged
     */
    class ExternalInterface : IDisposable
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary( string dllPath );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress( IntPtr dllHandle, string procName );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary( IntPtr dllHandle );

        private IntPtr _DllHandle;

        
        private IntPtr _AcceptConnectionPtr;

        // the function used to accept
        // those who connect via this interface
        //
        // we create delegate for the funciton right here,
        // because this interface is the only motherfucker
        // who's gonna use it and it's not enthreaded
        private AcceptConnection_Delegate AcceptConnectionRoutine;

        private IntPtr _ClosePtr;

        private Close_Delegate CloseRoutine;

        /*
         * The funcitons, used to communicate
         * with an external entity
         */
        private IntPtr _IsReceiverPtr;
        private IntPtr _GetDataPtr;
        private IntPtr _DispatchDataPtr;
        private IntPtr _GetMIDPtr;
        private IntPtr _MatchMIDPtr;

        private bool _IsTerminating;

        private ReaderWriterLock _IsTerminatingRWLock;

        public ExternalInterface(String dllPath)
        {
            _DllHandle = LoadLibrary( dllPath );

            _IsReceiverPtr = GetProcAddress( _DllHandle, "IsReceiver" );
            _GetDataPtr = GetProcAddress(_DllHandle, "GetData");
            _DispatchDataPtr = GetProcAddress(_DllHandle, "DispatchData");
            _GetMIDPtr = GetProcAddress(_DllHandle, "GetMID");
            _MatchMIDPtr = GetProcAddress(_DllHandle, "MatchMID");
            _AcceptConnectionPtr = GetProcAddress(_DllHandle, "AcceptConnection");
            _ClosePtr = GetProcAddress(_DllHandle, "Close");

            AcceptConnectionRoutine = (AcceptConnection_Delegate)
                Marshal.GetDelegateForFunctionPointer( _AcceptConnectionPtr, typeof(AcceptConnection_Delegate) );

            CloseRoutine = (Close_Delegate)
                Marshal.GetDelegateForFunctionPointer( _ClosePtr, typeof(Close_Delegate) );

            _IsTerminating = false;

            _IsTerminatingRWLock = new ReaderWriterLock();
        }
        /*
         * Supposed to call a DLL function, that
         * accepts connection and returns an id
         * that the dll uses to identify the entity
         * and then 
         */
        public ExternalEntity AcceptConnection()
        {
            // if we are terminating, quit right now
            _IsTerminatingRWLock.AcquireReaderLock(100);
            if (_IsTerminating == true)
            {
                _IsTerminatingRWLock.ReleaseReaderLock();
                return null;
            }
            _IsTerminatingRWLock.ReleaseReaderLock();

            UInt32 newID = AcceptConnectionRoutine();

            // if we are terminating, quit right now
            _IsTerminatingRWLock.AcquireReaderLock(100);
            if (_IsTerminating == true)
            {
                _IsTerminatingRWLock.ReleaseReaderLock();
                return null;
            }
            _IsTerminatingRWLock.ReleaseReaderLock();

            // We create a new instance of delegate
            // each time because we don't want access
            // conflicts, that are possible, since we
            // may have more than one external entity.
            // The very thread-safety of DLL functions
            // must be maintained by those, who code the DLL.     
            return new ExternalEntity(
                newID,
                (IsReceiver_Delegate)Marshal.GetDelegateForFunctionPointer( _IsReceiverPtr, typeof(IsReceiver_Delegate) )
                );
        }

        public void PassMIDDelegates( IDefinedEntity entity )
        {
            entity.AcceptMIDDelegates(
                (GetMID_Delegate)Marshal.GetDelegateForFunctionPointer( _GetMIDPtr, typeof(GetMID_Delegate) ),
                (MatchMID_Delegate)Marshal.GetDelegateForFunctionPointer( _MatchMIDPtr, typeof(MatchMID_Delegate) )
                );
        }

        public void PassGetDataDelegateToSender( Sender sender )
        {
            sender.AcceptGetDataDelegate(
                (GetData_Delegate)Marshal.GetDelegateForFunctionPointer( _GetDataPtr, typeof(GetData_Delegate) )
                );
        }

        public void PassDispatchDataDelegateToReceiver( Receiver receiver )
        {
            receiver.AcceptDispatchDataDelegate(
                (DispatchData_Delegate)Marshal.GetDelegateForFunctionPointer(_DispatchDataPtr, typeof(DispatchData_Delegate))
                );
        }

        public void Terminate()
        {
            _IsTerminatingRWLock.AcquireWriterLock(100);

            _IsTerminating = true;

            _IsTerminatingRWLock.ReleaseWriterLock();

            CloseRoutine();
        }

        public void Dispose()
        {
            FreeLibrary( _DllHandle );
        }
    }
}
