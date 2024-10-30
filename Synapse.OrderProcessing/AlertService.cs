using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Synapse.OrderProcessing
{
    public class AlertService : IAlertService
    {
        private readonly ILogger<AlertService> _logger;

        public AlertService(ILogger<AlertService> logger)
        {
            _logger = logger;
        }

        public async Task SendAlert(JObject order)
        {
            await Task.Delay(50);
            _logger.LogInformation($"Alert sent for order {order["id"]}.");
        }
    }
}
