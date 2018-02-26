using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using JsonServer;
using Newtonsoft.Json;

namespace CoordinateGenerator
{

    
      




    class Map
    {
        private int _width;
        private int _heigth;

        public int Width { get => _width; set { _width = value; } }


        public int Height { get => _heigth; set { _heigth = value; } }


        public Random rnd = new Random(DateTime.Now.Millisecond);


        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            rnd = new Random(DateTime.Now.Millisecond);
        }


       public Unit GenRandomPosition()
        {

            return new Unit(0, rnd.Next(0, 360), rnd.Next(0, Width), rnd.Next(0, Height));

        }


    }




    class Program
    {


        static public int ReadInt(string prompt)
        {
            Console.Write(prompt);
            string temp = Console.ReadLine();
            int res;
            while (!int.TryParse(temp, out res))
            {
                Console.Write(prompt);
                temp = Console.ReadLine();
            }
            return res;
        }





        static void Main(string[] args)
        {


            
            int width = ReadInt("Enter map width:");


            int height = ReadInt("Enter map height:")  ;

            int sleepTime = ReadInt("Enter send time in miliseconds: ");

            int Count = ReadInt("Write count of units: ");
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);


            IPEndPoint target = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);
            Map map = new Map(width, height);
            try
            {
                while (true)
                {
                    for (int i = 0; i < Count; i++)
                    {
                        Unit rndUnit = map.GenRandomPosition();
                        rndUnit.ID = i;
                        string strUnit = JsonConvert.SerializeObject(rndUnit);
                        Console.WriteLine("{0} sending....");
                        byte[] data = new byte[1024];
                        Encoding.Unicode.GetBytes(strUnit, 0, strUnit.Length, data, 0);
                        socket.SendTo(data, target);
                    }

                    Thread.Sleep(sleepTime);
                }

            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }


        }
    }
}
