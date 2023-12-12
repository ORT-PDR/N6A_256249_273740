using System.Text;
using MailServer.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MailServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var mqService = new MQService();
            await mqService.StartListeningToPurchaseEventsAsync();
        }
    }
}