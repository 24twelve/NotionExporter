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
            httpClient = new HttpClient(httpHandler)
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(15)
            };
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
            return MakePostRequestWithRetries<EnqueueTaskExportSpaceRequest, EnqueueTaskResult>("enqueueTask",
                new EnqueueTaskExportSpaceRequest(workspaceId))?.TaskId;
        }

        public GetTaskInfoResult PostGetTaskInfo(string taskId)
        {
            return MakePostRequestWithRetries<GetTaskInfoRequest, GetTaskInfoResults>("getTasks",
                    new GetTaskInfoRequest(taskId))
                ?.Results
                .FirstOrDefault();
        }


        public byte[] GetExportedWorkspaceZip(string downloadUrl)
        {
            return ExecuteWithRetries(() => httpClient.GetByteArrayAsync(downloadUrl).GetAwaiter().GetResult());
        }

        private TContentResult MakePostRequestWithRetries<TRequest, TContentResult>(string relativeUrl,
            TRequest request)
        {
            var result = ExecuteWithRetries(() => httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/{relativeUrl}")
                {
                    Content = new StringContent(JsonConvert.SerializeObject(request, Formatting.Indented),
                        Encoding.UTF8, "application/json")
                }).GetAwaiter().GetResult());

            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw new WebException($"HTTP request unsuccessful. {result}");
            }

            return JsonConvert.DeserializeObject<TContentResult>(result.Content.ReadAsStringAsync().GetAwaiter()
                .GetResult());
        }

        private T ExecuteWithRetries<T>(Func<T> action)
        {
            T result;

            var tryCount = 0;
            while (true)
            {
                try
                {
                    result = action();
                    break;
                }
                catch (Exception e)
                {
                    tryCount++;
                    Console.WriteLine(e);
                    Console.WriteLine($"Encountered exception, try count {tryCount}");
                    if (tryCount >= 3)
                    {
                        throw;
                    }
                }
            }

            return result;
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

    public class GetTaskInfoResults
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


    internal class EnqueueTaskExportSpaceRequest
    {
        public EnqueueTaskExportSpaceRequest(string workspaceId)
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