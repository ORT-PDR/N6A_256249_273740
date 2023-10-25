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

        public async Task SendAsync(byte[] data) 
        {
            int offset = 0;
            int size = data.Length;

            var networstream = _tcpClient.GetStream();
            await networstream.WriteAsync(data, offset, size);
        }


        public async Task<byte[]> ReceiveAsync(int length)
        {
            byte[] response = new byte[length];
            int offset = 0;

            var networkStream = _tcpClient.GetStream();

            while (offset < length)
            {
                int received = await networkStream.ReadAsync(response, offset, length - offset);
                if (received == 0)
                {
                    Console.WriteLine("Client disconnected");
                    throw new Exception();
                }
                offset += received;

            }
            return response;
        }
    }
}
