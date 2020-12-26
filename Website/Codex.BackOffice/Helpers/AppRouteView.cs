using Codex.Web.Services.Tools.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Net;

namespace Codex.BackOffice.Helpers
{
    public class AppRouteView : RouteView
    {
        [Inject]
        public NavigationManager? NavigationManager { get; set; }

        [Inject]
        public IApplicationData? ApplicationData { get; set; }

        protected override void Render(RenderTreeBuilder builder)
        {
            var authorize = Attribute.GetCustomAttribute(RouteData.PageType, typeof(AuthorizeAttribute)) != null;
            if (authorize && ApplicationData!.Auth == null)
            {
                var uri = new Uri(NavigationManager!.Uri);
                var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
                string? returnUrl;
                if ((returnUrl = queryDictionary["returnUrl"]) == null)
                {
                    returnUrl = WebUtility.UrlEncode(uri.PathAndQuery);
                }
                NavigationManager.NavigateTo($"account/login?returnUrl={returnUrl}");
            }
            else
            {
                base.Render(builder);
            }
        }
    }
}
