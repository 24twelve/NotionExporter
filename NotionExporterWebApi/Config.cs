using System;
using Newtonsoft.Json;
using NotionExporterWebApi.Extensions;
using JsonSerializer = NotionExporterWebApi.Extensions.JsonSerializer;

namespace NotionExporterWebApi
{
    public static class Config
    {
        public static void InitConfig(string configRaw)
        {
            var dto = JsonSerializer.DeserializeObject<ConfigDto>(configRaw);
            ValidateConfig(dto);

            ApplicationEnvironment = dto!.ApplicationEnvironment!.Value;
            NotionTokenV2 = dto!.NotionTokenV2!;
            IntegrationToken = dto!.IntegrationToken!;
            DropboxAccessToken = dto!.DropboxAccessToken!;
            NotionWorkspaceId = dto!.NotionWorkspaceId!;
            LogPath = dto!.LogPath!;
            RawConfig = dto;

            RawConfig.DropboxAccessToken = "SECRET";
            RawConfig.NotionTokenV2 = "SECRET";
            RawConfig.NotionWorkspaceId = "SECRET";
            RawConfig.IntegrationToken = "SECRET";
        }

        public static string ToPrettyJson()
        {
            return RawConfig.ToPrettyJson();
        }

        //todo: better throw on any null property
        //todo: ipse
        //todo: not store raw config, get better idea for config serialization
        private static void ValidateConfig(ConfigDto? configDto)
        {
            if (configDto == null)
            {
                throw new Exception(nameof(configDto));
            }

            if (!configDto.ApplicationEnvironment.HasValue)
            {
                throw new Exception(configDto.ApplicationEnvironment.ToString());
            }

            if (string.IsNullOrEmpty(configDto.NotionTokenV2))
            {
                throw new Exception("Null config option NotionTokenV2");
            }

            if (string.IsNullOrEmpty(configDto.IntegrationToken))
            {
                throw new Exception("Null config option IntegrationToken");
            }

            if (string.IsNullOrEmpty(configDto.DropboxAccessToken))
            {
                throw new Exception("Null config option DropboxAccessToken");
            }

            if (string.IsNullOrEmpty(configDto.NotionWorkspaceId))
            {
                throw new Exception("Null config option NotionWorkspaceId");
            }

            if (string.IsNullOrEmpty(configDto.LogPath))
            {
                throw new Exception("Null config option LogPath");
            }
        }

        public static ApplicationEnvironment ApplicationEnvironment { get; set; }

        public static string NotionTokenV2 { get; set; } = "";

        public static string IntegrationToken { get; set; } = "";

        public static string DropboxAccessToken { get; set; } = "";

        public static string NotionWorkspaceId { get; set; } = "";

        public static string LogPath { get; set; } = "";

        private static ConfigDto? RawConfig { get; set; }
    }

    public class ConfigDto
    {
        [JsonProperty("application-environment")]
        public ApplicationEnvironment? ApplicationEnvironment { get; set; }

        [JsonProperty("notion-token-v2")]
        public string? NotionTokenV2 { get; set; }

        [JsonProperty("integration-token")]
        public string? IntegrationToken { get; set; }

        [JsonProperty("dropbox-access-token")]
        public string? DropboxAccessToken { get; set; }

        [JsonProperty("notion-workspace-id")]
        public string? NotionWorkspaceId { get; set; }

        [JsonProperty("log-path")]
        public string? LogPath { get; set; }
    }
}