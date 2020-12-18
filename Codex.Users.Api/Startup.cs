using Autofac;
using Codex.Core;
using Codex.Core.Implementations;
using Codex.Core.Interfaces;
using Codex.Core.Models;
using Codex.Core.Roles.Interfaces;
using Codex.Tenants.Framework;
using Codex.Tenants.Framework.Implementations;
using Codex.Models.Tenants;
using Codex.Users.Api.Repositories.Interfaces;
using Codex.Users.Api.Services.Implementations;
using Codex.Users.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using Codex.Core.Cache;
using Codex.Core.RazorHelpers.Implementations;
using Codex.Core.RazorHelpers.Interfaces;
using Codex.Users.Api.Repositories.Implementations;
using Codex.Core.ApiKeys.Extensions;
using Codex.Core.ApiKeys.Models;
using Codex.Core.Roles.Implementations;
using Codex.Core.Tools;

namespace Codex.Users.Api
{
    [ExcludeFromCodeCoverage]
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
            services.Configure<MongoDbSettings>(Configuration.GetSection(nameof(MongoDbSettings)));

            services.AddSingleton(sp =>
                sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

            services.AddRazorPages();

            services.AddDaprClient(configure =>
            {
                var options = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    IgnoreNullValues = true
                };
                // Adds automatic json parsing to ObjectId.
                options.Converters.Add(new ObjectIdConverter());
                options.Converters.Add(new JsonStringEnumConverter());

                configure.UseJsonSerializationOptions(options);
            });

            services.AddSingleton<IExceptionHandler, CoreExceptionHandler>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<IRoleProvider, DefaultRoleProvider>();
            services.AddSingleton<IRoleService, RoleService>();
            services.AddSingleton<TenantCacheService, TenantCacheService>();
            services.AddSingleton<ApiKeyCacheService, ApiKeyCacheService>();
            services.AddSingleton<IUserRepository, UserRepository>(); // for try authenticate without tenantId inside request header
            services.AddSingleton<IUserService, UserService>(); // for try authenticate without tenantId inside request header
            services.AddSingleton<IAuthenticationService, AuthenticationService>(); // for try authenticate without tenantId inside request header
            services.AddSingleton<IMailService, MailJetMailService>();

            services.AddMultiTenancy()
                .WithResolutionStrategy<HostTenantResolutionStrategy>()
                .WithStore<TenantStore>();

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.IgnoreNullValues = true;
                // Adds automatic json parsing to ObjectId.
                options.JsonSerializerOptions.Converters.Add(new ObjectIdConverter());
            }).AddDapr();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Codex.Users.Api", Version = "v1" });
            });

            var jwtSecret = Encoding.ASCII.GetBytes(Configuration.GetValue<string>(ConfigConstant.JwtSecretKey));

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = "customAuthScheme";
                x.DefaultChallengeScheme = "customAuthScheme";
            })
            .AddPolicyScheme("customAuthScheme", "Authorization Bearer or ApiKey", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (context.Request.Headers.ContainsKey(HttpHeaderConstant.ApiKey))
                    {
                        return ApiKeyAuthenticationOptions.DefaultScheme;
                    }

                    return JwtBearerDefaults.AuthenticationScheme;
                };
            })
            .AddApiKeySupport(options => { })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtSecret),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }
        public static void ConfigureMultiTenantServices(Tenant? tenant, ContainerBuilder containerBuilder)
        {
            //This action has access to the tenant object so we can perform tenant specific logic here 
            //when deciding which services to register
            //These instances are scoped to the current tenant, so in this example 
            //it will be one instance per tenant
            _ = tenant;

            containerBuilder.RegisterType<RazorPartialToStringRenderer>().As<IRazorPartialToStringRenderer>().SingleInstance();
            containerBuilder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<UserMailService>().As<IUserMailService>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<AuthenticationService>().As<IAuthenticationService>().InstancePerLifetimeScope();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            IEnumerable<IExceptionHandler> exceptionHandlers)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "Codex.Users.Api v1"));
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseExceptionHandler(app => app.UseCustomErrors(env, exceptionHandlers));

            app.UseRouting();

            app.UseCloudEvents();

            app.UseMultiTenancy()
                .UseMultiTenantContainer();
            
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSubscribeHandler();
                endpoints.MapControllers();
            });
        }
    }
}
