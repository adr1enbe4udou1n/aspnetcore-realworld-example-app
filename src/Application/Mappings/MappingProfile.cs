using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using Application.Features.Profiles.Queries;
using Application.Interfaces;
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
            CreateMap<User, CurrentUserDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Token, opt => opt.MapFrom<JwtTokenResolver>());

            CreateMap<RegisterDTO, User>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Password, opt => opt.MapFrom<PasswordHashResolver>());

            CreateMap<User, ProfileDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Following, opt => opt.MapFrom<FollowingResolver>());

            CreateMap<ArticleCreateDTO, Article>()
                .ForMember(dest => dest.Slug, opt => opt.MapFrom<SlugResolver>())
                .ForMember(dest => dest.Author, opt => opt.MapFrom<AuthorResolver>())
                .ForMember(dest => dest.Tags, opt => opt.MapFrom<TagsResolver>());
            CreateMap<Article, ArticleDTO>()
                .ForMember(dest => dest.TagList, opt => opt.MapFrom(src => src.Tags.Select(t => t.Tag.Name)));
            CreateMap<User, AuthorDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Name));
        }
    }
}