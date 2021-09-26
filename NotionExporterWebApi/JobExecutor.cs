using System;
using System.Threading;
using System.Threading.Tasks;
using NotionExporterWebApi.Extensions;

namespace NotionExporterWebApi
{
    public class JobExecutor
    {
        public JobExecutor(string jobName, Func<CancellationToken, Task> job, TimeSpan period, TimeSpan timeBudget,
            CancellationToken cancellationToken)
        {
            this.jobName = jobName;
            this.job = job;
            this.period = period;
            this.timeBudget = timeBudget;
            this.cancellationToken = cancellationToken;
        }


        public async Task RunAsync()
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Log.For(this).Information($"Starting job {jobName}");
                bool? isCompletedInTime = null;
                while (!isCompletedInTime.HasValue || !isCompletedInTime.Value)
                {
                    isCompletedInTime = false;
#pragma warning disable 1998
                    await Task.Run(async () => isCompletedInTime =
                            job(cancellationToken) //вот в том треде пусть и работает
#pragma warning restore 1998
                                .Wait((int) timeBudget.TotalMilliseconds, cancellationToken), cancellationToken)
                        .ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (!isCompletedInTime.Value)
                    {
                        Log.For(jobName).Error("Job not completed within time budget of {0}, rerunning...",
                            timeBudget);
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                Log.For(this).Information("Job {0} finished, sleeping for {1}", jobName, period);
                Thread.Sleep(period);
            }
        }

        private readonly CancellationToken cancellationToken;
        private readonly Func<CancellationToken, Task> job;

        private readonly string jobName;
        private readonly TimeSpan period;
        private readonly TimeSpan timeBudget;
    }
}