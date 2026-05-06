using AutoMapper;
using UserMicroservice.Models;

namespace UserMicroservice.Config;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        CreateMap<UserModel, UserViewModel>();

    }
}