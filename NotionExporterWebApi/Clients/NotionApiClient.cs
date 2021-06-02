using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static NotionExporterWebApi.Extensions.ActionExtensions;
using JsonSerializer = NotionExporterWebApi.Extensions.JsonSerializer;

namespace NotionExporterWebApi.Clients
{
    public class NotionApiClient : IDisposable
    {
        public NotionApiClient(string tokenV2)
        {
            var cookieContainer = new CookieContainer();
            var httpHandler = new HttpClientHandler { CookieContainer = cookieContainer };
            httpClient = new HttpClient(httpHandler)
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(15)
            };
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            cookieContainer.Add(httpClient.BaseAddress,
                new Cookie("token_v2",
                    tokenV2));
        }


        public async Task<string> PostEnqueueExportWorkspaceTaskAsync(string workspaceId)
        {
            var request = new EnqueueTaskExportSpaceRequest(workspaceId);
            var result = await MakePostRequestWithRetriesAsync<EnqueueTaskExportSpaceRequest, EnqueueTaskResult>(
                "enqueueTask",
                request).ConfigureAwait(false);
            if (result.TaskId == null)
            {
                throw new WebException(
                    $"Notion did not return task id for request {JsonSerializer.SerializeObject(request)}");
            }

            return result.TaskId;
        }

        public async Task<GetTaskInfoResult> PostGetTaskInfoAsync(string taskId)
        {
            var result = await MakePostRequestWithRetriesAsync<GetTaskInfoRequest, GetTaskInfoResults>("getTasks",
                new GetTaskInfoRequest(taskId)).ConfigureAwait(false);
            if (!result.Results.Any())
            {
                throw new WebException($"Notion did not return task info for task id {taskId}");
            }

            return result.Results.First();
        }


        public Task<byte[]> GetExportedWorkspaceZipAsync(string downloadUrl)
        {
            return ExecuteWithRetriesAsync(async () =>
                await httpClient.GetByteArrayAsync(downloadUrl).ConfigureAwait(false));
        }

        private async Task<TContentResult> MakePostRequestWithRetriesAsync<TRequest, TContentResult>(string relativeUrl,
            TRequest request) where TContentResult : class
        {
            var result = await ExecuteWithRetriesAsync(async () => await httpClient.SendAsync(
                new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/{relativeUrl}")
                {
                    Content = new StringContent(JsonSerializer.SerializeObject(request),
                        Encoding.UTF8, "application/json")
                }).ConfigureAwait(false)).ConfigureAwait(false);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw new WebException($"HTTP request unsuccessful. {result}");
            }

            var contentResult = await ExecuteWithRetriesAsync(
                async () => JsonSerializer.DeserializeObject<TContentResult>(await result.Content
                    .ReadAsStringAsync().ConfigureAwait(false))).ConfigureAwait(false);

            return contentResult!;
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }


        private const string BaseUrl = "https://www.notion.so/api/v3";

        private readonly HttpClient httpClient;
    }

    public class EnqueueTaskResult
    {
        [JsonProperty("taskId")]
        public string? TaskId { get; set; }
    }

    public class GetTaskInfoRequest
    {
        public GetTaskInfoRequest(string taskId)
        {
            TaskIds = new[] { taskId };
        }

        [JsonProperty("taskIds")]
        public string[] TaskIds { get; set; }
    }

    public class GetTaskInfoResults
    {
        [JsonProperty("results")]
        public GetTaskInfoResult[] Results { get; set; } = Array.Empty<GetTaskInfoResult>();
    }

    public class GetTaskInfoResult
    {
        [JsonProperty("id")]
        public string? TaskId { get; set; }

        [JsonProperty("state")]
        public TaskState? State { get; set; }

        [JsonProperty("status")]
        public ProgressStatus? ProgressStatus { get; set; }
    }

    public class ProgressStatus
    {
        [JsonProperty("pagesExported")]
        public int? PagesExported { get; set; }

        [JsonProperty("type")]
        public StatusType? Type { get; set; }

        [JsonProperty("exportURL")]
        public string? ExportUrl { get; set; }
    }


    public enum StatusType
    {
        [EnumMember(Value = "complete")]
        Complete
    }


    public enum TaskState
    {
        [EnumMember(Value = "in_progress")]
        InProgress,

        [EnumMember(Value = "success")]
        Success
    }


    internal class EnqueueTaskExportSpaceRequest
    {
        public EnqueueTaskExportSpaceRequest(string workspaceId)
        {
            Task = new NotionTask(workspaceId);
        }

        [JsonProperty("task")]
        public NotionTask Task { get; set; }

        internal class NotionTask
        {
            public NotionTask(string workspaceId)
            {
                Request = new NotionRequest(workspaceId);
            }

            [JsonProperty("eventName")]
            public string EventName { get; set; } = "exportSpace";

            [JsonProperty("request")]
            public NotionRequest Request { get; set; }

            internal class NotionRequest
            {
                public NotionRequest(string workspaceId)
                {
                    SpaceId = workspaceId;
                }

                [JsonProperty("exportOptions")]
                public ExportOptions Options { get; set; } = new ExportOptions();

                [JsonProperty("spaceId")]
                public string SpaceId;

                internal class ExportOptions
                {
                    [JsonProperty("exportType")]
                    public string ExportType { get; set; } = "markdown";

                    [JsonProperty("locale")]
                    public string Locale { get; set; } = "en";

                    [JsonProperty("timeZone")]
                    public string TimeZone { get; set; } = "Asia/Yekaterinburg";
                }
            }
        }
    }
}