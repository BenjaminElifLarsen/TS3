using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TS3
{
    class Program
    {
        static string[] information = new string[4];
        static void Main(string[] args)
        {
            if(args.Length >= 0 && args.Length < 4)
            {
                information[0] = "6R6Z-74TE-59CF-MRJ5-2RO2-HOD9";
                information[1] = "10.11.5.197";
                information[2] = "+cFwYoa5";
                information[3] = "ClientQuery";
                Console.WriteLine("TS3.exe key server password userName");
            }
            else if(args.Length == 4)
            {
                information[0] = args[0]; //key
                information[1] = args[1]; //server
                information[2] = args[2]; //password
                information[3] = args[3]; //user name
            }
            Connent();
            
        }

        static void Connent()
        {
            TcpClient tcpClient = new TcpClient("127.0.0.1", 25639);

            Console.WriteLine("Auth");
            NetworkStream networkStream = tcpClient.GetStream();
            string message = $"auth apikey={information[0]}\n"; //argument 0
            //Those \n are needed from what I have read
            byte[] data = Encoding.ASCII.GetBytes(message);
            networkStream.Write(data, 0, data.Length);
            Thread.Sleep(1000);
            data = new byte[256];
            int bytes = networkStream.Read(data, 0, data.Length);
            Console.WriteLine(Encoding.ASCII.GetString(data, 0, bytes));
            Console.WriteLine();


            Console.WriteLine("Establishing Connection"); //will return a string that can be used to find the different client IDs
            message = $"connect address={information[1]} password={information[2]} nickname={information[3]}\n"; //argument 1 2 3
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
            

            string[] test = SerialPort.GetPortNames();
            /*need to get the port the arduino is connected too. Could do something similar to the python program
             * Loop through each port. For each port, connect and transmit a string. If there is no respone after like 1 second
             * Close the connection and try the next port.
             * When a port transmit a response back it will be arudiono. Keep the port open and enter the while loop
            */
            SerialPort serialPort = new SerialPort();
            int position = 0;
            for (int i = 0; i < test.Length; i++)
            {
                serialPort = new SerialPort(test[i], 9600);
                serialPort.Open();
                serialPort.Write("1");
                Thread.Sleep(1500);
                byte[] dataArduino = new byte[1];
                serialPort.ReadTimeout = 2500;
                try
                {
                    serialPort.Read(dataArduino, 0, 1);
                    position = i;
                    Console.WriteLine($"Prøver at forbinde til: {test[i]}");
                    if (dataArduino[0] == 1)
                    {
                        Console.WriteLine("Arudiono var fundet");
                        break;
                    }
                    else if (i == test.Length - 1)
                    {
                        Console.WriteLine("Arduino var ikke fundet. Tryk Enter for at afsluttet programmet.");
                        Console.ReadLine();
                        Environment.Exit(1);
                    }
                }
                catch
                {

                }
                serialPort.Close();
            }
            serialPort = new SerialPort(test[position], 9600);
            serialPort.Open();

            Console.WriteLine("Loop");
            while (true) //move this into a function
            {
                //Console.WriteLine(Environment.NewLine + "Client List"); //will return a string that can be used to find the different client IDs
                message = "clientlist -voice\n"; //"clientlist \n"
                data = Encoding.ASCII.GetBytes(message);
                networkStream.Write(data, 0, data.Length);
                Thread.Sleep(250);
                //Need to read while data is availible
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
                foreach(string user in users) 
                {
                    pos++;
                    string userID = user.Split(' ')[0];
                    //Console.Write(userID + " ");
                    //Console.WriteLine(user.Split(' ')[5]);
                    if (user.Split(' ')[5].Split('=')[1] == "1") {
                        serialPort.Write("1");
                        break;
                    }
                    else if(user.Split(' ')[5].Split('=')[1] == "0" && pos == users.Length)
                    {
                        serialPort.Write("0");
                    }
                }
            }
        }
    }
}
