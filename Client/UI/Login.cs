using System.Net;
using System.Net.Sockets;
using Communication;

namespace Client.UI
{
    public class Login
    {
        private ConversionHandler conversionHandler;
        private SettingsManager settingsMngr;
        private SocketHelper _socketHelper;

        public Login(SettingsManager _settingsMngr)
        {
            settingsMngr = _settingsMngr;
            conversionHandler = new ConversionHandler();
        }

        public Socket Log()
        {
            Console.Clear();
            Socket ret = null;
            try
            {
                bool isAuthenticated = false;
                SocketHelper socketHelper;

                while (!isAuthenticated)
                {
                    Console.WriteLine("Login (1) or create a new account (2)");
                    var text = Console.ReadLine();
                    if (text == "1")
                    {
                        ret = Connect();
                        socketHelper = new SocketHelper(ret);

                        _socketHelper = socketHelper;

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

                            ProductMenu _productMenu = new ProductMenu(ret);
                            _productMenu.ShowMainMenu(username);
                            isAuthenticated = false;
                        }
                        else
                        {
                            Console.WriteLine("Login failed. Please try again.");
                        }
                    }
                    else if (text == "2")
                    {
                        ret = Connect();
                        socketHelper = new SocketHelper(ret);

                        _socketHelper = socketHelper;

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

                        if (response == "success")
                        {
                            Console.WriteLine("User created successfully!");
                            Console.WriteLine("Welcome ");
                            Console.WriteLine("Press any key to continue");
                            Console.ReadKey();

                            ProductMenu _productMenu = new ProductMenu(ret);
                            _productMenu.ShowMainMenu(username);
                            isAuthenticated = false;
                        }
                        else
                        {
                            Console.WriteLine("Error creating new user:");
                            Console.WriteLine(response);
                        }
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
            return ret;
        }

        public Socket Connect()
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

            return socketClient;
        }

        private void Send(string response)
        {
            byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
            int responseLength = responseBytes.Length;

            byte[] lengthBytes = conversionHandler.ConvertIntToBytes(responseLength);
            _socketHelper.Send(lengthBytes);
            _socketHelper.Send(responseBytes);
        }
    }
}