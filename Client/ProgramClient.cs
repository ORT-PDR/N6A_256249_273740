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
        
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting client...");
            
            try
            {
                while (true)
                {
                    bool exit = false;
                    Login login = new Login(settingsMngr);
                    await login.Log();
                }
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