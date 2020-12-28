using Autofac;
using Autofac.Extensions.DependencyInjection;
using Codex.Models.Tenants;
using Codex.Tenants.Framework.Implementations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Codex.Tenants.Framework
{
    [ExcludeFromCodeCoverage]
    public class MultiTenantServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
    {
        private readonly Action<Tenant?, ContainerBuilder> _tenantServicesConfiguration;

        public MultiTenantServiceProviderFactory(Action<Tenant?, ContainerBuilder> tenantServicesConfiguration)
        {
            _tenantServicesConfiguration = tenantServicesConfiguration;
        }

        /// <summary>
        /// Create a builder populated with global services
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public ContainerBuilder CreateBuilder(IServiceCollection services)
        {
            var builder = new ContainerBuilder();

            builder.Populate(services);

            return builder;
        }

        /// <summary>
        /// Create our serivce provider
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <returns></returns>
        public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
        {
            MultiTenantContainer? container = null;

            MultiTenantContainer? containerAccessor()
            {
                return container;
            }

            containerBuilder
                .RegisterInstance((Func<MultiTenantContainer?>)containerAccessor)
                .SingleInstance();

            container = new MultiTenantContainer(containerBuilder.Build(), _tenantServicesConfiguration);

            return new AutofacServiceProvider(containerAccessor());
        }
    }
}
