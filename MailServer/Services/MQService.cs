using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MailServer.Services
{
    public class MQService
    {
        private bool isMailServerRunning;
        private BlockingCollection<string> messageQueue;

        public MQService()
        {
            isMailServerRunning = false;
            messageQueue = new BlockingCollection<string>();
        }

        public async Task StartListeningToPurchaseEventsAsync()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "mail_server_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(queue: "mail_server_queue", exchange: "purchase_events_exchange", routingKey: "");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var purchaseEventData = Encoding.UTF8.GetString(body);

                    Console.WriteLine("Received purchase event: {0}", purchaseEventData);

                    if (isMailServerRunning)
                    {
                        messageQueue.Add(purchaseEventData);
                    }
                    else
                    {
                        Console.WriteLine("Mail server is currently not running. Waiting for it to start...");
                    }
                };

                channel.BasicConsume(queue: "mail_server_queue", autoAck: true, consumer: consumer);

                Console.WriteLine("Mail server started. Waiting for purchase events...");
                isMailServerRunning = true;

                _ = Task.Run(async () => await ProcessMessages());
                
                while (true)
                {
                    await Task.Delay(1000);
                }
            }
        }

        private async Task ProcessMessages()
        {
            while (true)
            {
                if (messageQueue.TryTake(out var message))
                {
                    await SendEmailToUserAsync(message);
                    await Task.Delay(5000);
                }
                else
                {
                    await Task.Delay(1000);
                }
            }
        }

        private async Task SendEmailToUserAsync(string purchaseEventData)
        {
            var p = purchaseEventData.Split(",");
            var user = p[0];
            Console.WriteLine($"Sending mail to {user}");

            await Task.Delay(5000);
        }
    }
}
