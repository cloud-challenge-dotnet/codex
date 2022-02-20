﻿using Codex.Core.Models;
using Codex.Tenants.Framework.Interfaces;
using Codex.Tenants.Framework.Resources;
using Codex.Tests.Framework;

namespace Codex.Tenants.Api.Tests
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)));

            services.AddSingleton(sp =>
                sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

            var options = Options.Create(new LocalizationOptions { ResourcesPath = "Resources" });
            var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
            var stringLocalizer = new StringLocalizer<TenantFrameworkResource>(factory);
            services.AddSingleton<IStringLocalizer<TenantFrameworkResource>>(stringLocalizer);

            services.AddSingleton<ITenantAccessService, TestTenantAccessService>();

            services.AddSingleton<DbFixture, DbFixture>();
            services.AddSingleton<ITenantRepository, TenantRepository>();
        }
    }
}
