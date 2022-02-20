using Codex.Core.Models;
using Codex.Models.Users;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace Codex.Users.Api.Controllers;

public class UserTopicController : ControllerBase
{
    private readonly ILogger<UserTopicController> _logger;
    private readonly DaprClient _daprClient;

    public UserTopicController(
        ILogger<UserTopicController> logger,
        DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    [Topic(ConfigConstant.CodexPubSubName, TopicConstant.SendActivationUserMail)]
    [HttpPost]
    [Route(TopicConstant.SendActivationUserMail)]
    public async Task<IActionResult> ProcessSendActivationUserMailTopic([FromBody] TopicData<User> topicData)
    {
        User user = topicData.Data;
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            _logger.LogInformation("Receive send activation user mail topic, user id: {UserId}", user.Id);

            var request = _daprClient.CreateInvokeMethodRequest(ApiNameConstant.UserApi, "UserMail/activation", user);
            request.Method = HttpMethod.Post;
            request.Headers.Add(HttpHeaderConstant.TenantId, topicData.TenantId);
            await _daprClient.InvokeMethodAsync(request);
        }
        else
        {
            _logger.LogWarning($"Receive user topic without id");
        }
        return Ok();
    }
}