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
            await Task.CompletedTask;
        }
    }
}
