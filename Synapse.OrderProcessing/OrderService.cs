using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;

namespace Synapse.OrderProcessing
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly IAlertService _alertService;
        private readonly ILogger<OrderService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly string _fetchOrdersUrl;
        private readonly string _updateOrderUrl;

        public OrderService(HttpClient httpClient, IAlertService alertService, ILogger<OrderService> logger)
        {
            _httpClient = httpClient;
            _alertService = alertService;
            _logger = logger;

            _fetchOrdersUrl = "https://orders-api.com/orders";
            _updateOrderUrl = "https://orders-api.com/orders/{0}/update";

            _retryPolicy = Policy.Handle<HttpRequestException>()
                                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(2),
                                    (exception, timeSpan, retryCount, context) =>
                                    {
                                        _logger.LogWarning($"Retry {retryCount} for {context.PolicyKey} at {context.OperationKey}, due to: {exception}.");
                                    });
        }

        public async Task ProcessOrders()
        {
            try
            {
                var orders = await FetchOrders();
                foreach (var order in orders)
                {
                    if (order["status"]?.ToString() == "delivered")
                    {
                        await _alertService.SendAlert(order);
                        await UpdateOrder(order);
                    }
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError("Request timed out. Please check the endpoint or network connection.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing orders.");
            }
        }

        private async Task<JObject[]> FetchOrders()
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(10);
                var response = await _httpClient.GetAsync(_fetchOrdersUrl);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return JArray.Parse(data).ToObject<JObject[]>();
                }
                _logger.LogError("Failed to fetch orders with status code: {StatusCode}", response.StatusCode);
                return Array.Empty<JObject>();
            });
        }

        private async Task UpdateOrder(JObject order)
        {
            var orderId = order["id"]?.ToString();
            var url = string.Format(_updateOrderUrl, orderId);

            await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _httpClient.PostAsync(url, null);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Order {orderId} updated successfully.");
                }
                else
                {
                    _logger.LogError($"Failed to update order {orderId} with status code: {response.StatusCode}");
                }
            });
        }
    }
}
