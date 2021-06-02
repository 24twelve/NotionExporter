using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NotionExporterWebApi.Extensions;

namespace NotionExporterWebApi.Services
{
    public class ApplicationStateLoggingService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer(
                x => Log.For("ThreadPool").Information(ThreadPoolUtility.GetThreadPoolState().ToString()), null,
                period: TimeSpan.FromMinutes(1), dueTime: TimeSpan.Zero);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private Timer? timer;
    }
}