using Codex.Core.Models;
using Codex.Tenants.Framework.Interfaces;
using Codex.Tests.Framework;
using Codex.Security.Api.Repositories.Implementations;
using Codex.Security.Api.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Codex.Security.Api.Tests
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)));

            services.AddSingleton(sp =>
                sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

            services.AddSingleton<ITenantAccessService, TestTenantAccessService>();

            services.AddSingleton<DbFixture, DbFixture>();
            services.AddSingleton<IApiKeyRepository, ApiKeyRepository>();
        }
    }
}
