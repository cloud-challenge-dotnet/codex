using Codex.BackOffice.Services.Users.Implementations;
using Codex.Web.Services.Tools.Implementations;
using Codex.Web.Services.Tools.Interfaces;
using Codex.Web.Services.Users.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Codex.BackOffice
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services
                .AddScoped<IApplicationData, ApplicationData>()
                .AddScoped<IHttpManager, HttpManager>()
                .AddScoped<IAuthenticationService, AuthenticationService>()
                .AddScoped<IAlertService, AlertService>();

            builder.Services.AddScoped(sp =>
            {
                var apiUrl = new Uri(builder.Configuration["apiUrl"]);

                return new HttpClient { BaseAddress = apiUrl };
            });

            await builder.Build().RunAsync();
        }
    }
}
