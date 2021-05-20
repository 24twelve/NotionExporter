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
        //todo: global logging
        //todo: make periodic jobs
        //todo: make it webapp
        //todo: unknown state to unknown + sort of time budget for task polling
        //todo: fix encoding issues in zip if possible
        //todo: host somewhere
        public static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(path: "log.txt", rollingInterval: RollingInterval.Minute)
                .CreateLogger();

            try
            {
                ExportAndBackupNotionWorkspace();
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

        private static void ExportAndBackupNotionWorkspace()
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
            var taskInfo = notionClient.PostGetTaskInfo(taskId);
            while (taskInfo.State == TaskState.InProgress)
            {
                Log.Information("Exported notes: {0}", taskInfo.ProgressStatus.PagesExported);
                Thread.Sleep(TimeSpan.FromSeconds(5));
                taskInfo = notionClient.PostGetTaskInfo(taskId);
            }

            if (taskInfo.State == TaskState.Success && taskInfo.ProgressStatus.Type == StatusType.Complete)
            {
                var content = notionClient.GetExportedWorkspaceZip(taskInfo.ProgressStatus.ExportUrl);
                var path = $"NotionExport-{now:dd-MM-yyyy-hh-mm-ss}.zip";
                dropboxClient.UploadFileAndRotateOldFiles(path, content, now);
            }
            else
            {
                Log.Error("Something unexpected happened. Task state: {0}",
                    JsonConvert.SerializeObject(taskInfo, Formatting.Indented));
            }
        }
    }
}