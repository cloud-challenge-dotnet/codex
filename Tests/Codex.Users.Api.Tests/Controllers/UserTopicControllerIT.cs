using Codex.Core.Models;
using Codex.Models.Users;
using Codex.Tests.Framework;
using Codex.Users.Api.Controllers;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Users.Api.Tests
{
    public class UserTopicControllerIT : IClassFixture<Fixture>
    {
        public UserTopicControllerIT()
        {
        }

        [Fact]
        public async Task ProcessSendActivationUserMailTopic()
        {
            var logger = new Mock<ILogger<UserTopicController>>();
            var daprClient = new Mock<DaprClient>();

            daprClient.Setup(x => x.GetSecretAsync(It.IsAny<string>(), It.IsAny<string>(),
               It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(
                new Dictionary<string, string>() { { ConfigConstant.MicroserviceApiKey, "" } }
            ));

            daprClient.Setup(x => x.CreateInvokeMethodRequest(It.IsAny<HttpMethod>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<User>()))
                .Returns(new HttpRequestMessage());

            User user = new() { Id = ObjectId.GenerateNewId().ToString() };
            TopicData<User> userTopicData = new(TopicType.Modify, user, "global");

            var topicController = new UserTopicController(
                logger.Object,
                daprClient.Object
            );

            var result = await topicController.ProcessSendActivationUserMailTopic(userTopicData);

            var okResult = Assert.IsType<OkResult>(result);

            daprClient.Verify(x => x.InvokeMethodAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessSendActivationUserMailTopic_with_User_Id_Null()
        {
            var logger = new Mock<ILogger<UserTopicController>>();
            var daprClient = new Mock<DaprClient>();

            User user = new();
            TopicData<User> userTopicData = new(TopicType.Modify, user, "global");

            var topicController = new UserTopicController(
                logger.Object,
                daprClient.Object
            );

            var result = await topicController.ProcessSendActivationUserMailTopic(userTopicData);

            var okResult = Assert.IsType<OkResult>(result);

            daprClient.Verify(x => x.InvokeMethodAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
