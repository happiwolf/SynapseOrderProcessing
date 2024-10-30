using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Synapse.OrderProcessing
{
    public interface IAlertService
    {
        Task SendAlert(JObject order);
    }
}
