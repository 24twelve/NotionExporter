using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NotionExporter
{
    internal class EnqueueTaskResult
    {
        [JsonProperty("taskId")] public string TaskId { get; set; }
    }

    public class NotionApiClient : IDisposable
    {
        public NotionApiClient(string token)
        {
            var cookieContainer = new CookieContainer();
            var httpHandler = new HttpClientHandler {CookieContainer = cookieContainer};
            httpClient = new HttpClient(httpHandler) {BaseAddress = new Uri(BaseUrl)};
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            cookieContainer.Add(httpClient.BaseAddress,
                new Cookie("token_v2",
                    token));
        }

        public void Dispose()
        {
            httpClient?.Dispose();
        }

        public string PostEnqueueExportWorkspaceTask(string workspaceId)
        {
            var enqueueTaskUri = $"{BaseUrl}/enqueueTask";

            var enqueueTaskContent = new StringContent(
                JsonConvert.SerializeObject(new ExportSpaceRequest(workspaceId), Formatting.Indented),
                Encoding.UTF8, "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, enqueueTaskUri) {Content = enqueueTaskContent};

            var result = httpClient.SendAsync(httpRequest).GetAwaiter().GetResult();
            return JsonConvert
                .DeserializeObject<EnqueueTaskResult>(result.Content.ReadAsStringAsync().GetAwaiter().GetResult())
                ?.TaskId;
        }

        public GetTaskInfoResult PostGetTaskInfo(string taskId)
        {
            var uri = $"{BaseUrl}/getTasks";
            var content =
                new StringContent(JsonConvert.SerializeObject(new GetTaskInfoRequest(taskId), Formatting.Indented),
                    Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, uri) {Content = content};

            var result = httpClient.SendAsync(request).GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<TaskInfo>(result.Content.ReadAsStringAsync().GetAwaiter().GetResult())
                ?.Results.FirstOrDefault();
        }


        public byte[] GetExportedWorkspaceZip(string downloadUrl)
        {
            return httpClient.GetByteArrayAsync(downloadUrl).GetAwaiter().GetResult();
        }

        private const string BaseUrl = "https://www.notion.so/api/v3";

        private readonly HttpClient httpClient;
    }

    public class GetTaskInfoRequest
    {
        public GetTaskInfoRequest(string taskId)
        {
            TaskIds = new[] {taskId};
        }

        [JsonProperty("taskIds")] public string[] TaskIds { get; set; }
    }

    public class TaskInfo
    {
        [JsonProperty("results")] public GetTaskInfoResult[] Results { get; set; }
    }

    public class GetTaskInfoResult
    {
        [JsonProperty("id")] public string TaskId { get; set; }
        [JsonProperty("state")] public TaskState State { get; set; }
        [JsonProperty("status")] public ProgressStatus ProgressStatus { get; set; }
    }

    public class ProgressStatus
    {
        [JsonProperty("pagesExported")] public int? PagesExported { get; set; }
        [JsonProperty("type")] public StatusType? Type { get; set; } //sic!
        [JsonProperty("exportURL")] public string ExportUrl { get; set; }
    }

    public enum StatusType
    {
        [EnumMember(Value = "complete")] Complete,
        [EnumMember(Value = "progress")] Progress //todo: treat unknown enum values as nulls
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TaskState
    {
        [EnumMember(Value = "in_progress")] InProgress,
        [EnumMember(Value = "success")] Success
    }


    internal class ExportSpaceRequest
    {
        public ExportSpaceRequest(string workspaceId)
        {
            Task = new NotionTask(workspaceId);
        }

        [JsonProperty("task")] public NotionTask Task { get; set; }

        internal class NotionTask
        {
            public NotionTask(string workspaceId)
            {
                Request = new NotionRequest(workspaceId);
            }

            [JsonProperty("eventName")] public string EventName { get; set; } = "exportSpace";

            [JsonProperty("request")] public NotionRequest Request { get; set; }

            internal class NotionRequest
            {
                public NotionRequest(string workspaceId)
                {
                    SpaceId = workspaceId;
                }

                [JsonProperty("exportOptions")] public ExportOptions Options { get; set; } = new ExportOptions();
                [JsonProperty("spaceId")] public string SpaceId;

                internal class ExportOptions
                {
                    [JsonProperty("exportType")] public string ExportType { get; set; } = "markdown";
                    [JsonProperty("locale")] public string Locale { get; set; } = "en";
                    [JsonProperty("timeZone")] public string TimeZone { get; set; } = "Asia/Yekaterinburg";
                }
            }
        }
    }
}