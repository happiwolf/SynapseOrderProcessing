using System.Threading.Tasks;

namespace Synapse.OrderProcessing
{
    public interface IOrderService
    {
        Task ProcessOrders();
    }
}
