using Codex.Tests.Framework;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Codex.Security.Api.Repositories.Interfaces;
using Codex.Models.Security;
using Codex.Security.Api.Services.Implementations;

namespace Codex.Security.Api.Tests.Services
{
    public class ApiKeyServiceIT : IClassFixture<Fixture>
    {
        public ApiKeyServiceIT()
        {
        }

        [Fact]
        public async Task FindAll() 
        {
            var apiKeyRepository = new Mock<IApiKeyRepository>();
            ApiKeyCriteria apiKeyCriteria = new();

            apiKeyRepository.Setup(x => x.FindAllAsync(It.IsAny<ApiKeyCriteria>())).Returns(
                Task.FromResult(new List<ApiKey>()
                {
                    new(){ Id = "Id1" },
                    new(){ Id = "Id2" }
                })
            );

            var apiKeyService = new ApiKeyService(apiKeyRepository.Object);

            var apiKeyList = await apiKeyService.FindAllAsync(apiKeyCriteria);

            Assert.NotNull(apiKeyList);
            Assert.Equal(2, apiKeyList.Count);

            Assert.Equal("Id1", apiKeyList[0].Id);
            Assert.Equal("Id2", apiKeyList[1].Id);

            apiKeyRepository.Verify(x => x.FindAllAsync(It.IsAny<ApiKeyCriteria>()), Times.Once);
        }

        [Fact]
        public async Task FindOne()
        {
            var apiKeyRepository = new Mock<IApiKeyRepository>();

            string apiKeyId = "Id1";

            apiKeyRepository.Setup(x => x.FindOneAsync(It.IsAny<string>())).Returns(
                Task.FromResult((ApiKey?)new ApiKey { Id = "Id1" })
            );

            var apiKeyService = new ApiKeyService(apiKeyRepository.Object);

            var apiKey = await apiKeyService.FindOneAsync(apiKeyId);

            Assert.NotNull(apiKey);
            Assert.Equal(apiKeyId, apiKey!.Id);

            apiKeyRepository.Verify(x => x.FindOneAsync(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Create()
        {
            var apiKeyRepository = new Mock<IApiKeyRepository>();

            var apiKeyCreator = new ApiKey() { Id = "Id1", Name = "ApiKey 1" };

            apiKeyRepository.Setup(x => x.InsertAsync(It.IsAny<ApiKey>())).Returns(
                Task.FromResult(new ApiKey() { Id = "Id1", Name = "ApiKey 1" })
            );

            var apiKeyService = new ApiKeyService(apiKeyRepository.Object);

            var apiKey = await apiKeyService.CreateAsync(apiKeyCreator);

            Assert.NotNull(apiKey);
            Assert.NotNull(apiKey.Id);
            Assert.Equal(apiKeyCreator.Id!, apiKey.Id!);
            Assert.Equal(apiKeyCreator.Name!, apiKey.Name);

            apiKeyRepository.Verify(x => x.InsertAsync(It.IsAny<ApiKey>()), Times.Once);
        }

        [Fact]
        public async Task Create_With_Null_Id()
        {
            var apiKeyRepository = new Mock<IApiKeyRepository>();

            var apiKeyCreator = new ApiKey() { Id = null, Name = "ApiKey 1" };

            apiKeyRepository.Setup(x => x.InsertAsync(It.IsAny<ApiKey>())).Returns(
                Task.FromResult(apiKeyCreator with { Id = "123456" })
            );

            var apiKeyService = new ApiKeyService(apiKeyRepository.Object);

            var apiKey = await apiKeyService.CreateAsync(apiKeyCreator);

            Assert.NotNull(apiKey);
            Assert.NotNull(apiKey.Id);
            Assert.Equal("123456", apiKey.Id);
            Assert.Equal(apiKeyCreator.Name!, apiKey.Name);

            apiKeyRepository.Verify(x => x.InsertAsync(It.IsAny<ApiKey>()), Times.Once);
        }

        [Fact]
        public async Task Create_With_Empty_Id()
        {
            var apiKeyRepository = new Mock<IApiKeyRepository>();

            var apiKeyCreator = new ApiKey() { Id = "", Name = "ApiKey 1" };

            apiKeyRepository.Setup(x => x.InsertAsync(It.IsAny<ApiKey>())).Returns(
                Task.FromResult(apiKeyCreator with { Id = "123456" })
            );

            var apiKeyService = new ApiKeyService(apiKeyRepository.Object);

            var apiKey = await apiKeyService.CreateAsync(apiKeyCreator);

            Assert.NotNull(apiKey);
            Assert.NotNull(apiKey.Id);
            Assert.Equal("123456", apiKey.Id);
            Assert.Equal(apiKeyCreator.Name!, apiKey.Name);

            apiKeyRepository.Verify(x => x.InsertAsync(It.IsAny<ApiKey>()), Times.Once);
        }

        [Fact]
        public async Task Update()
        {
            var apiKeyRepository = new Mock<IApiKeyRepository>();

            string apiKeyId = "Id1";
            string apiKeyName = "ApiKey 1";
            var apiKey = new ApiKey() { Id = apiKeyId, Name = apiKeyName };

            apiKeyRepository.Setup(x => x.UpdateAsync(It.IsAny<ApiKey>())).Returns(
                Task.FromResult((ApiKey?)new ApiKey { Id = apiKeyId, Name = apiKeyName })
            );

            var apiKeyService = new ApiKeyService(apiKeyRepository.Object);

            var apiKeyResult = await apiKeyService.UpdateAsync(apiKey);

            Assert.NotNull(apiKeyResult);
            Assert.Equal(apiKeyId, apiKeyResult!.Id);
            Assert.Equal(apiKeyName, apiKeyResult!.Name);

            apiKeyRepository.Verify(x => x.UpdateAsync(It.IsAny<ApiKey>()), Times.Once);
        }

        [Fact]
        public async Task DeleteApiKey()
        {
            var apiKeyRepository = new Mock<IApiKeyRepository>();

            string apiKeyId = "Id1";
            string apiKeyName = "ApiKey 1";
            var apiKey = new ApiKey() { Id = apiKeyId, Name = apiKeyName };

            apiKeyRepository.Setup(x => x.DeleteAsync(It.IsAny<string>())).Returns(
                Task.CompletedTask
            );

            var apiKeyService = new ApiKeyService(apiKeyRepository.Object);

            await apiKeyService.DeleteAsync(apiKeyId);

            apiKeyRepository.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
