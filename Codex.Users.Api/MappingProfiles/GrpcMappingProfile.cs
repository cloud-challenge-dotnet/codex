﻿using AutoMapper;
using Codex.Models.Tenants;
using Codex.Models.Users;
using CodexGrpc.Tenants;
using CodexGrpc.Users;

namespace Codex.Users.Api.MappingProfiles;

public class GrpcMappingProfile : Profile
{
    public GrpcMappingProfile()
    {
        // Project GRPC mapping
        CreateMap<TenantModel, Tenant>().ReverseMap();
        
        // Authenticate service
        CreateMap<AuthModel, Auth>().ReverseMap();
        
        // User service
        CreateMap<UserModel, UserCreator>().ReverseMap();
        CreateMap<UserModel, User>().ReverseMap();
        CreateMap<UserCriteriaModel, UserCriteria>().ReverseMap();
    }
}