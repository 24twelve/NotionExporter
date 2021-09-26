using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NotionExporterWebApi.Clients;

namespace NotionExporterWebApi.Services
{
    public class NotionRecurringTasksHostedService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var jobExecutor = new JobExecutor(
                "AddScheduledRecurringTasksToInbox",
                AddScheduledRecurringTasksToInbox,
                TimeSpan.FromSeconds(5),
                TimeSpan.FromMinutes(1),
                cancellationToken);
            await jobExecutor.RunAsync().ConfigureAwait(false);
        }

        private async Task AddScheduledRecurringTasksToInbox(CancellationToken cancellationToken)
        {
            var integrationToken = await File.ReadAllTextAsync("secrets/integrationToken",
                cancellationToken).ConfigureAwait(false);
            var client = new NotionPublicApiClient(integrationToken);
            //read db

            //ensure all recurring tasks unchecked and correct date set
        }
    }
}