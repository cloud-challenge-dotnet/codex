using Codex.BackOffice.Models;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Codex.BackOffice.Extensions
{
    public static class WebAssemblyHostExtension
    {
        public async static Task SetDefaultCultureAsync(this WebAssemblyHost host)
        {
            CultureInfo culture;
            try
            {
                var jsInterop = host.Services.GetRequiredService<IJSRuntime>();
                var result = await jsInterop.InvokeAsync<string>("selectedCulture.get");

                if (string.IsNullOrWhiteSpace(result))
                {
                    result = await jsInterop.InvokeAsync<string>("browserCulture.get");
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    var foundCulture = AppCultures.SupportedCultures.FirstOrDefault(x => x.Name == result || x.TwoLetterISOLanguageName.ToLower() == result.ToLower());
                    culture = foundCulture ?? new CultureInfo(result);
                }
                else
                    culture = new CultureInfo("en-US");
            }
            catch (Exception exception)
            {
                culture = new CultureInfo("en-US");
                Console.WriteLine("Unable to get user language : " + exception.Message);
            }

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }
}
