using Codex.Core.Interfaces;
using Codex.Core.Models;
using Codex.Core.Models.Mail;
using Codex.Core.Resources;
using Codex.Models.Exceptions;
using Dapr.Client;
using Mailjet.Client;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Codex.Core.Implementations;

[ExcludeFromCodeCoverage]
public class MailJetMailService : IMailService
{
    private readonly ILogger<MailJetMailService> _logger;
    private readonly IStringLocalizer<CoreResource> _sl;
    private readonly DaprClient _daprClient;

    public MailJetMailService(
        ILogger<MailJetMailService> logger,
        IStringLocalizer<CoreResource> sl,
        DaprClient daprClient)
    {
        _logger = logger;
        _sl = sl;
        _daprClient = daprClient;
    }

    public async Task SendEmailAsync(Message message)
    {
        var secretValues = await _daprClient.GetSecretAsync(ConfigConstant.CodexKey, ConfigConstant.MailJetApiKey);
        var mailJetApiKey = secretValues[ConfigConstant.MailJetApiKey];
        secretValues = await _daprClient.GetSecretAsync(ConfigConstant.CodexKey, ConfigConstant.MailJetSecretKey);
        var mailJetSecretKey = secretValues[ConfigConstant.MailJetSecretKey];

        JArray toArray = new();
        message.To.ForEach(m => toArray.Add(new JObject {
            {"Email", m.Email},
            {"Name", m.Name}
        }));


        MailjetClient mailjetClient = new(mailJetApiKey, mailJetSecretKey);
        MailjetRequest request = new MailjetRequest
            {
                Resource = Mailjet.Client.Resources.SendV31.Resource
            }
            .Property(Mailjet.Client.Resources.Send.Messages, new JArray {
                new JObject {
                    { "From", new JObject {
                        {"Email", message.From.Email},
                        {"Name", message.From.Name}
                    } },
                    { "To", toArray },
                    {"Subject", message.Subject },
                    {"TextPart", message.TextPart},
                    {"HTMLPart", message.HtmlPart}
                }
            });

        var response = await mailjetClient.PostAsync(request);
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Mail sent. Subject: {Subject}", message.Subject);
        }
        else
        {
            throw new SendMailException(_sl[CoreResource.UnableToSendEmailWithMailjetProvider], "SEND_MAIL_ERROR");
        }
    }
}