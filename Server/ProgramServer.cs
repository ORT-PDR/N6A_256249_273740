using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Communication;
using Models;
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
        private static bool exit = false;

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting server...");

            try
            {
                var socketServer = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                string ipServer = settingsMngr.ReadSettings(ServerConfig.serverIPconfigkey);
                int ipPort = int.Parse(settingsMngr.ReadSettings(ServerConfig.serverPortconfigkey));

                var localEndpoint = new IPEndPoint(IPAddress.Parse(ipServer), ipPort);
                socketServer.Bind(localEndpoint);
                socketServer.Listen();
                
                new Thread(ConsoleInputThread).Start();

                while (!exit)
                {
                    var socketClient = socketServer.Accept();
                    Console.WriteLine("New connection!");
                    new Thread(() => HandleClient(socketClient)).Start();
                    
                }

                while (Thread.CurrentThread.ManagedThreadId != 1)
                {
                    Thread.Sleep(100);
                }

                Console.WriteLine("Closing server...");

                socketServer.Shutdown(SocketShutdown.Both);
                socketServer.Close();
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

        static void HandleClient(Socket socketCliente)
        {
            try
            {
                var socketHelper = new SocketHelper(socketCliente);
                bool exit = false;
                
                UserAuthorization userAuthorization = new UserAuthorization(socketHelper, conversionHandler, userService);
                ProductHandler productHandler = new ProductHandler(socketHelper, conversionHandler, productService);

                while (!exit)
                {
                    byte[] commandBytes = socketHelper.Receive(Protocol.FixedDataSize);
                    string command = conversionHandler.ConvertBytesToString(commandBytes);

                    if (command == Protocol.ProtocolCommands.Authenticate)
                    {
                        Console.WriteLine("Authentication requested by client.");
                        
                        userAuthorization.Authenticate();
                    }
                    if(command == Protocol.ProtocolCommands.PublishProduct)
                    {
                        Console.WriteLine("Product publish requested by client.");
                        productHandler.PublishProduct();
                    }
                    if(command == Protocol.ProtocolCommands.GetAllUserProducts)
                    {
                        Console.WriteLine("User products requested by client.");
                        productHandler.SendAllUserProducts();
                    }
                    if(command == Protocol.ProtocolCommands.GetAllProducts)
                    {
                        Console.WriteLine("All products requested by client.");
                        productHandler.SendAllProducts();
                    }
                    if (command == Protocol.ProtocolCommands.UpdateProduct)
                    {
                        Console.WriteLine("Update product requested by client.");
                        productHandler.UpdateProduct();
                    }
                    if(command == Protocol.ProtocolCommands.UpdateProductImage)
                    {
                        Console.WriteLine("Update product image requested by client.");
                        productHandler.UpdateProductImage();
                    }
                    if(command == Protocol.ProtocolCommands.DeleteProduct)
                    {
                        Console.WriteLine("Delete product requested by client.");
                        productHandler.DeleteProduct();
                    }
                    if(command == Protocol.ProtocolCommands.SearchProducts)
                    {
                        Console.WriteLine("Search products requested by client.");
                        productHandler.SearchProducts();
                    }
                    if(command == Protocol.ProtocolCommands.CreateUser)
                    {
                        Console.WriteLine("Client creating a new user.");
                        userAuthorization.CreateUser();
                    }
                    if(command == Protocol.ProtocolCommands.BuyProduct)
                    {
                        Console.WriteLine("Client buying a product.");
                        productHandler.BuyProduct();
                    }
                    if(command == Protocol.ProtocolCommands.GetAllPurchases)
                    {
                        Console.WriteLine("Client requesting all purchases.");
                        productHandler.SendAllPurchases();
                    }
                    if(command == Protocol.ProtocolCommands.RateProduct)
                    {
                        Console.WriteLine("Client rating a product.");
                        productHandler.RateProduct();
                    }
                    if(command == Protocol.ProtocolCommands.DownloadProductImage)
                    {
                        Console.WriteLine("Image download requested by client.");
                        productHandler.DownloadProductImage();
                    }
                    if(command == Protocol.ProtocolCommands.GetAllProductReviews)
                    {
                        Console.WriteLine("Client requesting all product reviews.");
                        productHandler.SendAllProductReviews();
                    }
                    if(command == Protocol.ProtocolCommands.Exit)
                    {
                        Console.WriteLine("Exit requested by client.");
                        exit = true;
                    }
                    
                    if (socketCliente.Poll(0, SelectMode.SelectRead) && socketCliente.Available == 0)
                    {
                        Console.WriteLine("Client disconnected");
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
                Console.WriteLine("Unexpected Exception: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected Exception: " + ex.Message);
            }
        }
        
        private static void ConsoleInputThread()
        {
            while (!exit)
            {
                if (Console.KeyAvailable)
                {
                    string line = Console.ReadLine();
                    if (line == "exit")
                    {
                        exit = true;
                        break;
                    }
                }

                Thread.Sleep(100);
            }
        }
        
    }
}