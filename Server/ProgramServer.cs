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

                bool exit = false;

                while (!exit)
                {
                    var socketClient = socketServer.Accept();
                    Console.WriteLine("New connection!");
                    new Thread(() => HandleClient(socketClient)).Start();

                    string line = Console.ReadLine();
                    if (line == "exit") { exit = true; }
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

                while (!exit)
                {
                    byte[] commandBytes = socketHelper.Receive(Protocol.FixedDataSize);
                    string command = conversionHandler.ConvertBytesToString(commandBytes);

                    if (command == Protocol.ProtocolCommands.Authenticate)
                    {
                        Console.WriteLine("Authentication requested by client.");
                        UserAuthorization userAuthorization = new UserAuthorization(socketHelper, conversionHandler, userService);
                        userAuthorization.Authenticate();
                    }
                    if(command == Protocol.ProtocolCommands.PublishProduct)
                    {
                        Console.WriteLine("Product publish requested by client.");
                        ProductHandler productHandler = new ProductHandler(socketHelper, conversionHandler, productService);
                        productHandler.PublishProduct();
                    }
                    if(command == Protocol.ProtocolCommands.GetAllUserProducts)
                    {
                        Console.WriteLine("Products requested by client.");
                        ProductHandler productHandler = new ProductHandler(socketHelper, conversionHandler, productService);
                        productHandler.SendAllUserProducts();
                    }
                    if (command == Protocol.ProtocolCommands.UpdateProduct)
                    {
                        Console.WriteLine("Update product requested by client.");
                        ProductHandler productHandler = new ProductHandler(socketHelper, conversionHandler, productService);
                        productHandler.UpdateProduct();
                    }
                    if(command == Protocol.ProtocolCommands.DeleteProduct)
                    {
                        Console.WriteLine("Delete product requested by client.");
                        ProductHandler productHandler = new ProductHandler(socketHelper, conversionHandler, productService);
                        productHandler.DeleteProduct();
                    }
                    if(command == Protocol.ProtocolCommands.SearchProducts)
                    {
                        Console.WriteLine("Search products requested by client.");
                        ProductHandler productHandler = new ProductHandler(socketHelper, conversionHandler, productService);
                        productHandler.SearchProducts();
                    }
                    if(command == Protocol.ProtocolCommands.CreateUser)
                    {
                        Console.WriteLine("Client reating a new user.");
                        UserAuthorization userAuthorization = new UserAuthorization(socketHelper, conversionHandler, userService);
                        userAuthorization.CreateUser();
                    }
                    if(command == Protocol.ProtocolCommands.Exit)
                    {
                        Console.WriteLine("Exit requested by client.");
                        exit = true;
                    }
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Client disconnected");
            }
            catch (ServerException ex)
            {
                throw new ServerException("Server Exception: " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new ServerException("Unexpected Exception: " + ex.Message);
            }
            finally
            {
                socketCliente.Close();
            }
        }

        static void MainMenu()
        {
            Console.WriteLine("God");
        }
    }
}