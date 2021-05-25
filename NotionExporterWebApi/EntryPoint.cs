using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace NotionExporterWebApi
{
    public static class EntryPoint
    {
        //todo:  //"not started job fix
        //todo: correct thorough async and thread pool
        public static void Main(string[] args)
        {
            const string outputTemplate =
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [T-{ThreadId}] {Message:lj} {NewLine}{Exception}";
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: outputTemplate)
                .WriteTo.File("bin/logs/log.txt", rollingInterval: RollingInterval.Day, outputTemplate:
                    outputTemplate)
                .CreateLogger();
            Log.For("EntryPoint").Information("Logging started.");
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .UseSerilog();
        }
    }
}