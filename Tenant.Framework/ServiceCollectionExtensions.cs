using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Codex.Tenants.Framework.Implementations;
using Codex.Tenants.Models;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Tenants.Framework
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Nice method to create the tenant builder
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the services (default tenant class)
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static TenantBuilder AddMultiTenancy(this IServiceCollection services)
            => new TenantBuilder(services);

        public static IServiceProvider UseMultiTenantServiceProvider(this IServiceCollection services, Action<Tenant?, ContainerBuilder> registerServicesForTenant)
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();

            //Declare our container and create a accessor function
            //This is to support the Func<MultiTenantContainer<T>> multiTenantContainerAccessor parameter in the middleware
            MultiTenantContainer? container = null;
            MultiTenantContainer? containerAccessor()
            {
                return container;
            }
            services.AddSingleton((Func<MultiTenantContainer?>)containerAccessor);

            //Add all the application level services to the builder
            containerBuilder.Populate(services);

            //Create and assign the new multiteant container
            container = new MultiTenantContainer(containerBuilder.Build(), registerServicesForTenant);

            //Return the new IServiceProvider which will be used to replace the standard one
            return new AutofacServiceProvider(containerAccessor());
        }
    }
}
