using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PurchaseServer
{
    public class MQService
    {
        public event EventHandler<string> PurchaseEventReceived;

        public void StartListening()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "weather", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received purchase event: {0}", message);
                OnPurchaseEventReceived(message);
            };

            channel.BasicConsume(queue: "weather", autoAck: true, consumer: consumer);
        }

        protected virtual void OnPurchaseEventReceived(string purchaseEvent)
        {
            PurchaseEventReceived?.Invoke(this, purchaseEvent);
        }
    }
}
