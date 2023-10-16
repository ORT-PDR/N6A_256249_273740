using System.Net;
using System.Net.Sockets;
using Communication;

namespace Client.UI
{
    public class Login
    {
        private ConversionHandler conversionHandler;
        private SettingsManager settingsMngr;
        private NetworkDataHelper _networkDataHelper;

        public Login(SettingsManager _settingsMngr)
        {
            settingsMngr = _settingsMngr;
            conversionHandler = new ConversionHandler();
        }

        public TcpClient Log()
        {
            TcpClient ret = null;
            try
            {
                bool isAuthenticated = false;
                SocketHelper socketHelper;
                NetworkDataHelper networkDataHelper;

                while (!isAuthenticated)
                {
                    Console.WriteLine("Login (1) or create a new account (2)");
                    var text = Console.ReadLine();
                    if (text == "1")
                    {
                        ret = Connect();

                        networkDataHelper = new NetworkDataHelper(ret);
                        _networkDataHelper = networkDataHelper;
                        
                        networkDataHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.Authenticate));
                        Console.WriteLine("Enter username: ");
                        string username = Console.ReadLine();
                        Console.WriteLine("Enter password: ");
                        string password = Console.ReadLine();

                        string credentials = $"{username}:{password}";
                        Send(credentials);

                        try
                        {
                            ret.ReceiveTimeout = 5000;
                            byte[] lBytes = networkDataHelper.Receive(Protocol.FixedDataSize);
                            int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                            byte[] responseBytes = networkDataHelper.Receive(dataLength);
                            string response = conversionHandler.ConvertBytesToString(responseBytes);

                            Console.WriteLine($"Server Response: {response}");

                            if (response == "Authentication successful")
                            {
                                isAuthenticated = true;
                                Console.WriteLine("Login successful");
                                Console.WriteLine("You're connected to the server!");
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
                        catch (SocketException ex)
                        {
                            Console.WriteLine("Unable to connect to server. Please try again.");
                        }
                    }
                    else if (text == "2")
                    {
                        ret = Connect();
                        networkDataHelper = new NetworkDataHelper(ret);
                        _networkDataHelper = networkDataHelper;

                        networkDataHelper.Send(conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.CreateUser));
                        Console.WriteLine("Enter username: ");
                        string username = Console.ReadLine();
                        Console.WriteLine("Enter password: ");
                        string password = Console.ReadLine();

                        string credentials = $"{username}:{password}";
                        Send(credentials);

                        try
                        {
                            ret.ReceiveTimeout = 5000;
                            byte[] lBytes = networkDataHelper.Receive(Protocol.FixedDataSize);
                            int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                            byte[] responseBytes = networkDataHelper.Receive(dataLength);
                            string response = conversionHandler.ConvertBytesToString(responseBytes);

                            if (response == "success")
                            {
                                Console.WriteLine("User created successfully!");
                                Console.WriteLine("You're connected to the server!");
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
                        catch (SocketException ex)
                        {
                            Console.WriteLine("Unable to connect to server. Please try again.");
                        }
                    }
                }
                
                if (ret != null)
                {
                    ret.Close();
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Server disconnected");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server disconnected");
            }
            return ret;
        }

        public TcpClient Connect()
        {
            try
            {
                var socketClient = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                string ipServer = settingsMngr.ReadSettings(ClientConfig.serverIPconfigkey);
                string ipClient = settingsMngr.ReadSettings(ClientConfig.clientIPconfigkey);
                int serverPort = int.Parse(settingsMngr.ReadSettings(ClientConfig.serverPortconfigkey));
                int clientPort = int.Parse(settingsMngr.ReadSettings(ClientConfig.clientPortconfigkey));

                Console.WriteLine("Cliente con IP {0} y puerto {1}", ipClient, clientPort);
                Console.WriteLine("Cliente conectado a IP {0} y puerto {1}", ipServer, serverPort);

                var localEndPoint = new IPEndPoint(IPAddress.Parse(ipClient), 0);
                var serverEndpoint = new IPEndPoint(IPAddress.Parse(ipServer), serverPort);

                var tcpClient = new TcpClient(localEndPoint);
                
                tcpClient.Connect(serverEndpoint);

                return tcpClient;
            }
            catch(SocketException ex)
            {
                Console.WriteLine("Could not connect to server");
                return null;
            }
        }

        private void Send(string response)
        {
            try
            {
                byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
                int responseLength = responseBytes.Length;

                byte[] lengthBytes = conversionHandler.ConvertIntToBytes(responseLength);
                _networkDataHelper.Send(lengthBytes);
                _networkDataHelper.Send(responseBytes);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Server Disconnected");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
            }
        }
    }
}