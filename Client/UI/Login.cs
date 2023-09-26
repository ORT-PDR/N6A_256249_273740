using System.Net.Sockets;
using Communication;

namespace Client.UI
{
    public class Login
    {
        private ProductMenu _productMenu;
        private ConversionHandler conversionHandler;
        private SocketHelper socketHelper;

        public Login(Socket socketClient)
        {
            conversionHandler = new ConversionHandler();
            _productMenu = new ProductMenu(socketClient);
            socketHelper = new SocketHelper(socketClient);
        }

        public void Show()
        {
            try
            {
                bool isAuthenticated = false;

                while (!isAuthenticated)
                {
                    Console.WriteLine("Login (1) or create a new account (2). Type exit to disconnect.");
                    var text = Console.ReadLine();
                    if (text == "1")
                    {
                        socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.Authenticate));
                        Console.WriteLine("Enter username: ");
                        string username = Console.ReadLine();
                        Console.WriteLine("Enter password: ");
                        string password = Console.ReadLine();

                        string credentials = $"{username}:{password}";
                        Send(credentials);

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
                    else if (text == "2")
                    {
                        socketHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.CreateUser));
                        Console.WriteLine("Enter username: ");
                        string username = Console.ReadLine();
                        Console.WriteLine("Enter password: ");
                        string password = Console.ReadLine();

                        string credentials = $"{username}:{password}";
                        Send(credentials);

                        byte[] lBytes = socketHelper.Receive(Protocol.FixedDataSize);
                        int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                        byte[] responseBytes = socketHelper.Receive(dataLength);
                        string response = conversionHandler.ConvertBytesToString(responseBytes);

                        if(response == "success")
                        {
                            Console.WriteLine("User created successfully!");
                            Console.WriteLine("Welcome ");
                            Console.WriteLine("Press any key to continue");
                            Console.ReadKey();
                            _productMenu.ShowMainMenu(username);
                        }
                        else
                        {
                            Console.WriteLine("Error creating new user:");
                            Console.WriteLine(response);
                        }
                    }
                    else if (text.ToLower() == "exit")
                    {
                        return;
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

        private void Send(string response)
        {
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            int responseLength = responseBytes.Length;

            byte[] lengthBytes = conversionHandler.ConvertIntToBytes(responseLength);
            socketHelper.Send(lengthBytes);
            socketHelper.Send(responseBytes);
        }
    }
}