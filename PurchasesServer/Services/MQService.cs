using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PurchasesServer
{
    public class MQService
    {
        public MQService()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
        
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "purchase_events_exchange", type: ExchangeType.Fanout);
            channel.QueueDeclare(queue: "purchase_events_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueBind(queue: "purchase_events_queue", exchange: "purchase_events_exchange", routingKey: "");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine("[x] Received purchase event: {0}", message);

                var p = message.Split(",");
                Purchase purchase = new Purchase
                {
                    Username = p[0],
                    Product = p[1],
                    Date = DateTime.Parse(p[2])
                };
            
                var purchaseDataAccess = PurchaseDataAccess.GetInstance();
                purchaseDataAccess.AddPurchase(purchase);
            };

            channel.BasicConsume(queue: "purchase_events_queue", autoAck: true, consumer: consumer);
        }
    }
}