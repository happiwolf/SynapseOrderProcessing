using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Synapse.OrderProcessing
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;

        public OrderService(ILogger<OrderService> logger)
        {
            _logger = logger;
        }

        public async Task ProcessOrders()
        {
            _logger.LogInformation("Starting order processing...");

            try
            {
                var orders = new[] { "order1", "order2" };
                foreach (var order in orders)
                {
                    _logger.LogInformation("Processing order: {OrderId}", order);
                    await Task.Delay(50);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during order processing.");
            }

            _logger.LogInformation("Completed order processing.");
        }
    }
}
