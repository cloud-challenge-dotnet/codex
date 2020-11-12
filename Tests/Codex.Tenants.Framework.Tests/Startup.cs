using Codex.Tests.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace Codex.Tenants.Framework.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Fixture, Fixture>();
        }
    }
}
