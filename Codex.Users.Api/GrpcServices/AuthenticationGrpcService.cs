using System.Threading.Tasks;
using AutoMapper;
using Codex.Models.Users;
using Codex.Tenants.Framework;
using Codex.Users.Api.Services.Interfaces;
using CodexGrpc.Users;
using Grpc.Core;

namespace Codex.Users.Api.GrpcServices;

public class AuthenticationGrpcService : AuthenticationService.AuthenticationServiceBase
{
    private readonly IMapper _mapper;
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationGrpcService(
        IMapper mapper,
        IAuthenticationService authenticationService)
    {
        _mapper = mapper;
        _authenticationService = authenticationService;
    }

    public override async Task<AuthModel> Authenticate(AuthenticateRequest request, ServerCallContext context)
    {
        UserLogin userLogin = new(
            request.Login,
            request.Password,
            context.GetHttpContext().GetTenant()?.Id ?? ""
        );

        Auth auth = await _authenticationService.AuthenticateAsync(userLogin);

        return _mapper.Map<AuthModel>(auth);
    }
}