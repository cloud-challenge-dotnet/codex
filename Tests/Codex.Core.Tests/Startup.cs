using Codex.Tests.Framework;

namespace Codex.Core.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            services.AddSingleton<Fixture, Fixture>();
        }
    }
}
