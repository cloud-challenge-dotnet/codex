using Autofac;
using Codex.Core;
using Codex.Core.Interfaces;
using Codex.Core.Models;
using Codex.Tenants.Api.Services;
using Codex.Tenants.Framework;
using Codex.Tenants.Framework.Implementations;
using Codex.Tenants.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Codex.Tenants.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // requires using Microsoft.Extensions.Options
            services.Configure<MongoDbSettings>(Configuration.GetSection(nameof(MongoDbSettings)));

            services.AddSingleton(sp =>
                sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

            services.AddSingleton<IExceptionHandler, CoreExceptionHandler>();
            services.AddSingleton<IExceptionHandler, ExceptionHandler2>();

            services.AddMultiTenancy()
                .WithResolutionStrategy<GlobalTenantResolutionStrategy>()
                .WithStore<InMemoryTenantStore>();

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.IgnoreNullValues = true;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TenantApi", Version = "v1" });
            });
        }

        public static void ConfigureMultiTenantServices(Tenant? tenant, ContainerBuilder containerBuilder)
        {
            //This action has access to the tenant object so we can perform tenant specific logic here 
            //when deciding which services to register
            //These instances are scoped to the current tenant, so in this example 
            //it will be one instance per tenant
            _= tenant;

            containerBuilder.RegisterType<TenantRepository>().As<ITenantRepository>().SingleInstance();
            containerBuilder.RegisterType<TenantService>().As<ITenantService>().SingleInstance();
            containerBuilder.RegisterType<TenantPropertiesService>().As<ITenantPropertiesService>().SingleInstance();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IEnumerable<IExceptionHandler> exceptionHandlers)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TenantApi v1"));
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseExceptionHandler(app => app.UseCustomErrors(env, exceptionHandlers));

            app.UseRouting();

            app.UseMultiTenancy()
                .UseMultiTenantContainer();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
