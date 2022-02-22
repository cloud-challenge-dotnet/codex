using Codex.Tenants.Framework.Implementations;
using Codex.Tenants.Framework.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Tenants.Framework;

/// <summary>
/// Configure tenant services
/// </summary>
[ExcludeFromCodeCoverage]
public class TenantBuilder
{
    private readonly IServiceCollection _services;

    public TenantBuilder(IServiceCollection services)
    {
        services.AddTransient<ITenantAccessService, TenantAccessService>();
        _services = services;
    }

    /// <summary>
    /// Register the tenant resolver implementation
    /// </summary>
    /// <typeparam name="TV"></typeparam>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public TenantBuilder WithResolutionStrategy<TV>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TV : class, ITenantResolutionStrategy
    {
        _services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        _services.Add(ServiceDescriptor.Describe(typeof(ITenantResolutionStrategy), typeof(TV), lifetime));
        return this;
    }

    /// <summary>
    /// Register the tenant store implementation
    /// </summary>
    /// <typeparam name="TV"></typeparam>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    public TenantBuilder WithStore<TV>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TV : class, ITenantStore
    {
        _services.Add(ServiceDescriptor.Describe(typeof(ITenantStore), typeof(TV), lifetime));
        return this;
    }
}