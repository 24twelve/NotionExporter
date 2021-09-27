using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NotionExporterWebApi.Clients;
using NotionExporterWebApi.Extensions;

namespace NotionExporterWebApi.Services
{
    public class NotionRecurringTasksHostedService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var jobExecutor = new JobExecutor(
                "AddScheduledRecurringTasksToInbox",
                AddScheduledRecurringTasksToInbox,
                period: TimeSpan.FromSeconds(5),
                timeBudget: TimeSpan.FromMinutes(1),
                cancellationToken);
            await jobExecutor.RunAsync().ConfigureAwait(false);
        }

        private async Task AddScheduledRecurringTasksToInbox(CancellationToken cancellationToken)
        {
            var client = new NotionPublicApiClient(Config.IntegrationToken);

            try
            {
                var db = await client.RetrieveDatabase("temp_БазаДляЭкспортера").ConfigureAwait(false);
                Log.For(this).Debug(db.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var x = 1;
            //read db

            //ensure all recurring tasks unchecked and correct date set
        }
    }
}