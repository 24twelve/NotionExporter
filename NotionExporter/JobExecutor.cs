using System;
using System.Threading;
using Serilog;

namespace NotionExporter
{
    public class JobExecutor
    {
        public JobExecutor(string jobName, Action<CancellationTokenSource> job, TimeSpan period,
            CancellationTokenSource cancellationTokenSource)
        {
            this.jobName = jobName;
            this.job = job;
            this.period = period;
            this.cancellationTokenSource = cancellationTokenSource;
        }


        public void Run()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                Log.Information($"Starting job {jobName}");
                job(cancellationTokenSource);
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }

                Log.Information($"Job {jobName} finished, sleeping for {{0}}", period);
                Thread.Sleep(period);
            }
        }

        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Action<CancellationTokenSource> job;

        private readonly string jobName;
        private readonly TimeSpan period;
    }
}