using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLS.Lib;

namespace NLS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (Server.Connect())
            {
                // TODO: Logger - 'Connected'
                CreateHostBuilder(args).Build().Run();
            }
            else
            {
                //TODO: Logger - 'Disconnected'
                Server.Disconnect();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
