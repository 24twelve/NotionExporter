using System;
using System.Threading;
using System.Threading.Tasks;

namespace NotionExporterWebApi
{
    public class JobExecutor
    {
        public JobExecutor(string jobName, Func<CancellationToken, Task> job, TimeSpan period,
            CancellationToken cancellationToken)
        {
            this.jobName = jobName;
            this.job = job;
            this.period = period;
            this.cancellationToken = cancellationToken;
        }


        public async Task RunAsync()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Log.For(this).Information($"Starting job {jobName}");
                await job(cancellationToken).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                Log.For(this).Information($"Job {jobName} finished, sleeping for {{0}}", period);
                Thread.Sleep(period);
            }
        }

        private readonly CancellationToken cancellationToken;
        private readonly Func<CancellationToken, Task> job;

        private readonly string jobName;
        private readonly TimeSpan period;
    }
}