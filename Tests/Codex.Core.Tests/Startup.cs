using Codex.Tests.Framework;
using Microsoft.Extensions.DependencyInjection;

namespace Codex.Core.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Fixture, Fixture>();
        }
    }
}
