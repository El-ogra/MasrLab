using AutoMapper;
using MasrLab.Application.Common.DTOs;
using MasrLab.Domain.Entities.Administrative;

namespace MasrLab.Application.Common.Mappings.Profiles;

public class UsersPermissionsMappingProfile : Profile
{
    public UsersPermissionsMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<Permission, PermissionDto>();
        CreateMap<Permission, PermissionAssignmentDto>();
    }
}
