using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace PurchasesServer
{
    public class Program
    {
        private static MQService mqService;
        public static void Main(string[] args)
        {
            // creamos conexion con RabbitMQ
            mqService = new MQService();
            CreateHostBuilder(args).Build().Run(); // bloqueante
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseUrls("http://localhost:5031");
                });
    }
}
