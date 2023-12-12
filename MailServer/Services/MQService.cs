using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MailServer.Services
{
    public class MQService
    {
        public MQService()
        {
            StartListeningToPurchaseEventsAsync().GetAwaiter().GetResult();
        }

        public async Task StartListeningToPurchaseEventsAsync()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" }; 

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "purchase_events_exchange", type: ExchangeType.Fanout);
                channel.QueueDeclare(queue: "mail_server_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(queue: "mail_server_queue", exchange: "purchase_events_exchange", routingKey: "");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var purchaseEventData = Encoding.UTF8.GetString(body);
                
                    Console.WriteLine("Received purchase event: {0}", purchaseEventData);
                
                    await SendEmailToUserAsync(purchaseEventData);
                };

                channel.BasicConsume(queue: "mail_server_queue", autoAck: true, consumer: consumer);

                Console.WriteLine("Waiting for purchase events...");
                Console.ReadLine(); 
            }
        }

        private async Task SendEmailToUserAsync(string purchaseEventData)
        {
            var p = purchaseEventData.Split(",");
            var user = p[0];
            Console.WriteLine($"Enviando correo a {user}");
    
            await Task.Delay(5000);
        }
    }
}


