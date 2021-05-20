using System;
using System.IO;
using System.Linq;
using Dropbox.Api;
using Dropbox.Api.Files;
using Serilog;

namespace NotionExporter
{
    public class DropboxClientWrapper
    {
        public DropboxClientWrapper(string authToken)
        {
            dropboxClient = new DropboxClient(authToken);
        }

        public void UploadFileAndRotateOldFiles(string fileName, byte[] content, DateTime now)
        {
            var expirationTimespan = TimeSpan.FromDays(10);
            var oldFileThreshold = now - expirationTimespan;

            //note: if some problem with a lot of files occur, use has_more field and iterate through all
            //note: we assume that update date == creation date because we never modify stored .zip files
            var filesToRemove = dropboxClient.Files.ListFolderAsync("").GetAwaiter().GetResult().Entries
                .Where(x => x.IsFile)
                .Select(x => x.AsFile)
                .Where(x => x.ServerModified < oldFileThreshold ||
                            x.ClientModified < oldFileThreshold)
                .Select(x => new DeleteArg(x.PathLower))
                .ToArray();
            Log.Information("Found {count} files older than {oldFileThreshold}", filesToRemove.Length,
                oldFileThreshold);
            if (filesToRemove.Length > 0)
            {
                Log.Information("Deleting {filesToRemove.Length} old files", filesToRemove.Length);
                dropboxClient.Files.DeleteBatchAsync(filesToRemove).GetAwaiter().GetResult();
            }

            fileName = $"/{fileName}";
            ActionExtensions.ExecuteWithRetries(() =>
                dropboxClient.Files.UploadAsync(fileName, body: new MemoryStream(content)).GetAwaiter().GetResult());
        }

        private readonly DropboxClient dropboxClient;
    }
}