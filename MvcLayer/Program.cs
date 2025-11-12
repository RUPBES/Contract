using Microsoft.AspNetCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using System.Net;


namespace MvcLayer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Ensure environment follows build configuration (Debug => Development, Release => Production)
#if DEBUG
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environments.Development);
#else
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environments.Production);
#endif
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(op =>
                    {
                        op.Listen(IPAddress.Parse("0.0.0.0"), 6972);
                        op.Limits.MaxRequestBodySize = int.MaxValue;
                    })
                    .UseStartup<Startup>();
                });

    }
}