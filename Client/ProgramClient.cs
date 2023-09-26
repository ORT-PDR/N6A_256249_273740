using System.Net;
using System.Net.Sockets;
using System.Text;
using Client.UI;
using Communication;

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
                var socketClient = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                string ipServer = settingsMngr.ReadSettings(ClientConfig.serverIPconfigkey);
                string ipClient = settingsMngr.ReadSettings(ClientConfig.clientIPconfigkey);
                int serverPort = int.Parse(settingsMngr.ReadSettings(ClientConfig.serverPortconfigkey));

                var localEndPoint = new IPEndPoint(IPAddress.Parse(ipClient), 0);
                socketClient.Bind(localEndPoint);
                var serverEndpoint = new IPEndPoint(IPAddress.Parse(ipServer), serverPort);
                socketClient.Connect(serverEndpoint);
                Console.WriteLine("You're connected to the server!");

                bool exit = false;
                while (!exit)
                {
                    Login login = new Login(socketClient);
                    login.Show();
                    exit = true;
                }
    
                Console.WriteLine("Closing client...");

                socketClient.Shutdown(SocketShutdown.Both);
                socketClient.Close();
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