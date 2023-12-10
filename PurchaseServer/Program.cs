using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace PurchaseServer
{
    public class Program
    {
        private static MQService mqService;
        private static PurchaseProcessor purchaseProcessor;

        public static void Main(string[] args)
        {
            mqService = new MQService();
            purchaseProcessor = new PurchaseProcessor();

            mqService.PurchaseEventReceived += PurchaseEventReceivedHandler;
            mqService.StartListening();

            Console.WriteLine("Purchase Server is running. Press [Enter] to exit.");
            Console.ReadLine();
        }

        private static void PurchaseEventReceivedHandler(object sender, string e)
        {
            purchaseProcessor.ProcessPurchase(e);
        }
    }
}