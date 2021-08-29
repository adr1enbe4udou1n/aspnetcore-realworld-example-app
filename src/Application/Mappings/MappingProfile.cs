using Application.Auth;
using AutoMapper;
using Domain.Entities;
using System;
using System.Linq;
using System.Reflection;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, CurrentUser>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Name));
        }
    }
}