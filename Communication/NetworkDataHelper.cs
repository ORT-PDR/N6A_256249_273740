using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Communication
{
    public class NetworkDataHelper
    {

        private readonly TcpClient _tcpClient;

        public NetworkDataHelper(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public void Send(byte[] data) 
        {
            int offset = 0;
            int size = data.Length;

            var networstream = _tcpClient.GetStream();
            networstream.Write(data, offset, size);
        }


        public byte[] Receive(int length)
        {
            byte[] response = new byte[length];
            int offset = 0;

            var networkStream = _tcpClient.GetStream();

            while (offset < length)
            {
                int received = networkStream.Read(response, offset, length - offset);   
                if (received == 0)
                {
                    throw new SocketException();
                }
                offset += received;

            }
            return response;

        }
    }
}
