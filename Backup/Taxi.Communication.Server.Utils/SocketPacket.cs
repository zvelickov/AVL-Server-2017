using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Taxi.Communication.Server.Utils
{
    public class SocketPacket
    {
        private string _deviceNumber;

        public string DeviceNumber
        {
            get { return _deviceNumber; }
            set { _deviceNumber = value; }
        }


        private Socket m_currentSocket = null;

        // Constructor which takes a Socket and a client number
        public SocketPacket(Socket socket)
        {
            _deviceNumber = null;
            m_currentSocket = socket;
        }

        public Socket CurrentSocket
        {
            get { return m_currentSocket; }
        }

        public const int BufferSize = 256;

        // Buffer to store the data sent by the client
        public byte[] dataBuffer = new byte[BufferSize];

        // Bufer to store data after reading from dataBuffer
        public byte[] SocketMessageBuffer = new byte[0];
       
    }


}
