using System;
using System.IO;
using System.Linq;
using Dropbox.Api;
using Dropbox.Api.Files;

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
            Console.WriteLine($"Found {filesToRemove.Length} files older than {oldFileThreshold}");
            if (filesToRemove.Length > 0)
            {
                Console.WriteLine($"Deleting {filesToRemove.Length} old files");
                dropboxClient.Files.DeleteBatchAsync(filesToRemove).GetAwaiter().GetResult();
            }

            fileName = $"/{fileName}";
            ActionExtensions.ExecuteWithRetries(() => dropboxClient.Files.UploadAsync(fileName, body: new MemoryStream(content)).GetAwaiter().GetResult());
        }

        private readonly DropboxClient dropboxClient;
    }
}