using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Log = NotionExporterWebApi.Extensions.Log;

namespace NotionExporterWebApi
{
    public static class EntryPoint
    {
        //todo: reduce mess with config (probably will be gone with DI)
        //todo: work on all ! annotations
        //todo: correct thorough async
        //todo: ping with current export state
        //todo: ping and log with current export state
        //todo: DI
        //todo: really later
        //todo: correct thorough async
        //todo: read out memry traffic places
        public static void Main(string[] args)
        {
            Config.InitConfig(File.ReadAllText("secrets/runtime-config.json"));
            ThreadPoolUtility.SetUp();
            InitLogging();
            Log.For("EntryPoint").Information("Logging started.");
            Log.For("EntryPoint").Information($"Start application with config {Config.ToPrettyJson()}");
            CreateHostBuilder(args).Build().Run();
        }

        private static void InitLogging()
        {
            const string outputTemplate =
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [T-{ThreadId}] {Message:lj} {NewLine}{Exception}";
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate: outputTemplate)
                .WriteTo.File(Config.LogPath, rollingInterval: RollingInterval.Day, outputTemplate:
                    outputTemplate, rollOnFileSizeLimit: true, fileSizeLimitBytes: 52_428_800)
                .CreateLogger();
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