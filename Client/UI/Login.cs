using System.Net.Sockets;
using Communication;
using Server.UI;

namespace Client.UI
{
    public class Login
    {
        private ProductMenu _productMenu;
        public void Show(Socket socketClient)
        {
            try
            {
                var conversionHandler = new ConversionHandler();
                var socketHelper = new SocketHelper(socketClient);
                var _productMenu = new ProductMenu(socketClient);

                bool isAuthenticated = false;

                while (!isAuthenticated)
                {
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
                        isAuthenticated = true;
                        Console.WriteLine("Login successful");
                        Console.WriteLine("Welcome ");
                        Console.WriteLine("Press any key to continue");
                        Console.ReadKey();
                        _productMenu.ShowMainMenu(username);
                    }
                    else
                    {
                        Console.WriteLine("Login failed. Please try again.");
                    }
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