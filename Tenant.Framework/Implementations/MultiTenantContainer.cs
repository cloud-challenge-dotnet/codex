using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;
using Codex.Tenants.Framework.Interfaces;
using Codex.Tenants.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Codex.Tenants.Framework.Implementations
{
    internal class MultiTenantContainer : IContainer
    {
        //This is the base application container
        private readonly IContainer _applicationContainer;
        //This action configures a container builder
        private readonly Action<Tenant?, ContainerBuilder> _tenantContainerConfiguration;

        //This dictionary keeps track of all of the tenant scopes that we have created
        private readonly Dictionary<string, ILifetimeScope> _tenantLifetimeScopes = new Dictionary<string, ILifetimeScope>();

        private readonly object _lock = new object();
        private const string _multiTenantTag = "multitenantcontainer";

        public MultiTenantContainer(IContainer applicationContainer, Action<Tenant?, ContainerBuilder> containerConfiguration)
        {
            _tenantContainerConfiguration = containerConfiguration;
            _applicationContainer = applicationContainer;
        }

        /// <summary>
        /// Get the current teanant from the application container
        /// </summary>
        /// <returns></returns>
        private Tenant? GetCurrentTenant()
        {
            //We have registered our TenantAccessService in Part 1, the service is available in the application container which allows us to access the current Tenant
            return _applicationContainer.Resolve<ITenantAccessService>().GetTenantAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get the scope of the current tenant
        /// </summary>
        /// <returns></returns>
        public ILifetimeScope GetCurrentTenantScope()
        {
            return GetTenantScope(GetCurrentTenant()?.Id);
        }

        /// <summary>
        /// Get (configure on missing)
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public ILifetimeScope GetTenantScope(string? tenantId)
        {
            //If no tenant (e.g. early on in the pipeline, we just use the application container)
            if (tenantId == null)
                return _applicationContainer;

            //If we have created a lifetime for a tenant, return
            if (_tenantLifetimeScopes.ContainsKey(tenantId))
                return _tenantLifetimeScopes[tenantId];

            lock (_lock)
            {
                if (_tenantLifetimeScopes.ContainsKey(tenantId))
                {
                    return _tenantLifetimeScopes[tenantId];
                }
                else
                {
                    //This is a new tenant, configure a new lifetimescope for it using our tenant sensitive configuration method
                    _tenantLifetimeScopes.Add(tenantId, _applicationContainer.BeginLifetimeScope(_multiTenantTag, a => _tenantContainerConfiguration(GetCurrentTenant(), a)));
                    return _tenantLifetimeScopes[tenantId];
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                Dispose(true);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            foreach (var scope in _tenantLifetimeScopes)
                scope.Value.Dispose();

            _applicationContainer.Dispose();

            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            lock (_lock)
            {
                return new ValueTask(new Task(async () =>
                {
                    foreach (var scope in _tenantLifetimeScopes)
                        await scope.Value.DisposeAsync();
                    await _applicationContainer.DisposeAsync();
                }));
            }
        }

        public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning
        {
            add {
                foreach (var scope in _tenantLifetimeScopes)
                    scope.Value.ChildLifetimeScopeBeginning += value;
            }
            remove
            {
                foreach (var scope in _tenantLifetimeScopes)
                    scope.Value.ChildLifetimeScopeBeginning -= value;
            }
        }

        public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding
        {
            add
            {
                foreach (var scope in _tenantLifetimeScopes)
                    scope.Value.CurrentScopeEnding += value;
            }
            remove
            {
                foreach (var scope in _tenantLifetimeScopes)
                    scope.Value.CurrentScopeEnding -= value;
            }
        }

        public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning
        {
            add
            {
                foreach (var scope in _tenantLifetimeScopes)
                    scope.Value.ResolveOperationBeginning += value;
            }
            remove
            {
                foreach (var scope in _tenantLifetimeScopes)
                    scope.Value.ResolveOperationBeginning -= value;
            }
        }

        public object Tag => GetCurrentTenantScope().Tag;

        public ILifetimeScope BeginLifetimeScope()
        {
            return GetTenantScope(GetCurrentTenant()?.Id).BeginLifetimeScope();
        }

        public ILifetimeScope BeginLifetimeScope(object tag)
        {            
            return GetTenantScope(GetCurrentTenant()?.Id).BeginLifetimeScope(tag);
        }

        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return GetTenantScope(GetCurrentTenant()?.Id).BeginLifetimeScope(configurationAction);
        }

        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
        {
            return GetTenantScope(GetCurrentTenant()?.Id).BeginLifetimeScope(tag, configurationAction);
        }

        public object ResolveComponent(ResolveRequest request)
        {
            return GetTenantScope(GetCurrentTenant()?.Id).ResolveComponent(request);
        }

        public DiagnosticListener DiagnosticSource => _applicationContainer.DiagnosticSource;

        public IDisposer Disposer => GetTenantScope(GetCurrentTenant()?.Id).Disposer;

        public IComponentRegistry ComponentRegistry => GetTenantScope(GetCurrentTenant()?.Id).ComponentRegistry;
    }
}
