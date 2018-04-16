using EntitiesFabrics;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System;
using System.Linq;
using EntitiesFabrics.Analiziers;

namespace RobotsInterfaces
{



    [PartNotDiscoverable()]
    [Export(typeof(IEntity))]
    public class Matlab : Entity
    {


        private const string scriptPath = ".\\scripts\\GoToPoint.m";
        private const string scriptName = "GoToPoint";


        public Dictionary<uint, PointDouble> DestinationPoints;
        

        public override void Receive(byte[] data)
        {
            Position pos = Position.Parser.ParseFrom(data);



            foreach (var item in pos.Units)
            {

                object result;
                if (!MatlabWrapper.Invoke(scriptName, out result, item.X, item.Y, item.Orientation,
                                                     DestinationPoints[item.Id].X,
                                                     DestinationPoints[item.Id].Y,
                                                     DestinationPoints[item.Id].Angle))
                    logger.Error("ERROR!Cant invoke script for {0} unit. ({1} , {2}, {3} => ({4} , {5}, {6} )",
                        item.Id, item.X, item.Y, item.Orientation, DestinationPoints[item.Id].X, DestinationPoints[item.Id].Y, DestinationPoints[item.Id].Angle);


                int[] speeds = result as int[];

                Int16 speed1 = (Int16)speeds[0];
                Int16 speed2 = (Int16)speeds[0];


                byte[] resArr = new byte[16];
                BitConverter.GetBytes(speed1).CopyTo(resArr, 0);
                BitConverter.GetBytes(speed2).CopyTo(resArr, 8);

                RemoteController.CommandsToSend.Push(
                    new Command(RemoteController.MacAdresses[(int)item.Id], resArr)); 

            }
        }




        public override void Shutdown()
        {
            base.Shutdown();
            RemoteController.Destroy();
        }


        [ImportingConstructor]
        public Matlab() : base("Matlab", "ProtoBuffReceiver", true)
        {
            DestinationPoints = new Dictionary<uint, PointDouble>();
            DestinationPoints.Add(0, new PointDouble(0, 0, 0));
            MatlabWrapper.AddScript(scriptName, scriptPath);
            logger.Info("Matlab entity is creating...");
            RemoteController.Init();

        }



        ~Matlab()
        {
            Shutdown();
        }
    }




}
