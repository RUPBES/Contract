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
                        op.Limits.MaxRequestBodySize = null;
                    })
                    .UseStartup<Startup>();
                });

    }

    //public class Program
    //{
    //    public static void Main(string[] args)
    //    {
    //        // �������� ���� � �����
    //        var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
    //        //���� � �������� �������
    //        var pathToContentRoot = Path.GetDirectoryName(pathToExe);
    //        // ������� ����
    //        var host = WebHost.CreateDefaultBuilder(args)
    //            //.ConfigureKestrel(op =>
    //            //    {
    //            //        op.Listen(IPAddress.Parse("0.0.0.0"), 6972);
    //            //        op.Limits.MaxRequestBodySize = null;
    //            //    })
    //            .UseKestrel(op =>
    //            {
    //                op.Limits.MaxRequestBodySize = null;
    //            })
    //                .UseContentRoot(pathToContentRoot)
    //                .UseStartup<Startup>()
    //                .UseUrls("http://0.0.0.0:6972/")
    //                .Build();
    //        //��������� � ���� ������
    //        host.RunAsService();
    //    }
    //}
}