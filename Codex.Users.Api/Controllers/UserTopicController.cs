using Codex.Core.Models;
using Codex.Models.Users;
using Dapr;
using Dapr.Client;
using Dapr.Client.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Codex.Users.Api.Controllers
{
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
            User? user = topicData.Data;
            if (!string.IsNullOrWhiteSpace(user?.Id))
            {
                _logger.LogInformation($"Receive send activation user mail topic, user id: {user.Id}");
                // TODO Add api key ADMIN from global tenant and add tenant Id to header
                await _daprClient.InvokeMethodAsync("userapi", $"UserMail/activation",
                    data: user,
                    httpExtension: new HTTPExtension() { 
                        Verb = HTTPVerb.Post,
                        Headers = new Dictionary<string, string>(){
                            { "tenantId", topicData.TenantId }
                        }
                    }
                );
            }
            else
            {
                _logger.LogWarning($"Receive user topic without id");
            }
            return Ok();
        }
    }
}
