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
                Socket socketClient;

                bool exit = false;
                Login login = new Login(settingsMngr);
                socketClient = login.Log();
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