using Codex.Models.Security;
using Codex.Security.Api.Repositories.Interfaces;
using Codex.Tests.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Codex.Security.Api.Tests.Repositories
{
    public class ApiKeyRepositoryIT : IClassFixture<DbFixture>
    {
        private readonly DbFixture _fixture;
        private readonly IApiKeyRepository _apiKeyRepository;

        public ApiKeyRepositoryIT(DbFixture fixture)
        {
            _fixture = fixture;
            _apiKeyRepository = _fixture.Services.GetService<IApiKeyRepository>()!;
        }

        [Fact]
        public async Task FindAll()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/apiKeys.json");

            ApiKeyCriteria apiKeyCriteria = new();
            var apiKeyList = await _apiKeyRepository.FindAllAsync(apiKeyCriteria);

            Assert.NotNull(apiKeyList);
            Assert.Equal(2, apiKeyList.Count);

            //Not updated
            Assert.Equal("global.1", apiKeyList[0].Id);
            Assert.Equal("global.2", apiKeyList[1].Id);
        }

        [Fact]
        public async Task Insert()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/apiKeys.json");

            var apiKey = await _apiKeyRepository.InsertAsync(new("global.3", "apiKey3", new() { "USER" }));

            Assert.NotNull(apiKey);
            Assert.Equal("global.3", apiKey.Id);
            Assert.Equal("apiKey3", apiKey.Name);
            Assert.Single(apiKey.Roles);
            Assert.Contains("USER", apiKey.Roles);
        }

        [Fact]
        public async Task Update()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/apiKeys.json");

            var apiKeys = await _apiKeyRepository.UpdateAsync(
                new("global.1", "Admin Api Key", new() { "TENANT_MANAGER" })
            );

            Assert.NotNull(apiKeys);
            Assert.Equal("Admin Api Key", apiKeys!.Name);
            Assert.NotNull(apiKeys!.Roles);
            Assert.Single(apiKeys!.Roles);
            Assert.Equal("TENANT_MANAGER", apiKeys!.Roles[0]);

            //Not updated
            Assert.Equal("global.1", apiKeys!.Id);
        }

        [Fact]
        public async Task Delete()
        {
            await _fixture.UseDataSetAsync(locations: @"Resources/apiKeys.json");

            await _apiKeyRepository.DeleteAsync("global.1");

            bool exist = await _apiKeyRepository.ExistsByIdAsync("global.1");

            Assert.False(exist);
        }
    }
}
