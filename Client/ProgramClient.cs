using System.Net;
using System.Net.Sockets;
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
                string ipServer = settingsMngr.ReadSettings(ClientConfig.serverIPconfigkey);
                int serverPort = int.Parse(settingsMngr.ReadSettings(ClientConfig.serverPortconfigkey));

                Protocol.Connect(ipServer, serverPort); 

                Console.WriteLine("You're connected to the server!");

                bool exit = false;
                while (!exit)
                {
                    // var productMenu = new ProductMenu();
                    // productMenu.ShowMainMenu();

                    string line = Console.ReadLine();
                    if (line == "exit") { exit = true; }
                }
    
                Console.WriteLine("Closing client...");

                Protocol.Disconnect();
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