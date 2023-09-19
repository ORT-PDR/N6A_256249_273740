using System.Net;
using System.Net.Sockets;
using System.Text;
using Client.UI;
using Communication;
using Server;
using Server.BusinessLogic;
using Server.UI;

namespace Client
{
    public class ProgramClient
    {
        static readonly SettingsManager settingsMngr = new SettingsManager();
        private static readonly Login login = new Login();
        
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
                    login.Show(socketCliente);
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
    }
}