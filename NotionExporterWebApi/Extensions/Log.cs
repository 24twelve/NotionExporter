using Serilog;
using Serilog.Core;

namespace NotionExporterWebApi.Extensions
{
    public static class Log
    {
        public static ILogger For<T>(T obj)
        {
            return For(typeof(T).Name);
        }

        public static ILogger For(string contextName)
        {
            return Serilog.Log.ForContext(Constants.SourceContextPropertyName, contextName);
        }
    }
}