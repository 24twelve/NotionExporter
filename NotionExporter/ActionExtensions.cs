using System;
using Serilog;

namespace NotionExporter
{
    public static class ActionExtensions
    {
        public static T ExecuteWithRetries<T>(Func<T> action)
        {
            T result;

            var tryCount = 0;
            while (true)
            {
                try
                {
                    result = action();
                    break;
                }
                catch (Exception e)
                {
                    tryCount++;
                    Log.Error("Encountered exception {e} on try {tryCount}", e, tryCount);
                    if (tryCount >= 3)
                    {
                        throw;
                    }
                }
            }

            return result;
        }
    }
}