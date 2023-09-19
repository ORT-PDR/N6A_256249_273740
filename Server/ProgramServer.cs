using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Communication;

namespace Server
{
    public class ProgramServer
    {
        static readonly SettingsManager settingsMngr = new SettingsManager();
        static readonly Storage storage = Storage.Instance;

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
                    Console.WriteLine("Authentication requested by client.");
                    
                    byte[] lengthBytes = socketHelper.Receive(Protocol.FixedDataSize);
                    int dataLength = BitConverter.ToInt32(lengthBytes, 0);
                    byte[] credentialsBytes = socketHelper.Receive(dataLength);
                    string credentials = conversionHandler.ConvertBytesToString(credentialsBytes);
                    string[] credentialsParts = credentials.Split(':');

                    if (credentialsParts.Length == 2)
                    {
                        string username = credentialsParts[0];
                        string password = credentialsParts[1];
                        
                        bool authenticationResult = true; //Authenticate(username, password);
                        
                        string response = authenticationResult ? "Authentication successful" : "Authentication failed";
                        byte[] responseBytes = conversionHandler.ConvertStringToBytes(response);
                        socketHelper.Send(responseBytes);
                        
                        if (authenticationResult)
                        {
                            MainMenu();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid credentials format.");
                        byte[] responseBytes = conversionHandler.ConvertStringToBytes("Invalid credentials format");
                        socketHelper.Send(responseBytes);
                    }
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

        static void Login()
        {
            try
            {
                Console.WriteLine("Authenticating...");
                //TODO logica inicio de sesion. Si inicia sesion bien, va a otro method q maneja al cliente ahi
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
        }

        static void MainMenu()
        {
            Console.WriteLine("God");
        }
    }
}