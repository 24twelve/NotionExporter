using System;
using System.Threading.Tasks;
using Serilog;

namespace NotionExporterWebApi
{
    public static class ActionExtensions
    {
        public static async Task<T> ExecuteWithRetriesAsync<T>(Func<Task<T>> action)
        {
            var tryCount = 0;
            while (true)
            {
                try
                {
                    var result = await action().ConfigureAwait(false);
                    if (result != null)
                    {
                        return result;
                    }

                    tryCount++;
                }
                catch (Exception e)
                {
                    Log.Error("Encountered exception {e} on try {tryCount}", e, tryCount);
                    if (tryCount >= 3)
                    {
                        throw;
                    }
                }
            }
        }
    }
}