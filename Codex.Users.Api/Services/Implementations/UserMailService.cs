using Codex.Models.Users;
using Codex.Users.Api.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Codex.Core.RazorHelpers.Interfaces;
using Mailjet.Client;
using Newtonsoft.Json.Linq;
using System;
using Codex.Users.Api.Models;
using Codex.Tenants.Framework.Utils;
using Codex.Core.Cache;
using Codex.Models.Tenants;
using Dapr.Client;
using Codex.Core.Models;
using System.Web;

namespace Codex.Users.Api.Services.Implementations
{
    public class UserMailService : IUserMailService
    {
        private readonly ILogger<UserMailService> _logger;
        private readonly DaprClient _daprClient;
        private readonly IUserService _userService;
        private readonly IRazorPartialToStringRenderer _razorPartialToStringRenderer;
        private readonly CacheService<Tenant> _tenantCacheService;

        public UserMailService(
            ILogger<UserMailService> logger,
            DaprClient daprClient,
            IRazorPartialToStringRenderer razorPartialToStringRenderer,
            IUserService userService,
            CacheService<Tenant> tenantCacheService)
        {
            _logger = logger;
            _daprClient = daprClient;
            _razorPartialToStringRenderer = razorPartialToStringRenderer;
            _userService = userService;
            _tenantCacheService = tenantCacheService;
        }

        public async Task SendActivateUserMailAsync(string tenantId, User user)
        {
            var foundUser = _userService.FindOneAsync(user.Id!);

            if (foundUser == null)
            {
                _logger.LogError($"User '{user.Id}' not found");
                return;
            }

            var tenant = await TenantTools.SearchTenantByIdAsync(_logger, _tenantCacheService, _daprClient, tenantId);

            var secretValues = await _daprClient.GetSecretAsync(ConfigConstant.CodexKey, ConfigConstant.BackOfficeUrl);
            var backOfficeUrl = secretValues[ConfigConstant.BackOfficeUrl];

            UriBuilder uriBuilder = new(scheme: "https", backOfficeUrl, 443, $"activate/{user.Id}");
            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["activationCode"] = user.ActivationCode;
            uriBuilder.Query = parameters.ToString();
            string activationLink = uriBuilder.Uri.ToString();

            UserNameActivationModel model = new(tenant, user, activationLink);
            string data = await _razorPartialToStringRenderer.RenderPartialToStringAsync("/Templates/_ActivateUserTemplate.cshtml", model);

            _logger.LogInformation($"data => {data}");

            MailjetClient mailjetClient = new("3a98963ff6302e5ad43bba205bc07d31", "46dde372445248e4847cd908de3e16ba");
            MailjetRequest request = new MailjetRequest
            {
                Resource = Mailjet.Client.Resources.SendV31.Resource
            }
            //.Property(Mailjet.Client.Resources.Send.To,  new List<Recipient>() { "fortin.guillaume@gmail.com" })
            .Property(Mailjet.Client.Resources.Send.Messages, new JArray {
                new JObject {
                    { "From", new JObject {
                        {"Email", "thetyne@live.fr"},
                        {"Name", "Codex"}
                    } },
                    { "To", new JArray {
                        new JObject {
                            {"Email", "fortin.guillaume@gmail.com"},
                            {"Name", "NitrofCG"}
                        }
                    } },
                    {"Subject", "codex test"},
                    {"HTMLPart", data}
                }
            });


            var response = await mailjetClient.PostAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(string.Format("Total: {0}, Count: {1}\n", response.GetTotal(), response.GetCount()));
                Console.WriteLine(response.GetData());
            }
            else
            {
                Console.WriteLine(string.Format("StatusCode: {0}\n", response.StatusCode));
                Console.WriteLine(string.Format("ErrorInfo: {0}\n", response.GetErrorInfo()));
                Console.WriteLine(response.GetData());
            }
        }
    }
}
