using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Codex.Tenants.Framework.Implementations;
using Codex.Tenants.Models;
using System;

namespace Codex.Tenants.Framework
{
    public class MultiTenantServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
    {
        public Action<Tenant?, ContainerBuilder> _tenantServicesConfiguration;

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

            Func<MultiTenantContainer?> containerAccessor = () =>
            {
                return container;
            };

            containerBuilder
                .RegisterInstance(containerAccessor)
                .SingleInstance();

            container = new MultiTenantContainer(containerBuilder.Build(), _tenantServicesConfiguration);

            return new AutofacServiceProvider(containerAccessor());
        }
    }
}
