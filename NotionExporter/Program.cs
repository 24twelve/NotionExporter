using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Serilog;

namespace NotionExporter
{
    public static class Program
    {
        //todo: nullref when taskInfo null???? - добавил эксепшн на этот случай, ловим
        //todo: reduce unholy mess with cancelattion and loops
        //todo: make it webapp
        //todo: DI
        //todo: some configuration lib
        //todo: threading
        //todo: unknown state to unknown + sort of time budget for task polling
        //todo: fix encoding issues in zip if possible
        //todo: host somewhere
        public static void Main()
        {
            ConfigureLogging();
            var cancellationTokenSource = ConfigureCancellation();
            try
            {
                new JobExecutor("ExportAndBackupNotion", ExportAndBackupNotionWorkspace,
                    TimeSpan.FromMinutes(30), cancellationTokenSource).Run();
            }
            catch (Exception e)
            {
                Log.Fatal("Encountered exception. Application terminated. {e}", e);
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static CancellationTokenSource ConfigureCancellation()
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                cts.Cancel();
                e.Cancel = true;
            };
            return cts;
        }

        private static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(path: "log.txt", rollingInterval: RollingInterval.Minute)
                .CreateLogger();
            Log.Information("Logging started.");
        }

        private static void ExportAndBackupNotionWorkspace(CancellationTokenSource cancellationTokenSource)
        {
            var now = DateTime.Now;
            var notionAccessToken =
                File.ReadAllText("secrets/token_v2"); //note: it seems that token_v2 cookie never expire
            var workspaceId = File.ReadAllText("secrets/workspace_id");
            var dropboxAccessToken = File.ReadAllText("secrets/dropbox_access_token");
            var dropboxClient = new DropboxClientWrapper(dropboxAccessToken);
            var notionClient = new NotionApiClient(notionAccessToken);


            Log.Information("Begin Notion export for {now}", now);
            var taskId = notionClient.PostEnqueueExportWorkspaceTask(workspaceId);
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var taskInfo = notionClient.PostGetTaskInfo(taskId);
                while (taskInfo.State == TaskState.InProgress)
                {
                    Log.Information("Exported notes: {0}", taskInfo.ProgressStatus.PagesExported);
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        Log.Information("Cancellation requested. Stopping...");
                        break;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    taskInfo = notionClient.PostGetTaskInfo(taskId);
                }

                if (cancellationTokenSource.IsCancellationRequested)
                {
                    Log.Information("Cancellation requested. Stopping...");
                    break;
                }

                if (taskInfo.State == TaskState.Success && taskInfo.ProgressStatus.Type == StatusType.Complete)
                {
                    var content = notionClient.GetExportedWorkspaceZip(taskInfo.ProgressStatus.ExportUrl);
                    var path = $"NotionExport-{now:dd-MM-yyyy-hh-mm-ss}.zip";
                    dropboxClient.UploadFileAndRotateOldFiles(path, content, now);
                    Log.Information("Successfully backed up {0}", path);
                    break;
                }

                Log.Error("Something unexpected happened. Task state: {0}",
                    JsonConvert.SerializeObject(taskInfo, Formatting.Indented));
            }
        }
    }
}