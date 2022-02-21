using AutoMapper;
using Codex.Core.Tools.AutoMapper;
using Codex.Models.Security;
using Codex.Security.Api.Repositories.Interfaces;
using Codex.Security.Api.Repositories.Models;
using Codex.Security.Api.Services.Implementations;
using Codex.Tests.Framework;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Codex.Security.Api.MappingProfiles;
using Xunit;

namespace Codex.Security.Api.Tests.Services;

public class ApiKeyServiceIt : IClassFixture<Fixture>
{
    private readonly IMapper _mapper;

    public ApiKeyServiceIt()
    {
        //auto mapper configuration
        var mockMapper = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.AllowNullDestinationValues = true;
            cfg.AddProfile<CoreMappingProfile>();
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<Codex.Core.MappingProfiles.GrpcMappingProfile>();
            cfg.AddProfile<GrpcMappingProfile>();
        });
        _mapper = mockMapper.CreateMapper();
    }

    [Fact]
    public async Task FindAll()
    {
        var apiKeyRepository = new Mock<IApiKeyRepository>();
        ApiKeyCriteria apiKeyCriteria = new();

        apiKeyRepository.Setup(x => x.FindAllAsync(It.IsAny<ApiKeyCriteria>())).Returns(
            Task.FromResult(new List<ApiKeyRow>()
            {
                new(){ Id = "Id1" },
                new(){ Id = "Id2" }
            })
        );

        var apiKeyService = new ApiKeyService(apiKeyRepository.Object, _mapper);

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
            Task.FromResult((ApiKeyRow?)new ApiKeyRow { Id = "Id1" })
        );

        var apiKeyService = new ApiKeyService(apiKeyRepository.Object, _mapper);

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

        apiKeyRepository.Setup(x => x.InsertAsync(It.IsAny<ApiKeyRow>())).Returns(
            Task.FromResult(new ApiKeyRow() { Id = "Id1", Name = new() { { "en", "ApiKey 1" } } })
        );

        var apiKeyService = new ApiKeyService(apiKeyRepository.Object, _mapper);

        var apiKey = await apiKeyService.CreateAsync(apiKeyCreator);

        Assert.NotNull(apiKey);
        Assert.NotNull(apiKey.Id);
        Assert.Equal(apiKeyCreator.Id!, apiKey.Id!);
        Assert.Equal(apiKeyCreator.Name, apiKey.Name);

        apiKeyRepository.Verify(x => x.InsertAsync(It.IsAny<ApiKeyRow>()), Times.Once);
    }

    [Fact]
    public async Task Create_With_Null_Id()
    {
        var apiKeyRepository = new Mock<IApiKeyRepository>();

        var apiKeyCreator = new ApiKey() { Id = null, Name = "ApiKey 1" };

        apiKeyRepository.Setup(x => x.InsertAsync(It.IsAny<ApiKeyRow>())).Returns(
            Task.FromResult(new ApiKeyRow { Id = "123456", Name = new() { { "en", "ApiKey 1" } } })
        );

        var apiKeyService = new ApiKeyService(apiKeyRepository.Object, _mapper);

        var apiKey = await apiKeyService.CreateAsync(apiKeyCreator);

        Assert.NotNull(apiKey);
        Assert.NotNull(apiKey.Id);
        Assert.Equal("123456", apiKey.Id);
        Assert.Equal(apiKeyCreator.Name, apiKey.Name);

        apiKeyRepository.Verify(x => x.InsertAsync(It.IsAny<ApiKeyRow>()), Times.Once);
    }

    [Fact]
    public async Task Create_With_Empty_Id()
    {
        var apiKeyRepository = new Mock<IApiKeyRepository>();

        var apiKeyCreator = new ApiKey() { Id = "", Name = "ApiKey 1" };

        apiKeyRepository.Setup(x => x.InsertAsync(It.IsAny<ApiKeyRow>())).Returns(
            Task.FromResult(new ApiKeyRow { Id = "123456", Name = new() { { "en", "ApiKey 1" } } })
        );

        var apiKeyService = new ApiKeyService(apiKeyRepository.Object, _mapper);

        var apiKey = await apiKeyService.CreateAsync(apiKeyCreator);

        Assert.NotNull(apiKey);
        Assert.NotNull(apiKey.Id);
        Assert.Equal("123456", apiKey.Id);
        Assert.Equal(apiKeyCreator.Name, apiKey.Name);

        apiKeyRepository.Verify(x => x.InsertAsync(It.IsAny<ApiKeyRow>()), Times.Once);
    }

    [Fact]
    public async Task Update()
    {
        var apiKeyRepository = new Mock<IApiKeyRepository>();

        string apiKeyId = "Id1";
        string apiKeyName = "ApiKey 1";
        var apiKey = new ApiKey() { Id = apiKeyId, Name = apiKeyName };

        apiKeyRepository.Setup(x => x.UpdateAsync(It.IsAny<ApiKeyRow>())).Returns(
            Task.FromResult((ApiKeyRow?)new ApiKeyRow { Id = apiKeyId, Name = new() { { "en", apiKeyName } } })
        );

        var apiKeyService = new ApiKeyService(apiKeyRepository.Object, _mapper);

        var apiKeyResult = await apiKeyService.UpdateAsync(apiKey);

        Assert.NotNull(apiKeyResult);
        Assert.Equal(apiKeyId, apiKeyResult!.Id);
        Assert.Equal(apiKeyName, apiKeyResult.Name);

        apiKeyRepository.Verify(x => x.UpdateAsync(It.IsAny<ApiKeyRow>()), Times.Once);
    }

    [Fact]
    public async Task DeleteApiKey()
    {
        var apiKeyRepository = new Mock<IApiKeyRepository>();

        string apiKeyId = "Id1";

        apiKeyRepository.Setup(x => x.DeleteAsync(It.IsAny<string>())).Returns(
            Task.CompletedTask
        );

        var apiKeyService = new ApiKeyService(apiKeyRepository.Object, _mapper);

        await apiKeyService.DeleteAsync(apiKeyId);

        apiKeyRepository.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Once);
    }
}