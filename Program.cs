using SerialPinComander.Communication;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TS3
{
    class Program
    {
        static string[] information = new string[4];

        private static SerialPinCommander spc = SerialPinCommander.GetCommander();
        private static TcpClient tcpClient;
        static void Main(string[] args)
        {
            if(args.Length >= 0 && args.Length < 4)
            {
                //information[0] = "6R6Z-74TE-59CF-MRJ5-2RO2-HOD9";
                //information[1] = "10.11.5.197";
                //information[2] = "+cFwYoa5";
                //information[3] = "ClientQuery";
                Console.WriteLine("TS3.exe key server password userName");
                Console.WriteLine("Tryk Enter for at afsluttet programmet.");
                Console.ReadLine();
            }
            else if(args.Length == 4)
            {
                information[0] = args[0]; //key
                information[1] = args[1]; //server
                information[2] = args[2]; //password
                information[3] = args[3]; //user name
                spc.ConnectionSuccess += SucceededConnection;
                spc.ConnectionFail += FailedConnection;
                ArduinoConnection();
                NetworkStream nws = TS3Connection();
                Transmitter(nws);
            }
            
        }

        /// <summary>
        /// Will try and connection to a TS3 sever using the information in the information array.
        /// 
        /// </summary>
        static NetworkStream TS3Connection()
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 25639);

                Console.WriteLine("Auth");
                NetworkStream networkStream = tcpClient.GetStream();
                string message = $"auth apikey={information[0]}\n"; //argument 0
                //Those \n are needed in the strings messages else TS3 will not react correctly on them
                byte[] data = Encoding.ASCII.GetBytes(message);
                networkStream.Write(data, 0, data.Length);
                Thread.Sleep(1000);
                data = new byte[256];
                int bytes = networkStream.Read(data, 0, data.Length);
                Console.WriteLine(Encoding.ASCII.GetString(data, 0, bytes));
                Console.WriteLine();

                Console.WriteLine("Establishing Connection");
                message = $"connect address={information[1]} " +
                    $"password={information[2]} " +
                    $"nickname={information[3]} " +
                    $"channel={"Vejkanalen"}\n"; //argument 1 2 3
                data = Encoding.ASCII.GetBytes(message);
                networkStream.Write(data, 0, data.Length);
                Thread.Sleep(1000);
                data = new byte[256];
                bytes = networkStream.Read(data, 0, data.Length);
                Console.WriteLine("Received");
                Console.WriteLine(Encoding.ASCII.GetString(data, 0, bytes));
                Console.WriteLine("Hvis der er error ids som ikke er lig med 0, betyder det at programmet ikke har lykkes at oprette forbindelse til serveren." +
                    Environment.NewLine + "Tjek om kodeord og brugernavn er korrekt. " +
                    Environment.NewLine + "Tjek også om brugeren allerede har oprettet forbindelse til serveren. Hvis de har, sluk for forbindelsen til serveren og start dette program igen.");

                return networkStream;
            }
            catch (Exception e)
            {
                Console.WriteLine("Fejlede i at oprette forbindelse til TS3." + Environment.NewLine + "Tjek om TS3 er kørende. Hvis ikke, start programmet, men log ikke på serveren.");
                Console.WriteLine("Fejlbesked:" + Environment.NewLine + e.Message);
                return null;
            }
        }

        private static void Transmitter(NetworkStream networkStream)
        {
            if(networkStream == null)
            {
                Console.WriteLine("Netværkstrøm er nul. Tryk Enter for at afsluttet programmet.");
                Console.ReadLine();
                Environment.Exit(1);
            }
            string message;
            byte[] data;
            while (!spc.IsConnected) { }
            Console.WriteLine("Loop");
            while (true)
            {
                //will return a string that can be used to find the different client IDs
                message = "clientlist -voice\n"; //"clientlist \n"
                data = Encoding.ASCII.GetBytes(message);
                networkStream.Write(data, 0, data.Length);
                Thread.Sleep(250);
                List<byte> bytelist = new List<byte>();
                byte[] smallArray = new byte[1];
                while (networkStream.DataAvailable)
                {
                    networkStream.Read(smallArray, 0, smallArray.Length);
                    bytelist.Add(smallArray[0]);
                }
                string info = Encoding.ASCII.GetString(bytelist.ToArray(), 0, bytelist.Count);
                string[] users = info.Split('|');
                int pos = 0;
                Console.WriteLine($"Fandt {users.Length} brugere");
                foreach (string user in users)
                {
                    pos++;
                    string userID = user.Split(' ')[0];
                    if (user.Split(' ')[5].Split('=')[1] == "1")
                    {
                        spc.State = SerialPinCommander.SerialTS3State.ResiveData;
                        break;
                    }
                    else if (user.Split(' ')[5].Split('=')[1] == "0" && pos == users.Length)
                    {
                        spc.State = SerialPinCommander.SerialTS3State.NoResivingData;
                    }
                }
            }
        }

        public static async Task ArduinoConnection()
        {
            await spc.ConnectAsync();
        }

        private static void FailedConnection(object sender, EventArgs e)
        {
            Console.WriteLine("Arduino blev ikke fundet. Tryk Enter for at afsluttet programmet.");
            Console.ReadLine();
            Environment.Exit(1);
        }

        private static void SucceededConnection(object sender, EventArgs e)
        {
            Console.WriteLine("Arudiono blev fundet");
        }
    }
}
