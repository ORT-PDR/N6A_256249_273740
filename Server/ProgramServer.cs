using System.Net;
using System.Net.Sockets;
using Communication;

namespace Server
{
    public class ProgramServer
    {
        static readonly SettingsManager settingsMngr = new SettingsManager();
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting server...");

            var socketServer = new Socket(
            AddressFamily.InterNetwork,
                SocketType.Stream,
                    ProtocolType.Tcp);

            string ipServer = settingsMngr.ReadSettings(ServerConfig.serverIPconfigkey);
            int ipPort = int.Parse(settingsMngr.ReadSettings(ServerConfig.serverPortconfigkey));

            var localEndpoint = new IPEndPoint(IPAddress.Parse(ipServer), ipPort);
            socketServer.Bind(localEndpoint);
            socketServer.Listen();

            bool exit = false;

            while (!exit)
            {
                var socketClient = socketServer.Accept();
                Console.WriteLine("New connection!");
                new Thread(() => ManejarCliente(socketClient)).Start();

                string line = Console.ReadLine();
                if (line == "exit") { exit = true; }
            }

            Console.WriteLine("Closing server...");

            socketServer.Shutdown(SocketShutdown.Both);
            socketServer.Close();
        }

        static void ManejarCliente(Socket socketCliente)
        {
            //aca vamos a manejar todo lo que muestra y procesa el server al cliente
            //try
            //{
            //    Console.WriteLine("Client is connected");
            //    bool clienteConectado = true;
            //    while (clienteConectado)
            //    {
            //        var fileCommonHandler = new FileCommsHandler(socketCliente);
            //        fileCommonHandler.ReceiveFile();
            //        Console.WriteLine("Archivo recibido!!");
            //    }
            //    Console.WriteLine("Cliente Desconectado");
            //}
            //catch (SocketException)
            //{
            //    Console.WriteLine("Cliente Desconectado!");
            //}
        }
    }
}