using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Communication;
using Server.BusinessLogic;
using Server.UIHandler;

namespace Server
{
    public class ProgramServer
    {
        static readonly SettingsManager settingsMngr = new SettingsManager();
        static readonly Storage storage = Storage.Instance;
        static readonly UserService userService = new UserService(storage);
        static readonly ProductService productService = new ProductService(storage);
        static readonly ConversionHandler conversionHandler = new ConversionHandler();
        static TcpListener tcpListener;
        private static bool exit = false;
        private static List<TcpClient> clients = new List<TcpClient>();
        
        
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting server...");

            try
            {
                string ipServer = settingsMngr.ReadSettings(ServerConfig.serverIPconfigkey);
                int ipPort = int.Parse(settingsMngr.ReadSettings(ServerConfig.serverPortconfigkey));
                var localEndpoint = new IPEndPoint(IPAddress.Parse(ipServer), ipPort);
                
                tcpListener = new TcpListener(localEndpoint);
                tcpListener.Start();
                
                
                var exitTask = Task.Run(async () => await HandleConsoleInputAsync());

                while (!exit)
                {
                    try
                    {
                        TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                        clients.Add(tcpClient);
                        Console.WriteLine("New connection!");
                        var task = Task.Run(async () => await HandleClient(tcpClient));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Server Closed");
                    }
                }

                if (!exit)
                {
                    CloseServer();
                }
            }
            catch (ServerException ex)
            {
                throw new ServerException("Server Exception: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new ServerException("Unexpected Exception: " + ex.Message);
            }
        }

        static async Task HandleClient(TcpClient tcpClient)
        {
            try
            {
                NetworkDataHelper networkDataHelper = new NetworkDataHelper(tcpClient);

                UserAuthorization userAuthorization =
                    new UserAuthorization(networkDataHelper, conversionHandler, userService);
                ProductHandler productHandler =
                    new ProductHandler(networkDataHelper, conversionHandler, productService);

                while (!exit)
                {
                    byte[] commandBytes = await networkDataHelper.ReceiveAsync(Protocol.FixedDataSize);
                    string command = conversionHandler.ConvertBytesToString(commandBytes);

                    if (command == Protocol.ProtocolCommands.Authenticate)
                    {
                        Console.WriteLine("Authentication requested by client.");

                        await userAuthorization.Authenticate();
                    }

                    if (command == Protocol.ProtocolCommands.PublishProduct)
                    {
                        Console.WriteLine("Product publish requested by client.");
                        await productHandler.PublishProduct();
                    }

                    if (command == Protocol.ProtocolCommands.GetAllUserProducts)
                    {
                        Console.WriteLine("User products requested by client.");
                        await productHandler.SendAllUserProducts();
                    }

                    if (command == Protocol.ProtocolCommands.GetAllProducts)
                    {
                        Console.WriteLine("All products requested by client.");
                        await productHandler.SendAllProducts();
                    }

                    if (command == Protocol.ProtocolCommands.UpdateProduct)
                    {
                        Console.WriteLine("Update product requested by client.");
                        await productHandler.UpdateProduct();
                    }

                    if (command == Protocol.ProtocolCommands.UpdateProductImage)
                    {
                        Console.WriteLine("Update product image requested by client.");
                        await productHandler.UpdateProductImage();
                    }

                    if (command == Protocol.ProtocolCommands.DeleteProduct)
                    {
                        Console.WriteLine("Delete product requested by client.");
                        await productHandler.DeleteProduct();
                    }

                    if (command == Protocol.ProtocolCommands.SearchProducts)
                    {
                        Console.WriteLine("Search products requested by client.");
                        await productHandler.SearchProducts();
                    }

                    if (command == Protocol.ProtocolCommands.CreateUser)
                    {
                        Console.WriteLine("Client creating a new user.");
                        await userAuthorization.CreateUser();
                    }

                    if (command == Protocol.ProtocolCommands.BuyProduct)
                    {
                        Console.WriteLine("Client buying a product.");
                        await productHandler.BuyProduct();
                    }

                    if (command == Protocol.ProtocolCommands.GetAllPurchases)
                    {
                        Console.WriteLine("Client requesting all purchases.");
                        await productHandler.SendAllPurchases();
                    }

                    if (command == Protocol.ProtocolCommands.RateProduct)
                    {
                        Console.WriteLine("Client rating a product.");
                        await productHandler.RateProduct();
                    }

                    if (command == Protocol.ProtocolCommands.DownloadProductImage)
                    {
                        Console.WriteLine("Image download requested by client.");
                        await productHandler.DownloadProductImage();
                    }

                    if (command == Protocol.ProtocolCommands.GetAllProductReviews)
                    {
                        Console.WriteLine("Client requesting all product reviews.");
                        await productHandler.SendAllProductReviews();
                    }

                    if (command == Protocol.ProtocolCommands.Exit)
                    {
                        Console.WriteLine("Exit requested by client.");
                        exit = true;
                    }

                    if (tcpClient.Client.Poll(0, SelectMode.SelectRead) && tcpClient.Available == 0)
                    {
                        Console.WriteLine("Client disconnected");
                        tcpClient.Close();
                        break;
                    }
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Client disconnected");
            }
            catch (ServerException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ThreadInterruptedException ex)
            {
                Console.WriteLine("ThreadInterruptedException during socket close: " + ex.Message);
            }
            catch (Exception ex)
            {
                
            }
        }
        
        private static async Task HandleConsoleInputAsync()
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    string line = Console.ReadLine();
                    if (line.ToLower() == "exit")
                    {
                        exit = true;
                        
                        Console.WriteLine("Received exit command");
                        CloseServer();
                        break;
                    }
                }

                await Task.Delay(100);
            }
        }
        private static void CloseServer()
        {
            try
            {
                exit = true;
                if (tcpListener != null)
                {
                    Console.WriteLine("Closing Server!");
                    tcpListener.Stop();
                }
                foreach (TcpClient client in clients)
                {
                    client.Close();
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine("SocketException during socket close: " + ex.Message);
            }
            catch (ThreadInterruptedException ex)
            {
                Console.WriteLine("ThreadInterruptedException during socket close: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during socket close: " + ex.Message);
            }
        }
    }
}