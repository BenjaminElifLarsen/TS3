using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialPinComander.Communication
{
    /// <summary>
    /// A class to commicate to SerialPinCommand on Arduino Leonardo from TeamSpeek3 Program (TS3)
    /// </summary>
    public class SerialPinCommander
    {
        /// <summary>
        /// This instance of commander for singleton
        /// </summary>
        private static SerialPinCommander serialPinCommander;

        /// <summary>
        /// PortRange for Arduino Leonardo Serial Speed
        /// </summary>
        private const int portRange = 9600;

        /// <summary>
        /// Last resived data from Arduino
        /// </summary>
        private string data = string.Empty;

        /// <summary>
        /// TS3 States for the Arduino Leonardo 5v output 
        /// </summary>
        public enum SerialTS3State
        {
            /// <summary>
            /// Tell the arduino to not seding 5v to radio
            /// </summary>
            NoResivingData,
            /// <summary>
            /// Tell the arduino to seding 5v to radio
            /// </summary>
            ResiveData
        }

        /// <summary>
        /// Get or set the current state
        /// </summary>
        public SerialTS3State State { get; set; }

        /// <summary>
        /// Current Serial port the arduino are connected to
        /// </summary>
        private SerialPort currentSerialPort;

        /// <summary>
        /// Get the info abute is Arduino are connected or not
        /// </summary>
        public bool IsConnected { private set; get; }

        /// <summary>
        /// Set this class to only be init by a singleton
        /// </summary>
        private SerialPinCommander()
        {

        }

        /// <summary>
        /// Get the SerialPincommand with the connection to Arduino
        /// </summary>
        /// <returns></returns>
        public static SerialPinCommander GetCommander()
        {
            if (serialPinCommander is null)
                serialPinCommander = new SerialPinCommander();
            return serialPinCommander;
        }

        /// <summary>
        /// Resive data 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResiceData_Event(object sender, SerialDataReceivedEventArgs e)
        {
            if (sender is SerialPort)
            {
                var port = (SerialPort)sender;
                data = ResiveData(ref port);
                Console.WriteLine(data);
            }
        }

        /// <summary>
        /// Get data from current Serial commication port
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        private string ResiveData(ref SerialPort port)
        {
            int bufferSize = port.BytesToRead;
            byte[] buffer = new byte[bufferSize];
            port.Read(buffer, 0, bufferSize);
            return Encoding.Default.GetString(buffer);
        }

        /// <summary>
        /// Send data to to current Serial commication port
        /// </summary>
        /// <param name="port"></param>
        /// <param name="data"></param>
        private void SendData(ref SerialPort port, string data)
        {
            byte[] buffer = Encoding.Default.GetBytes(data);
            port.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Starte to sending a stream of 8-bit's '1's to Arduino Leonardo
        /// </summary>
        private async void StartSender()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    if (State == SerialTS3State.ResiveData)
                        SendData(ref currentSerialPort, "1");
                }
            });
        }

        /// <summary>
        /// Connection fail event
        /// </summary>
        public event EventHandler ConnectionFail;

        /// <summary>
        /// Connection success event
        /// </summary>
        public event EventHandler ConnectionSuccess;

        /// <summary>
        /// Connect to Arduino Leonardo Serial Port
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            //
            if (!IsConnected)
            {
                // Get all com serial names
                var portNames = SerialPort.GetPortNames().ToList();

                SerialPort serialPort;

                // Loop all com port names 
                foreach (var name in portNames.OrderByDescending(i => portNames.IndexOf(i)))
                {
                    // Target new comport by comport name
                    serialPort = new SerialPort(name, portRange, Parity.None);

                    try
                    {
                        // Set the RTS to enable
                        serialPort.RtsEnable = true;
                        // Set the data resive event
                        serialPort.DataReceived += ResiceData_Event;
                        // Open the serialport
                        serialPort.Open();
                        // Send '1' to arduino to get a response for to check for is the rigth arduino devcice 
                        SendData(ref serialPort, "1");
                        // Wait 2sek for response data
                        await Task.Delay(2000);
                        // If data has '1' the connection istablish 
                        if (data.Equals("1"))
                        {
                            // Set the connection is connectet to true
                            IsConnected = true;
                            // Set the current serial port
                            currentSerialPort = serialPort;
                            // Start sender to to send stream of '1' if in the rigth state
                            StartSender();
                            // Call connection success event if et connectet
                            ConnectionSuccess?.Invoke(this, new EventArgs());
                        }
                        else
                        {
                            // Call connection fail event if et not connectet
                            ConnectionFail?.Invoke(this, new EventArgs());
                        }
                    }

                    finally
                    {
                        // If port not are connected to rigth Arduino close serila connection if is has one
                        if (currentSerialPort != null && currentSerialPort.IsOpen && !IsConnected)
                            currentSerialPort.Close();
                    }
                    // If its connected break out of the loop
                    if (IsConnected)
                        break;
                }
            }
        }

        /// <summary>
        /// Disconnect from Arduino Lenardo Serilal Port if its connected
        /// </summary>
        public void Disconnect()
        {
            // Check for is thre are a connection an close it
            if (currentSerialPort != null && currentSerialPort.IsOpen && IsConnected)
            {
                currentSerialPort.Close();
                currentSerialPort = null;
                IsConnected = false;
            }
        }
    }
}
