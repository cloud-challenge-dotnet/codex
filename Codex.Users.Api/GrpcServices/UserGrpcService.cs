using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Codex.Core.Security;
using Codex.Models.Roles;
using Codex.Models.Users;
using Codex.Tenants.Framework;
using Codex.Users.Api.Services.Interfaces;
using CodexGrpc.Users;
using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace Codex.Users.Api.GrpcServices;

public class UserGrpcService : UserService.UserServiceBase
{
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public UserGrpcService(
        IMapper mapper,
        IUserService userService)
    {
        _mapper = mapper;
        _userService = userService;
    }

    
    [TenantAuthorize(Roles = "TENANT_MANAGER,USER")]
    public override async Task<UserModel> FindOne(FindOneUserRequest request, ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        string? contextUserId = httpContext.GetUserId();
        if (!httpContext.User.IsInRole(RoleConstant.TenantManager) && contextUserId != request.Id)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, request.Id));
        }

        var user = await _userService.FindOneAsync(request.Id);

        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, request.Id));
        }

        user = OffendUserFields(httpContext, user);

        return _mapper.Map<UserModel>(user);
    }

    [TenantAuthorize(Roles = RoleConstant.TenantManager)]
    public override async Task<UserListResponse> FindAll(FindAllUserRequest request, ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        
        var userCriteria = _mapper.Map<UserCriteria>(request.Criteria);
        
        var users = await _userService.FindAllAsync(userCriteria);

        UserListResponse response = new();
        response.Users.AddRange(users.Select(it => _mapper.Map<UserModel>(OffendUserFields(httpContext, it))));
        return response;
    }

    [TenantAuthorize(Roles = RoleConstant.TenantManager)]
    public override async Task<UserModel> Create(UserModel request, ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        
        var userCreator = _mapper.Map<UserCreator>(request);
        
        string? tenantId = httpContext.GetTenant()?.Id;
        var user = await _userService.CreateAsync(tenantId!, userCreator);
        
        user = OffendUserFields(httpContext, user);

        return _mapper.Map<UserModel>(user);
    }

    [TenantAuthorize(Roles = "TENANT_MANAGER,USER")]
    public override async Task<UserModel> Update(UserModel request, ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        
        var user = _mapper.Map<User>(request);
        
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            throw new ArgumentNullException(nameof(UserModel.Id), "Invalid user id");
        }
        
        string? contextUserId = httpContext.GetUserId();
        if (!httpContext.User.IsInRole(RoleConstant.TenantManager) && contextUserId != user.Id)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, request.Id));
        }

        User? userResult;
        if (!httpContext.User.IsInRole(RoleConstant.TenantManager) && contextUserId == user.Id)
        {
            userResult = await _userService.FindOneAsync(user.Id);
            if (userResult == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, request.Id));
            }
            user = user with
            { // current user without TENANT_MANAGER Role can't update all user fields
                PasswordHash = null,
                ActivationCode = null,
                ActivationValidity = null,
                Roles = userResult.Roles,
                Active = userResult.Active
            };
        }

        userResult = await _userService.UpdateAsync(user);
        if (userResult == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, user.Id));
        }

        userResult = OffendUserFields(httpContext, userResult);

        return _mapper.Map<UserModel>(userResult);
    }

    [TenantAuthorize(Roles = "TENANT_MANAGER,USER")]
    public override async Task<UserModel> UpdatePassword(UpdatePasswordRequest request, ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        
        string? contextUserId = httpContext.GetUserId();
        if (!httpContext.User.IsInRole(RoleConstant.TenantManager) && contextUserId != request.UserId)
        {
            throw new RpcException(new Status(StatusCode.Unauthenticated, request.UserId));
        }

        var user = await _userService.UpdatePasswordAsync(request.UserId, request.Password);

        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, request.UserId));
        }

        user = OffendUserFields(httpContext, user);

        return _mapper.Map<UserModel>(user);
    }

    public override async Task<UserModel> ActivateUser(ActivateUserRequest request, ServerCallContext context)
    {
        var httpContext = context.GetHttpContext();
        
        var user = await _userService.FindOneAsync(request.UserId);
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, request.UserId));
        }

        user = await _userService.ActivateUserAsync(user, request.ActivationCode);
        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, request.UserId));
        }

        user = OffendUserFields(httpContext, user);

        return _mapper.Map<UserModel>(user);
    }

    private static User OffendUserFields(HttpContext httpContext, User user)
    {
        if (!httpContext.User.IsInRole(RoleConstant.TenantManager))
        {
            return user with
            {
                ActivationCode = null,
                ActivationValidity = null,
                PasswordHash = null
            };
        }
        return user;
    }
}