using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace NotionExporterWebApi
{
    public class DropboxClientWrapper
    {
        public DropboxClientWrapper(string authToken)
        {
            dropboxClient = new DropboxClient(authToken);
        }

        public async Task UploadFileAndRotateOldFilesAsync(string fileName, byte[] content, DateTime now)
        {
            var expirationTimespan = TimeSpan.FromDays(10);
            var oldFileThreshold = now - expirationTimespan;
            const int countThreshold = 30;

            //note: if some problem with a lot of files occur, use has_more field and iterate through all
            //note: we assume that update date == creation date because we never modify stored .zip files
            var allFiles = await dropboxClient.Files.ListFolderAsync("").ConfigureAwait(false);
            var oldFiles = allFiles.Entries
                .Where(x => x.IsFile)
                .Select(x => x.AsFile)
                .Where(x => x.ServerModified < oldFileThreshold ||
                            x.ClientModified < oldFileThreshold)
                .Select(x => new DeleteArg(x.PathLower))
                .ToArray();
            Log.For(this).Information("Found {count} files older than {oldFileThreshold}", oldFiles.Length,
                oldFileThreshold);
            if (oldFiles.Length > 0)
            {
                Log.For(this).Information("Deleting {filesToRemove.Length} old files", oldFiles.Length);
                await dropboxClient.Files.DeleteBatchAsync(oldFiles).ConfigureAwait(false);
            }

            allFiles = await dropboxClient.Files.ListFolderAsync("").ConfigureAwait(false);
            var excessFiles = allFiles.Entries
                .Where(x => x.IsFile)
                .Select(x => x.AsFile)
                .OrderByDescending(x => x.ServerModified)
                .Skip(countThreshold)
                .Select(x => new DeleteArg(x.PathLower))
                .ToArray();

            Log.For(this).Information("Found {count} files more than allowed number {countThreshold}",
                excessFiles.Length,
                countThreshold);
            if (excessFiles.Length > 0)
            {
                Log.For(this).Information("Deleting {0} excess files", excessFiles.Length);
                await dropboxClient.Files.DeleteBatchAsync(excessFiles).ConfigureAwait(false);
            }

            fileName = $"/{fileName}";
            await ActionExtensions.ExecuteWithRetriesAsync(async () =>
                    await dropboxClient.Files.UploadAsync(fileName, body: new MemoryStream(content))
                        .ConfigureAwait(false))
                .ConfigureAwait(false);
        }

        private readonly DropboxClient dropboxClient;
    }
}