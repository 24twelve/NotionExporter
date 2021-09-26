using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NotionExporterWebApi.Clients;
using NotionExporterWebApi.Extensions;

namespace NotionExporterWebApi.Services
{
    public class NotionExporterHostedService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var jobExecutor = new JobExecutor("ExportAndBackupNotion", ExportAndBackupNotionWorkspace,
                TimeSpan.FromDays(1), TimeSpan.FromHours(1), cancellationToken);
            await jobExecutor.RunAsync();
        }

        private async Task ExportAndBackupNotionWorkspace(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var dropboxClient = new DropboxClientWrapper(Config.DropboxAccessToken);
            var notionClient = new NotionWebApiClient(Config.NotionTokenV2);


            Log.For(this).Information("Begin Notion export for {now}", now);
            var taskId = await notionClient.PostEnqueueExportWorkspaceTaskAsync(Config.NotionWorkspaceId)
                .ConfigureAwait(false);
            while (!cancellationToken.IsCancellationRequested)
            {
                var taskInfo = await notionClient.PostGetTaskInfoAsync(taskId).ConfigureAwait(false);
                while (taskInfo.State != TaskState.Success)
                {
                    if (taskInfo.ProgressStatus != null)
                    {
                        Log.For(this).Information("Exported notes: {0}. Task state: {1}",
                            taskInfo.ProgressStatus.PagesExported, JsonSerializer.SerializeObject(taskInfo));
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Log.For(this).Information("Cancellation requested. Stopping...");
                        break;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    taskInfo = await notionClient.PostGetTaskInfoAsync(taskId).ConfigureAwait(false);
                }

                //todo: reduce unholy mess with cancelattion and loops
                if (cancellationToken.IsCancellationRequested)
                {
                    Log.For(this).Information("Cancellation requested. Stopping...");
                    break;
                }

                if (taskInfo.ProgressStatus!.Type == StatusType.Complete)
                {
                    var content = await notionClient.GetExportedWorkspaceZipAsync(taskInfo.ProgressStatus!.ExportUrl!)
                        .ConfigureAwait(false);
                    var path = $"NotionExport-{now:dd-MM-yyyy-hh-mm-ss}.zip";
                    await dropboxClient.UploadFileAndRotateOldFilesAsync(path, content, now);
                    Log.For(this).Information("Successfully backed up {0}", path);
                    break;
                }

                Log.For(this).Error("Something unexpected happened. Task state: {0}",
                    JsonSerializer.SerializeObject(taskInfo));
            }
        }
    }
}