using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Communication
{
    public static class Protocol
    {
        private static TcpClient client;
        private static NetworkStream stream;

        static Protocol()
        {
            client = new TcpClient();
        }

        public static void Connect(string ipAddress, int port)
        {
            try
            {
                client.Connect(ipAddress, port);
                stream = client.GetStream();
            }
            catch (Exception ex)
            {
                throw new ProtocolException("Connection error: " + ex.Message);
            }
        }

        public static void Disconnect()
        {
            try
            {
                stream.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                throw new ProtocolException("Disconnection error: " + ex.Message);
            }
        }

        public static void SendRequest(string header, string command, string data)
        {
            try
            {
                string message = $"{header}:{command}:{data.Length}:{data}";
                byte[] messageBytes = Encoding.ASCII.GetBytes(message);

                stream.Write(BitConverter.GetBytes(messageBytes.Length), 0, 4);
                stream.Write(messageBytes, 0, messageBytes.Length);
            }
            catch (Exception ex)
            {
                throw new ProtocolException("Error sending request: " + ex.Message);
            }
        }

        public static string ReceiveResponse()
        {
            try
            {
                byte[] lengthBuffer = new byte[4];
                stream.Read(lengthBuffer, 0, 4);
                int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                byte[] messageBytes = new byte[messageLength];
                stream.Read(messageBytes, 0, messageLength);

                string message = Encoding.ASCII.GetString(messageBytes);

                string[] parts = message.Split(':');
                if (parts.Length == 4)
                {
                    string header = parts[0];
                    string command = parts[1];
                    int dataLength = int.Parse(parts[2]);
                    string data = parts[3];

                    if (data.Length == dataLength)
                    {
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ProtocolException("Error receiving response: " + ex.Message);
            }

            return null;
        }
    }
}
