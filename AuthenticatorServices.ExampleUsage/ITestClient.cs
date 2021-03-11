using System.Net.Http;
using System.Threading.Tasks;

namespace AuthenticatorServices.ExampleUsage
{
    public interface ITestClient
    {
        Task<HttpResponseMessage> GetAsync();
    }
}
