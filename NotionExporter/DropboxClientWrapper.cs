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
            const int countThreshold = 30;

            //note: if some problem with a lot of files occur, use has_more field and iterate through all
            //note: we assume that update date == creation date because we never modify stored .zip files
            var oldFiles = dropboxClient.Files.ListFolderAsync("").GetAwaiter().GetResult().Entries
                .Where(x => x.IsFile)
                .Select(x => x.AsFile)
                .Where(x => x.ServerModified < oldFileThreshold ||
                            x.ClientModified < oldFileThreshold)
                .Select(x => new DeleteArg(x.PathLower))
                .ToArray();
            Log.Information("Found {count} files older than {oldFileThreshold}", oldFiles.Length,
                oldFileThreshold);
            if (oldFiles.Length > 0)
            {
                Log.Information("Deleting {filesToRemove.Length} old files", oldFiles.Length);
                dropboxClient.Files.DeleteBatchAsync(oldFiles).GetAwaiter().GetResult();
            }

            var excessFiles = dropboxClient.Files.ListFolderAsync("").GetAwaiter().GetResult().Entries
                .Where(x => x.IsFile)
                .Select(x => x.AsFile)
                .OrderByDescending(x => x.ServerModified)
                .Skip(countThreshold)
                .Select(x => new DeleteArg(x.PathLower))
                .ToArray();

            Log.Information("Found {count} files more than allowed number {countThreshold}", excessFiles.Length,
                countThreshold);
            if (excessFiles.Length > 0)
            {
                Log.Information("Deleting {0} excess files", excessFiles.Length);
                dropboxClient.Files.DeleteBatchAsync(excessFiles).GetAwaiter().GetResult();
            }

            fileName = $"/{fileName}";
            ActionExtensions.ExecuteWithRetries(() =>
                dropboxClient.Files.UploadAsync(fileName, body: new MemoryStream(content)).GetAwaiter().GetResult());
        }

        private readonly DropboxClient dropboxClient;
    }
}