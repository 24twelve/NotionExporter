using Serilog;
using Serilog.Core;

namespace NotionExporterWebApi.Extensions
{
    public static class Log
    {
        public static ILogger For<T>(T obj)
        {
            return Serilog.Log.ForContext(typeof(T));
        }

        public static ILogger For(string contextName)
        {
            return Serilog.Log.ForContext(propertyName: Constants.SourceContextPropertyName, contextName);
        }
    }
}