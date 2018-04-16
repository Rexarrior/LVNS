using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using EntitiesFabrics;
using System.IO;
using NLog;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;



namespace RobotsInterfaces
{
    /// <summary>
    /// This class allows you to retrieve the IP Address and Host Name for a specific machine on the local network when you only know it's MAC Address.
    /// </summary>
    public class IPInfo
    {
        public IPInfo(string macAddress, string ipAddress)
        {
            this.MacAddress = macAddress;
            this.IPAddress = ipAddress;
        }

        public string MacAddress { get; private set; }
        public string IPAddress { get; private set; }

        private string _HostName = string.Empty;
        public string HostName
        {
            get
            {
                if (string.IsNullOrEmpty(this._HostName))
                {
                    try
                    {
                        // Retrieve the "Host Name" for this IP Address. This is the "Name" of the machine.
                        this._HostName = Dns.GetHostEntry(this.IPAddress).HostName;
                    }
                    catch
                    {
                        this._HostName = string.Empty;
                    }
                }
                return this._HostName;
            }
        }


        #region "Static Methods"

        /// <summary>
        /// Retrieves the IPInfo for the machine on the local network with the specified MAC Address.
        /// </summary>
        /// <param name="macAddress">The MAC Address of the IPInfo to retrieve.</param>
        /// <returns></returns>
        public static IPInfo GetIPInfo(string macAddress)
        {
            var ipinfo = (from ip in IPInfo.GetIPInfo()
                          where ip.MacAddress.ToLowerInvariant() == macAddress.ToLowerInvariant()
                          select ip).FirstOrDefault();

            return ipinfo;
        }

        /// <summary>
        /// Retrieves the IPInfo for All machines on the local network.
        /// </summary>
        /// <returns></returns>
        public static List<IPInfo> GetIPInfo()
        {
            try
            {
                var list = new List<IPInfo>();

                foreach (var arp in GetARPResult().Split(new char[] { '\n', '\r' }))
                {
                    // Parse out all the MAC / IP Address combinations
                    if (!string.IsNullOrEmpty(arp))
                    {
                        var pieces = (from piece in arp.Split(new char[] { ' ', '\t' })
                                      where !string.IsNullOrEmpty(piece)
                                      select piece).ToArray();
                        if (pieces.Length == 3)
                        {
                            list.Add(new IPInfo(pieces[1], pieces[0]));
                        }
                    }
                }

                // Return list of IPInfo objects containing MAC / IP Address combinations
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("IPInfo: Error Parsing 'arp -a' results", ex);
            }
        }

        /// <summary>
        /// This runs the "arp" utility in Windows to retrieve all the MAC / IP Address entries.
        /// </summary>
        /// <returns></returns>
        private static string GetARPResult()
        {
            Process p = null;
            string output = string.Empty;

            try
            {
                p = Process.Start(new ProcessStartInfo("arp", "-a")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                });

                output = p.StandardOutput.ReadToEnd();

                p.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("IPInfo: Error Retrieving 'arp -a' Results", ex);
            }
            finally
            {
                if (p != null)
                {
                    p.Close();
                }
            }

            return output;
        }

        #endregion
    }



    public static class WifiManager
    {


        static private void _execute(string fileName)
        {
            var proc = new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = fileName,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(proc);
        }

        static public void WifiStart()
        {
            _execute("wifistart.bat");
        }



        static public void WifiStop()
        {
            _execute("wifistop.bat");
        }



        static public List<IPInfo> GetClients()
        {

            return IPInfo.GetIPInfo();
           

            
        }

    }




    public static class RemoteController
    {
        private const string NETWORK_NAME = "controlNetwork";
        const int PORT = 5555;
        const int PULSETIME = 100; 


        private const string CONFIG_FILE_NAME = "macs.config";

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static Task _listenTask;
        private static Task _receiveTask;
        private static Task _initTask;
        private static UdpClient _udpClient;



        public static Dictionary<string, string> ActiveRobots;

        public static List<string> MacAdresses;

        public static Stack<Command> CommandsToSend;



        private static void _listenAction()
        {
            if (!_initTask.IsCompleted)
                Thread.Sleep(10);
            while (true)
            {
                List<IPInfo> clients = WifiManager.GetClients();

                foreach (var client in clients)
                {

                    if (MacAdresses.Contains(client.MacAddress) && !ActiveRobots.ContainsKey(client.MacAddress))
                        ActiveRobots.Add(client.MacAddress, client.IPAddress); 
                }
                Thread.Sleep(PULSETIME);
            }
        }




        public static void Init()
        {
            ActiveRobots = new Dictionary<string, string>();
            MacAdresses = new List<string>();
            _initTask = new Task(delegate ()
            {
                logger.Info("Initializing...");
                RemoteController.CommandsToSend = new Stack<Command>();
                try
                {
                    StreamReader reader = new StreamReader(new FileStream(CONFIG_FILE_NAME, FileMode.Open, FileAccess.Read));
                    while (!reader.EndOfStream)
                    {
                        string ln = reader.ReadLine();
                        MacAdresses.Add(ln);
                    }
                }
                catch (Exception e)
                {
                    logger.Error("Config file of  interface have been corrupted. Reading aborted. Exception: {0}", e.Message);

                }
                logger.Info("Mac adreses has readed successfull");


                _udpClient = new UdpClient();
                WifiManager.WifiStart();
                _listenTask = new Task(_listenAction);
                _listenTask.Start();
                _receiveTask = new Task(_receiveAction);
                _receiveTask.Start();
                logger.Info("Wifi has started successfuk. Initializing has completed. ");

            });
            _initTask.Start();
        }








        private static void _receiveAction()
        {
            while (true)
            {
                while (CommandsToSend.Count > 0)
                {
                    Command command = CommandsToSend.Pop();
                    string mac = command.Id; 

                    IPEndPoint adress;
                    if (ActiveRobots.ContainsKey(mac))
                    {
                        adress = new IPEndPoint(
                            IPAddress.Parse(RemoteController.ActiveRobots[mac]), PORT);
                       
                        _udpClient.Send(command.Message, command.Message.Length, adress);
                    }
                    else
                    {
                        logger.Error("The  message was received is directed to unconnected target. ");
                    }


                }
                Thread.Sleep(PULSETIME);
            }

        }



        public static void Destroy()
        {
            WifiManager.WifiStop();
            _udpClient.Dispose();
        }

    }





    
}
