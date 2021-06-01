using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace NotionExporterWebApi
{
    public static class EntryPoint
    {
        //todo: nice deploy script on linux and nice logs filepath
        //todo: correct thorough async
        //todo: ping with current export state
        //todo: read out memry traffic places
        public static void Main(string[] args)
        {
            const string outputTemplate =
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [T-{ThreadId}] {Message:lj} {NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: outputTemplate)
                .WriteTo.File("bin/logs/log.txt", rollingInterval: RollingInterval.Day, outputTemplate:
                    outputTemplate, rollOnFileSizeLimit: true, fileSizeLimitBytes: 52_428_800)
                .CreateLogger();
            Extensions.Log.For("EntryPoint").Information("Logging started.");

            ThreadPoolUtility.SetUp();

            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .UseSystemd() //todo: use IF LINUX
                .UseSerilog();
        }
    }
}