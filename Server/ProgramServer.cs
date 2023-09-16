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
                    new Thread(() => ManejarCliente(socketClient)).Start();

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

        static void ManejarCliente(Socket socketCliente)
        {
            try
            {
                Console.WriteLine("Client is connected");
                bool clienteConectado = true;
                while (clienteConectado)
                {
                    // TODO: la lógica de comunicación con el cliente
                }
                Console.WriteLine("Cliente Desconectado");
            }
            catch (SocketException)
            {
                Console.WriteLine("Cliente Desconectado!");
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
    }
}