using System;
using Newtonsoft.Json;
using JsonSerializer = NotionExporterWebApi.Extensions.JsonSerializer;

namespace NotionExporterWebApi
{
    public static class Config
    {
        public static void InitConfig(string configRaw)
        {
            var dto = JsonSerializer.DeserializeObject<ConfigDto>(configRaw);
            ValidateConfig(dto);

            NotionTokenV2 = dto!.NotionTokenV2!;
            DropboxAccessToken = dto!.DropboxAccessToken!;
            NotionWorkspaceId = dto!.NotionWorkspaceId!;
            UserName = dto!.UserName!;
            LogPath = dto!.LogPath!;
        }

        //todo: better throw on any null property
        private static void ValidateConfig(ConfigDto? configDto)
        {
            if (configDto == null)
            {
                throw new ArgumentNullException(nameof(configDto));
            }

            if (string.IsNullOrEmpty(configDto.NotionTokenV2))
            {
                throw new ArgumentNullException(configDto.NotionTokenV2);
            }

            if (string.IsNullOrEmpty(configDto.DropboxAccessToken))
            {
                throw new ArgumentNullException(configDto.DropboxAccessToken);
            }

            if (string.IsNullOrEmpty(configDto.NotionWorkspaceId))
            {
                throw new ArgumentNullException(configDto.NotionWorkspaceId);
            }

            if (string.IsNullOrEmpty(configDto.UserName))
            {
                throw new ArgumentNullException(configDto.UserName);
            }

            if (string.IsNullOrEmpty(configDto.LogPath))
            {
                throw new ArgumentNullException(configDto.LogPath);
            }
        }

        [JsonProperty("notion-token-v2")]
        public static string NotionTokenV2 { get; set; } = "";

        [JsonProperty("dropbox-access-token")]
        public static string DropboxAccessToken { get; set; } = "";

        [JsonProperty("notion-workspace-id")]
        public static string NotionWorkspaceId { get; set; } = "";

        [JsonProperty("username")]
        public static string UserName { get; set; } = "";

        [JsonProperty("log-path")]
        public static string LogPath { get; set; } = "";
    }

    public class ConfigDto
    {
        [JsonProperty("notion-token-v2")]
        public string? NotionTokenV2 { get; set; }

        [JsonProperty("dropbox-access-token")]
        public string? DropboxAccessToken { get; set; }

        [JsonProperty("notion-workspace-id")]
        public string? NotionWorkspaceId { get; set; }

        [JsonProperty("username")]
        public string? UserName { get; set; }

        [JsonProperty("log-path")]
        public string? LogPath { get; set; }
    }
}