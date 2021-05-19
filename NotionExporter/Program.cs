using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace NotionExporter
{
    public static class Program
    {
        //todo: push to dropbox
        //todo: make periodic jobs
        //todo: make it webapp
        //todo: unknown state to unknown + sort of time budget
        //todo: host somewhere
        public static void Main(string[] args)
        {
            var token = File.ReadAllText("secrets/token_v2"); //note: it seems that token_v2 cookie never expire
            var workspaceId = File.ReadAllText("secrets/workspace_id");

            var notionClient = new NotionApiClient(token);
            var taskId = notionClient.PostEnqueueExportWorkspaceTask(workspaceId);
            var taskInfo = notionClient.PostGetTaskInfo(taskId);
            while (taskInfo.State == TaskState.InProgress)
            {
                Console.WriteLine($"Exported notes: {taskInfo.ProgressStatus.PagesExported}");
                Thread.Sleep(TimeSpan.FromSeconds(5));
                taskInfo = notionClient.PostGetTaskInfo(taskId);
            }

            if (taskInfo.State == TaskState.Success && taskInfo.ProgressStatus.Type == StatusType.Complete)
            {
                var result = notionClient.GetExportedWorkspaceZip(taskInfo.ProgressStatus.ExportUrl);
                File.WriteAllBytes($"NotionExport-{DateTime.Now:dd-MM-yyyy}.zip",
                    result); //todo: fix encoding issues in zip if possible
            }
            else
            {
                Console.WriteLine(
                    $"Something unexpected happened. Task state: {JsonConvert.SerializeObject(taskInfo, Formatting.Indented)}");
            }
        }
    }
}