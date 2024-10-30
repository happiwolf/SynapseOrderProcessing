using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Synapse.OrderProcessing
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(configure => configure.AddConsole())
                .AddSingleton<IConfiguration>(configuration)
                .AddHttpClient<IOrderService, OrderService>(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                })
                .Services
                .AddTransient<IAlertService, AlertService>()
                .BuildServiceProvider();

            var orderService = serviceProvider.GetService<IOrderService>();
            orderService?.ProcessOrders().GetAwaiter().GetResult();
        }
    }
}
