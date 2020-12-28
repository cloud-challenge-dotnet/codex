using Codex.Core.Cache;
using Codex.Core.Interfaces;
using Codex.Core.Models;
using Codex.Core.Models.Mail;
using Codex.Core.RazorHelpers.Interfaces;
using Codex.Models.Users;
using Codex.Tenants.Framework.Utils;
using Codex.Users.Api.Models;
using Codex.Users.Api.Services.Interfaces;
using Dapr.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Web;

namespace Codex.Users.Api.Services.Implementations
{
    public class UserMailService : IUserMailService
    {
        private readonly ILogger<UserMailService> _logger;
        private readonly DaprClient _daprClient;
        private readonly IUserService _userService;
        private readonly IMailService _mailService;
        private readonly IRazorPartialToStringRenderer _razorPartialToStringRenderer;
        private readonly TenantCacheService _tenantCacheService;

        public UserMailService(
            ILogger<UserMailService> logger,
            DaprClient daprClient,
            IRazorPartialToStringRenderer razorPartialToStringRenderer,
            IUserService userService,
            IMailService mailService,
            TenantCacheService tenantCacheService)
        {
            _logger = logger;
            _daprClient = daprClient;
            _razorPartialToStringRenderer = razorPartialToStringRenderer;
            _userService = userService;
            _mailService = mailService;
            _tenantCacheService = tenantCacheService;
        }

        public async Task SendActivateUserMailAsync(string tenantId, User user)
        {
            var foundUser = await _userService.FindOneAsync(user.Id!);

            if (foundUser == null)
            {
                _logger.LogError($"User '{user.Id}' not found");
                return;
            }

            var tenant = await TenantTools.SearchTenantByIdAsync(_logger, _tenantCacheService, _daprClient, tenantId);

            var secretValues = await _daprClient.GetSecretAsync(ConfigConstant.CodexKey, ConfigConstant.BackOfficeUrl);
            var backOfficeUrl = secretValues[ConfigConstant.BackOfficeUrl];

            secretValues = await _daprClient.GetSecretAsync(ConfigConstant.CodexKey, ConfigConstant.SenderEmail);
            var mailSenderEmail = secretValues[ConfigConstant.SenderEmail];

            UriBuilder uriBuilder = new(scheme: "https", backOfficeUrl, 443, $"activate/{user.Id}");
            var parameters = HttpUtility.ParseQueryString(string.Empty);
            parameters["activationCode"] = user.ActivationCode;
            uriBuilder.Query = parameters.ToString();
            string activationLink = uriBuilder.Uri.ToString();

            UserNameActivationModel model = new(tenant, user, activationLink);

            string subject = "Activation de votre compte";
            string htmlPart = await _razorPartialToStringRenderer.RenderPartialToStringAsync("/Templates/_ActivateUserTemplate.cshtml", model);

            await _mailService.SendEmailAsync(new Message(
                From: new(Email: mailSenderEmail, Name: tenant.Name),
                To: new()
                {
                    new(user.Email, user.Login)
                },
                Subject: subject,
                HtmlPart: htmlPart
            ));
        }
    }
}
