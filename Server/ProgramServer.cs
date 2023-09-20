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
                Console.WriteLine("Client is connected");

                var socketHelper = new SocketHelper(socketCliente);
                var conversionHandler = new ConversionHandler();

                byte[] commandBytes = socketHelper.Receive(Protocol.FixedDataSize);
                string command = conversionHandler.ConvertBytesToString(commandBytes);

                if (command == Protocol.ProtocolCommands.Authenticate)
                {
                    UserAuthorization userAuthorization = new UserAuthorization(socketHelper, conversionHandler, userService);
                    userAuthorization.Authenticate();
                }
                if(command == Protocol.ProtocolCommands.PublishProduct)
                {
                    ProductHandler productHandler = new ProductHandler(socketHelper, conversionHandler, productService);

                }
                else
                {
                    Console.WriteLine("Unknown authentication command from client.");
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