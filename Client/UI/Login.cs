using System.Net;
using System.Net.Sockets;
using Communication;
using System;

namespace Client.UI
{
    public class Login
    {
        private ConversionHandler conversionHandler;
        private SettingsManager settingsMngr;
        private NetworkDataHelper _networkDataHelper;
        private static TcpClient _tcpClient;

        public Login(SettingsManager _settingsMngr)
        {
            settingsMngr = _settingsMngr;
            conversionHandler = new ConversionHandler();
        }

        public async Task Log()
        {
            try
            {
                bool isAuthenticated = false;
                NetworkDataHelper networkDataHelper;

                while (!isAuthenticated)
                {
                    Console.WriteLine("Login (1) or create a new account (2)");
                    var text = Console.ReadLine();
                    if (text == "1")
                    {
                        await ConnectAsync();
                        networkDataHelper = new NetworkDataHelper(_tcpClient);
                        _networkDataHelper = networkDataHelper;

                        await networkDataHelper.SendAsync(
                            conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.Authenticate));
                        Console.WriteLine("Enter username: ");
                        string username = Console.ReadLine();
                        Console.WriteLine("Enter password: ");
                        string password = Console.ReadLine();

                        string credentials = $"{username}:{password}";
                        await SendAsync(credentials);

                        try
                        {
                            _tcpClient.ReceiveTimeout = 3000;

                            var receiveTask = networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
                            var delayTask = Task.Delay(3000);

                            var completedTask = await Task.WhenAny(receiveTask, delayTask);

                            if (completedTask == receiveTask)
                            {
                                byte[] lBytes = await receiveTask;
                                int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                                byte[] responseBytes = await networkDataHelper.ReceiveAsync(dataLength);
                                string response = conversionHandler.ConvertBytesToString(responseBytes);

                                if (response == "success")
                                {
                                    Console.WriteLine("User created successfully!");
                                    Console.WriteLine("You're connected to the server!");
                                    Console.WriteLine("Press any key to continue");
                                    Console.ReadKey();

                                    ProductMenu _productMenu = new ProductMenu(_tcpClient);
                                    await _productMenu.ShowMainMenu(username);
                                    isAuthenticated = false;
                                }
                                else
                                {
                                    Console.WriteLine("Error creating new user:");
                                    Console.WriteLine(response);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Timeout waiting for server response.");
                            }
                        }
                        catch (SocketException ex)
                        {
                            Console.WriteLine("Unable to connect to server. Please try again.");
                        }
                        catch (IOException ex)
                        {
                            
                        }
                    }
                    else if (text == "2")
                    {
                        await ConnectAsync();
                        networkDataHelper = new NetworkDataHelper(_tcpClient);
                        _networkDataHelper = networkDataHelper;

                        await networkDataHelper.SendAsync(
                            conversionHandler.ConvertStringToBytes(Protocol.ProtocolCommands.CreateUser));
                        Console.WriteLine("Enter username: ");
                        string username = Console.ReadLine();
                        Console.WriteLine("Enter password: ");
                        string password = Console.ReadLine();

                        string credentials = $"{username}:{password}";
                        await SendAsync(credentials);

                        try
                        {
                            _tcpClient.ReceiveTimeout = 3000;

                            var receiveTask = networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
                            var delayTask = Task.Delay(3000);

                            var completedTask = await Task.WhenAny(receiveTask, delayTask);

                            if (completedTask == receiveTask)
                            {
                                byte[] lBytes = await receiveTask;
                                int dataLength = conversionHandler.ConvertBytesToInt(lBytes);
                                byte[] responseBytes = await networkDataHelper.ReceiveAsync(dataLength);
                                string response = conversionHandler.ConvertBytesToString(responseBytes);

                                if (response == "success")
                                {
                                    Console.WriteLine("User created successfully!");
                                    Console.WriteLine("You're connected to the server!");
                                    Console.WriteLine("Press any key to continue");
                                    Console.ReadKey();

                                    ProductMenu _productMenu = new ProductMenu(_tcpClient);
                                    await _productMenu.ShowMainMenu(username);
                                    isAuthenticated = false;
                                }
                                else
                                {
                                    Console.WriteLine("Error creating new user:");
                                    Console.WriteLine(response);
                                }
                            }
                            else
                            {
                                Console.WriteLine("Timeout waiting for server response.");
                            }
                        }
                        catch (SocketException ex)
                        {
                            Console.WriteLine("Unable to connect to server. Please try again.");
                        }
                        catch (IOException ex)
                        {
                            
                        }
                    }
                }

                if (_tcpClient != null)
                {
                    _tcpClient.Close();
                }
            }
            catch (SocketException)
            {
                Console.Clear();
                Console.WriteLine("Server disconnected");
            }
            catch (IOException)
            {
                
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.WriteLine("Server disconnected");
            }
        }

        private async Task ConnectAsync()
        {
            try
            {
                string ipServer = settingsMngr.ReadSettings(ClientConfig.serverIPconfigkey);
                string ipClient = settingsMngr.ReadSettings(ClientConfig.clientIPconfigkey);
                int serverPort = int.Parse(settingsMngr.ReadSettings(ClientConfig.serverPortconfigkey));
                
                var localEndPoint = new IPEndPoint(IPAddress.Parse(ipClient), 0);
                var serverEndpoint = new IPEndPoint(IPAddress.Parse(ipServer), serverPort);

                var tcpClient = new TcpClient(localEndPoint);
                _tcpClient = tcpClient;
                
                await _tcpClient.ConnectAsync(serverEndpoint);
            }
            catch(SocketException ex)
            {
                Console.WriteLine("Could not connect to server");
            }
        }

        private async Task SendAsync(string response)
        {
            try
            {
                byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
                int responseLength = responseBytes.Length;
                
                byte[] lengthBytes = conversionHandler.ConvertIntToBytes(responseLength);
                await _networkDataHelper.SendAsync(lengthBytes);
                await _networkDataHelper.SendAsync(responseBytes);
            }
            catch (SocketException ex)
            {
                Console.Clear();
                Console.WriteLine("Server Disconnected");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
            }
        }
    }
}