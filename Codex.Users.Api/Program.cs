using Codex.Tenants.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Codex.Users.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(
                    new MultiTenantServiceProviderFactory(Startup.ConfigureMultiTenantServices)
                )
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
