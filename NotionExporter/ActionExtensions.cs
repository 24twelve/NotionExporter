using System;

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
                    Console.WriteLine(e);
                    Console.WriteLine($"Encountered exception, try count {tryCount}");
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