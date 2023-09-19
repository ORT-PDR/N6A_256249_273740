using System.Net.Sockets;
using Communication;
using Server.UI;

namespace Client.UI
{
    public class Login
    {
        public void Show(Socket socketClient)
        {
            try
            {
                var conversionHandler = new ConversionHandler();
                var socketHelper = new SocketHelper(socketClient);
                
                socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.Authenticate));
                Console.WriteLine("Enter username: ");
                string username = Console.ReadLine();
                Console.WriteLine("Enter password: ");
                string password = Console.ReadLine();

                string credentials = $"{username}:{password}";
                byte[] credentialsData = conversionHandler.ConvertStringToBytes(credentials);

                byte[] lengthBytes = conversionHandler.ConvertIntToBytes(credentialsData.Length);
                socketHelper.Send(lengthBytes);

                socketHelper.Send(credentialsData);

                byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                byte[] responseBytes = socketHelper.Receive(dataLength);
                string response = conversionHandler.ConvertBytesToString(responseBytes);

                Console.WriteLine($"Server Response: {response}");
                
                if (response == "Authentication successful")
                {
                    System.Console.WriteLine("Login successful");
                    System.Console.WriteLine("Welcome ");
                    System.Console.WriteLine("Press any key to continue");
                    System.Console.ReadKey();
                    //ProductMenu.ShowMainMenu();
                }
                else
                {
                    System.Console.WriteLine("Login failed");
                    System.Console.WriteLine("Press any key to continue");
                    System.Console.ReadKey();
                    Show(socketClient);
                }

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