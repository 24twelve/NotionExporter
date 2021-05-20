using System;
using Serilog;

namespace NotionExporter
{
    public static class ActionExtensions
    {
        public static T ExecuteWithRetries<T>(Func<T> action)
        {
            var tryCount = 0;
            while (true)
            {
                try
                {
                    var result = action();
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