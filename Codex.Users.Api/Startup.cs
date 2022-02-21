using Autofac;
using Codex.Core;
using Codex.Core.Cache;
using Codex.Core.Implementations;
using Codex.Core.Interfaces;
using Codex.Core.Models;
using Codex.Core.RazorHelpers.Implementations;
using Codex.Core.RazorHelpers.Interfaces;
using Codex.Core.Roles.Implementations;
using Codex.Core.Roles.Interfaces;
using Codex.Core.Tools;
using Codex.Core.Tools.AutoMapper;
using Codex.Models.Tenants;
using Codex.Tenants.Framework;
using Codex.Tenants.Framework.Implementations;
using Codex.Users.Api.Repositories.Implementations;
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Codex.Core.Authentication;
using Codex.Core.Authentication.Extensions;
using Codex.Core.Authentication.Models;
using Codex.Users.Api.GrpcServices;
using Codex.Users.Api.MappingProfiles;
using CodexGrpc.Tenants;
using Dapr.Client;

namespace Codex.Users.Api;

[ExcludeFromCodeCoverage]
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLocalization();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new List<CultureInfo>()
            {
                new CultureInfo("en-US"),
                new CultureInfo("fr-FR")
            };

            options.DefaultRequestCulture = new(culture: "en-US", uiCulture: "en-US");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.ApplyCurrentCultureToResponseHeaders = true;
            options.FallBackToParentUICultures = true;
        });

        services.Configure<MongoDbSettings>(Configuration.GetSection(nameof(MongoDbSettings)));

        services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

        services.AddGrpc(options =>
        {
            options.Interceptors.Add<CoreExceptionGrpcInterceptor>();
        });
        services.AddGrpcReflection();
        
        services.AddRazorPages();

        services.AddDaprClient(configure =>
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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
        services.AddSingleton(_ =>
        {
            var callInvoker = DaprClient.CreateInvocationInvoker(ApiNameConstant.TenantApi);
            return new TenantService.TenantServiceClient(
                callInvoker
            );
        });
        services.AddSingleton<ITenantCacheService, TenantCacheService>();
        services.AddSingleton<IApiKeyCacheService, ApiKeyCacheService>();
        services.AddSingleton<IUserRepository, UserRepository>(); // for try authenticate without tenantId inside request header
        services.AddSingleton<IUserService, UserService>(); // for try authenticate without tenantId inside request header
        services.AddSingleton<IAuthenticationService, AuthenticationService>(); // for try authenticate without tenantId inside request header
        services.AddSingleton<IMailService, MailJetMailService>();

        services.AddMultiTenancy()
            .WithResolutionStrategy<HostTenantResolutionStrategy>()
            .WithStore<TenantStore>();

        services.AddAutoMapper(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile<CoreMappingProfile>();
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<Codex.Core.MappingProfiles.GrpcMappingProfile>();
            cfg.AddProfile<GrpcMappingProfile>();
        }, typeof(Startup), typeof(CoreMappingProfile));

        services.AddControllers()
            //Add controller from Codex.Core
            .AddApplicationPart(typeof(Codex.Core.Controllers.CacheControllerBase<>).Assembly)
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                // Adds automatic json parsing to ObjectId.
                options.JsonSerializerOptions.Converters.Add(new ObjectIdConverter());
            })
            .AddDapr();

        services.AddCors(options =>
        {
            options.AddPolicy(
                "Open",
                builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
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
            .AddApiKeySupport()
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
        }
        else
        {
            app.UseHttpsRedirection();
        }

        var localizationOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
        app.UseRequestLocalization(localizationOptions!.Value);

        app.UseExceptionHandler(appBuilder => appBuilder.UseCustomErrors(env, exceptionHandlers));

        app.UseRouting();

        app.UseCors("Open");

        app.UseCloudEvents();

        app.UseMultiTenancy()
            .UseMultiTenantContainer();
        
        app.UseDaprApiToken();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapSubscribeHandler();
            endpoints.MapControllers();
                
            endpoints.MapGrpcService<AuthenticationGrpcService>();

            if (env.IsDevelopment())
            {
                endpoints.MapGrpcReflectionService();
            }
        });
    }
}