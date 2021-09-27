using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NotionExporterWebApi.Clients
{
    public class NotionPublicApiClient : IDisposable
    {
        public NotionPublicApiClient(string integrationToken)
        {
            client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(15)
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(integrationToken);
        }

        public async Task Authenticate()
        {

        }

        public async Task RetrieveDatabase(string userFriendlyDatabaseName)
        {

        }

        private const string BaseUrl = "https://api.notion.com";
        private readonly HttpClient client;

        public void Dispose()
        {
            //todo: check dispose called
            client.Dispose();
        }
    }
}