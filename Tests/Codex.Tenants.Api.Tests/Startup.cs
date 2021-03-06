﻿using Codex.Core.Models;
using Codex.Tenants.Api.Repositories.Implementations;
using Codex.Tenants.Api.Repositories.Interfaces;
using Codex.Tenants.Framework.Interfaces;
using Codex.Tenants.Framework.Resources;
using Codex.Tests.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Globalization;

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
