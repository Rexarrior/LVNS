#region description
/*
This file contain some interfaces  zero generation which are such small
and so often used that I see no reason to devide they.
There are next Interfaces:
 ILoggable, IControlable, IEventBaseUser,
 IPositionBaseUser, IControler, , IEventBase, IPositionBase, 
 IInternalEvent, ICommand, IControlPoint

 */
#endregion

#region includes
using System;
using Journalizization;
using System.Collections.Generic;


#endregion

namespace Core{

    namespace ZeroGenerationInterfaces
    {
        interface ILoggable
        {
            ILogger logger{set; get;}
        }



        /// <summary>
        /// Somebody who can receive cont
        /// </summary>
        interface IControlable
        {
            void takeCommand(ICommand command); 

        }



        /// <summary>
        /// Somebody who need to operate with events base
        /// </summary>
        interface IEventBaseUser
        {
            IEventBase eventBase{set;}
        }




        /// <summary>
        /// Somebody who need to read from position base
        /// </summary>
        interface IPositionBaseReader
        {
            IPositionBase IPositionBase{set;} 
        }




        /// <summary>
        /// Somebody, who need to write to position base.  Usually it is the PSD. 
        /// </summary>
        interface IPositionBaseOwner
        {
            IPositionBase IPositionBase{set;}         
        }



        /// <summary>
        /// Somebody who need to manage something another module
        /// </summary>
        interface IControler
        {
            void sendCommand(IControlable target, ICommand command)
        }



        /// <summary>
        /// Base for any events happines in core of LVNS
        /// </summary>
        interface IEventBase
        {
            ICollection<Pair<DataTime, IInternalEvent>> Events{get;}
            void append(IInternalEvent);     
        }




        /// <summary>
        /// Command message for manage any component of LVNS. 
        /// Think:
        /// 1. On Command exemplar may consist of one elementary command or
        ///    several elementary command with delimiter.
        /// 2. As a format of elementary message may be used some ID values
        ///    which is references to a dictionarry of elementary
        ///    command values in kernel or string-name. Perhaps, string values only need
        ///    for journal or error message.   
        /// </summary>
        interface ICommand
        {
            string value; 

            bool isCorrect
            {get;} 

            void Verify(ICollection<string> correctCommands);

        }


        /// <summary>
        /// Base point of any control, primitive to implement without processing.
        /// </summary>
        interface IControlPoint
        {
            string ID{set; get;}
        }


        /// <summary>
        /// Any event in Core.
        /// </summary>
        interface IInternalEvent
        {
            string sender{get; set;} 
            DataTime time{get; set;} 
            bool IsProcessed{get; set;}
        }

    


    }
}