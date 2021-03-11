using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AuthenticatorServices.ExampleUsage
{

    public class TestClient : ITestClient
    {
        private readonly HttpClient _client;
        public TestClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> GetAsync()
        {
            return await _client.GetAsync("http://google.com");
        }
    }
}
