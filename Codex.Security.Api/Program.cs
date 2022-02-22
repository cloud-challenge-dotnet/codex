using Codex.Tenants.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;
using Dapr.Client;
using Dapr.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Codex.Security.Api;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        // Create Dapr Client
        var daprClient = new DaprClientBuilder()
            .Build();
        
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((services) =>
            {
                // Add the DaprClient to DI.
                services.AddSingleton(daprClient);
            })
            .ConfigureAppConfiguration((configBuilder) =>
            {
                // To retrive specific secrets use secretDescriptors
                // Create descriptors for the secrets you want to rerieve from the Dapr Secret Store.
                // var secretDescriptors = new DaprSecretDescriptor[]
                // {
                //     new DaprSecretDescriptor("super-secret")
                // };
                // configBuilder.AddDaprSecretStore("demosecrets", secretDescriptors, client);

                // Add the secret store Configuration Provider to the configuration builder.
                configBuilder.AddDaprSecretStore("codex", daprClient);
            })
            .UseServiceProviderFactory(
                new MultiTenantServiceProviderFactory(Startup.ConfigureMultiTenantServices)
            )
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}