using System.IO;
using Dropbox.Api;

namespace NotionExporter
{
    public class DropboxClientWrapper
    {
        public DropboxClientWrapper(string authToken)
        {
            dropboxClient = new DropboxClient(authToken);
        }

        public void BackupFileWithRotation(string fileName, byte[] content)
        {
            fileName = $"/{fileName}";
            dropboxClient.Files.UploadAsync(fileName, body: new MemoryStream(content)).GetAwaiter().GetResult();

            //enumerate
            //if more than 10, delete excess oldest
            //log something
        }

        private readonly DropboxClient dropboxClient;
    }
}