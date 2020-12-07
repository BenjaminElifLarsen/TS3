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
        static void Main(string[] args)
        {
            Test();
        }

        static void Test()
        {
            TcpClient tcpClient = new TcpClient("127.0.0.1", 25639);

            Console.WriteLine("Auth");
            NetworkStream networkStream = tcpClient.GetStream();
            string message = "auth apikey=6R6Z-74TE-59CF-MRJ5-2RO2-HOD9\n"; //Those \n are needed from what I have read
            byte[] data = Encoding.ASCII.GetBytes(message);
            networkStream.Write(data, 0, data.Length);
            Thread.Sleep(1000);
            data = new byte[256];
            int bytes = networkStream.Read(data, 0, data.Length);
            Console.WriteLine(Encoding.ASCII.GetString(data, 0, bytes));
            Console.WriteLine();

            //Console.WriteLine("Help");
            //message = "help clientvariable client_unique_Identifier\n"; //find help on a specific function
            //data = Encoding.ASCII.GetBytes(message);
            //networkStream.Write(data, 0, data.Length);
            //Thread.Sleep(1000); //these delays are present to ensure that there are data to read
            //data = new byte[2048];
            //bytes = networkStream.Read(data, 0, data.Length);
            //Console.WriteLine(Encoding.ASCII.GetString(data, 0, bytes));
            //Console.WriteLine();

            Console.WriteLine("Connection"); //will return a string that can be used to find the different client IDs
            message = "connect address=10.11.5.197 password=+cFwYoa5 nickname=ClientQuery\n";
            data = Encoding.ASCII.GetBytes(message);
            networkStream.Write(data, 0, data.Length);
            Thread.Sleep(1000);
            data = new byte[256];
            bytes = networkStream.Read(data, 0, data.Length);
            Console.WriteLine(Encoding.ASCII.GetString(data, 0, bytes));
            Console.WriteLine();

            //Console.WriteLine("Client List"); //will return a string that can be used to find the different client IDs
            //message = "clientlist\n";
            //data = Encoding.ASCII.GetBytes(message);
            //networkStream.Write(data, 0, data.Length);
            //Thread.Sleep(1000);
            //data = new byte[256];
            //bytes = networkStream.Read(data, 0, data.Length);
            //Console.WriteLine(Encoding.ASCII.GetString(data, 0, bytes));
            //Console.WriteLine();

            //Console.WriteLine("Unique Identifier"); //get the unique identifier for client 1.
            //message = "clientvariable clid=4 client_unique_identifier\n";
            //data = Encoding.ASCII.GetBytes(message);
            //networkStream.Write(data, 0, data.Length);
            //Thread.Sleep(1000);
            //data = new byte[256];
            //bytes = networkStream.Read(data, 0, data.Length);
            //Console.WriteLine(Encoding.ASCII.GetString(data, 0, bytes));
            //Console.WriteLine();

            //Console.WriteLine("Flag"); //get the flag for client 1
            //message = "clientvariable clid=2 client_flag_talking\n";
            //data = Encoding.ASCII.GetBytes(message);
            //networkStream.Write(data, 0, data.Length);
            //Thread.Sleep(1000);
            //data = new byte[256];
            //bytes = networkStream.Read(data, 0, data.Length);
            //Console.WriteLine(Encoding.ASCII.GetString(data, 0, bytes));

            string[] test = SerialPort.GetPortNames();
            SerialPort serialPort = new SerialPort(test[0], 9600);
            serialPort.Open();


            Console.WriteLine("Loop");
            while (true)
            {
                Console.WriteLine(Environment.NewLine + "Client List"); //will return a string that can be used to find the different client IDs
                message = "clientlist -voice\n"; //"clientlist \n"
                data = Encoding.ASCII.GetBytes(message);
                networkStream.Write(data, 0, data.Length);
                Thread.Sleep(250);
                data = new byte[2048]; //Need to read while data is availible
                List<byte> bytelist = new List<byte>();
                byte[] smallArray = new byte[1];
                while (networkStream.DataAvailable)
                {
                    networkStream.Read(smallArray, 0, smallArray.Length);
                    bytelist.Add(smallArray[0]);
                }
                //bytes = networkStream.Read(data, 0, data.Length);
                string info = Encoding.ASCII.GetString(bytelist.ToArray(), 0, bytelist.Count);
                string[] users = info.Split('|');

                foreach(string user in users) 
                {
                    string userID = user.Split(' ')[0];
                    Console.Write(userID + " ");
                    Console.WriteLine(user.Split(' ')[5]);
                    //message = $"clientvariable {userID} client_flag_talking\n";
                    //data = Encoding.ASCII.GetBytes(message);
                    //networkStream.Write(data, 0, data.Length);
                    //Thread.Sleep(250);
                    //data = new byte[256];
                    //bytes = networkStream.Read(data, 0, data.Length);
                    //string talkingInfo = Encoding.ASCII.GetString(data, 0, bytes);

                    //Console.WriteLine(talkingInfo.Split(' ')[1].Split('\n')[0]);
                }
            }
        }
    }
}
