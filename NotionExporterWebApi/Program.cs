using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace NotionExporterWebApi
{
    public static class Program
    {
        //todo: ctrl+c cancellation
        //todo: logging context and thread id
        //todo: correct thorough async and thread pool
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("bin/logs/log.txt", rollingInterval: RollingInterval.Minute)
                .CreateLogger();
            Log.Information("Logging started.");
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