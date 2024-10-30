using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Xunit;
using Microsoft.Extensions.Logging;
using Synapse.OrderProcessing;

namespace Synapse.OrderProcessing.Tests
{
    public class OrderServiceTests
    {
        [Fact]
        public async Task ProcessOrders_DeliveredOrder_SendsAlertAndUpdatesOrder()
        {
            var mockHttp = new Mock<HttpMessageHandler>();
            mockHttp.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[{ \"id\": \"1\", \"status\": \"delivered\" }]")
                });
            var httpClient = new HttpClient(mockHttp.Object);

            var alertServiceMock = new Mock<IAlertService>();
            var loggerMock = new Mock<ILogger<OrderService>>();
            var service = new OrderService(httpClient, alertServiceMock.Object, loggerMock.Object);

            await service.ProcessOrders();

            alertServiceMock.Verify(a => a.SendAlert(It.IsAny<JObject>()), Times.Once);
        }

        [Fact]
        public async Task ProcessOrders_NonDeliveredOrder_DoesNotSendAlert()
        {
            var mockHttp = new Mock<HttpMessageHandler>();
            mockHttp.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("[{ \"id\": \"2\", \"status\": \"in transit\" }]")
                });
            var httpClient = new HttpClient(mockHttp.Object);

            var alertServiceMock = new Mock<IAlertService>();
            var loggerMock = new Mock<ILogger<OrderService>>();
            var service = new OrderService(httpClient, alertServiceMock.Object, loggerMock.Object);

            await service.ProcessOrders();

            alertServiceMock.Verify(a => a.SendAlert(It.IsAny<JObject>()), Times.Never);
        }

        [Fact]
        public async Task ProcessOrders_ApiFailure_LogsError()
        {
            var mockHttp = new Mock<HttpMessageHandler>();
            mockHttp.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("API failure"));

            var httpClient = new HttpClient(mockHttp.Object);
            var alertServiceMock = new Mock<IAlertService>();
            var loggerMock = new Mock<ILogger<OrderService>>();
            var service = new OrderService(httpClient, alertServiceMock.Object, loggerMock.Object);

            await service.ProcessOrders();

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while processing orders.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
