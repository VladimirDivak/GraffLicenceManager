using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace GraffLicenceManager
{
    public enum CardPosition
    {
        Top,
        Center,
        Bottom,
        Full
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel(x => x.Listen(IPAddress.Loopback, 5000));
                    webBuilder.UseStartup<Startup>();
                });
    }
}