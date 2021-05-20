using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace NotionExporterWebApi
{
    public class NotionExporterHostedService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            File.AppendAllText("c:\\temp\\123.txt", "I am working");
        }
    }
}