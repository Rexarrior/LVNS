#region  description
/*
Remoute system dispatcher. Need to transport control stream to any devices without knowledge about the intrface of transporting
 */
#endregion

#region  includes
using System; 
using ZeroGenerationInterfaces;
using System.Collections.Generic; 
#endregion

namespace Core
{
    namespace RSDispatcher
    {
        class RSD: IRemouteSystemDispatcher
        {
            #region private

            private List<IDevice> _devices;
            private List<IRemouteInterface> _interfaces; 
            private Logger _logger; 
            
            #endregion
            
            #region public

        

            #region Properties
        
            Logger ILoggable.Logger {
                
                set
                {
                    if (value is ILogger)
                        this._logger = value;
                    else
                    {
                        throw new ArgumentException("logger isn't ILogger");

                    }
                }
                get
                {
                    return _logger;
                }
            }



            List<IRemouteInterface> IRemouteSystemDispatcher.Interfaces
            {
                set
                {
                    this._interfaces = value;
                }
                get {
                    return Interfaces;
                }
            }


            List<IDevice> IRemouteSystemDispatcher.Devices
            {
                set
                {
                    this._devices = value
                }
                get
                {
                    return devices;
                }
            }
            
            List<InternalEvents> IEventBaseUser.EventsBase
            {
                set
                {   
                    
                    this._eventBase = value; 
                }
            }

            #endregion 
            
            #region methods
            
            void IRemouteSystemDispatcher.sendControlPoint(IDevice target, IControlPoint controlPoint )
            {
                throw new NotImplementedException();
            }



            void IControlable.takeCommand(ICommand command)
            {
                throw new NotImplementedException();
            } 



            void initialize(string pathToInterfacesDir, string pathToDevicesDir, string configFileName)
            {
                throw new NotImplementedException();
            }
            
            #endregion
            
            #region constructors
            
            

            RSD( string pathToInterfacesDir, string pathToDevicesDir, string configFileName))
            {
                this.initialize(pathToInterfacesDir, pathToDevicesDir, configFileName);
            }

            #endregion
            #endregion
            

        }

    }
}