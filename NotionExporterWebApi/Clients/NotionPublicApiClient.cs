using System;
using System.Linq;
using System.Threading.Tasks;
using Notion.Client;

namespace NotionExporterWebApi.Clients
{
    public class NotionPublicApiClient
    {
        public NotionPublicApiClient(string integrationToken)
        {
            client = new NotionClient(new ClientOptions { AuthToken = integrationToken });
        }

        public async Task<Database> RetrieveDatabase(string userFriendlyDatabaseName)
        {
            if (tasksDatabaseId == null)
            {
                var allDatabases = (await client.Databases.ListAsync()).Results;
                tasksDatabaseId = allDatabases
                    .SingleOrDefault(x => x.Title.First().PlainText == userFriendlyDatabaseName)?.Id;
                if (tasksDatabaseId == null)
                {
                    throw new Exception($"Could not retrieve database id for '{userFriendlyDatabaseName}'");
                }
            }

            return await client.Databases.RetrieveAsync(tasksDatabaseId);
        }

        private readonly NotionClient client;
        private string? tasksDatabaseId;
    }
}