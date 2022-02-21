using System.Threading.Tasks;
using AutoMapper;
using Codex.Core.Tools.AutoMapper;
using Codex.Models.Users;
using Codex.Tests.Framework;
using Codex.Users.Api.GrpcServices;
using Codex.Users.Api.MappingProfiles;
using Codex.Users.Api.Services.Interfaces;
using Moq;
using Xunit;

namespace Codex.Users.Api.Tests.GrpcServices;

public class AuthenticationGrpcServiceIt : IClassFixture<Fixture>
{
    private readonly IMapper _mapper;

    public AuthenticationGrpcServiceIt()
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
    public async Task Authenticate()
    {
        string tenantId = "global";
        var authenticationService = new Mock<IAuthenticationService>();

        authenticationService.Setup(s => s.AuthenticateAsync(It.IsAny<UserLogin>()))
            .Returns(Task.FromResult(new Auth(Id: "ID1", Login: "Login", Token: "5634534564")));

        AuthenticationGrpcService authenticationGrpcService = new(_mapper, authenticationService.Object);
        
        var serverCallContext = Fixture.CreateServerCallContext(
            Fixture.CreateHttpContext(tenantId)
        );

        var auth = await authenticationGrpcService.Authenticate(
            new(),
            serverCallContext
        );

        authenticationService.Verify(v => v.AuthenticateAsync(It.IsAny<UserLogin>()), Times.Once);

        Assert.NotNull(auth);
        Assert.Equal("ID1", auth.Id);
        Assert.Equal("Login", auth.Login);
        Assert.Equal("5634534564", auth.Token);
    }
}