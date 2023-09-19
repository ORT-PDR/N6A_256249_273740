using System.Net;
using System.Net.Sockets;
using System.Text;
using Communication;
using Server;
using Server.BusinessLogic;
using Server.UI;

namespace Client
{
    public class ProgramClient
    {
        static readonly SettingsManager settingsMngr = new SettingsManager();
        
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting client...");
            
            try
            {
                var socketCliente = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                string ipServer = settingsMngr.ReadSettings(ClientConfig.serverIPconfigkey);
                string ipClient = settingsMngr.ReadSettings(ClientConfig.clientIPconfigkey);
                int serverPort = int.Parse(settingsMngr.ReadSettings(ClientConfig.serverPortconfigkey));

                var localEndPoint = new IPEndPoint(IPAddress.Parse(ipClient), 0);
                socketCliente.Bind(localEndPoint);
                var serverEndpoint = new IPEndPoint(IPAddress.Parse(ipServer), serverPort);
                socketCliente.Connect(serverEndpoint);
                Console.WriteLine("You're connected to the server!");

                bool exit = false;
                while (!exit)
                {
                    Login(socketCliente);
                    string line = Console.ReadLine();
                    if (line == "exit") { exit = true; }
                }
    
                Console.WriteLine("Closing client...");

                socketCliente.Shutdown(SocketShutdown.Both);
                socketCliente.Close();
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
            }
        }
        
        static void Login(Socket socketClient)
        {
            try
            {
                var socketHelper = new SocketHelper(socketClient);
                
                socketHelper.Send(Encoding.UTF8.GetBytes(Protocol.ProtocolCommands.Authenticate));
                Console.WriteLine("Enter username: ");
                string username = Console.ReadLine();
                Console.WriteLine("Enter password: ");
                string password = Console.ReadLine();
                
                string credentials = $"{username}:{password}";
                socketHelper.Send(Encoding.UTF8.GetBytes(credentials));
                
                byte[] responseBytes = socketHelper.Receive(Protocol.MaxPacketSize);
                string response = Encoding.UTF8.GetString(responseBytes);

                Console.WriteLine($"Server Response: {response}");
            }
            catch (SocketException)
            {
                Console.WriteLine("Server disconnected");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
            }
        }
    }
}