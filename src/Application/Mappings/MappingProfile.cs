using System;
using System.Linq;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using Application.Features.Profiles.Queries;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            User currentUser = null;

            CreateMap<User, CurrentUserDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Token, opt => opt.MapFrom<JwtTokenResolver>());

            CreateMap<RegisterDTO, User>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username));

            CreateMap<User, ProfileDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Following, opt => opt.MapFrom<FollowingResolver>());

            CreateMap<ArticleCreateDTO, Article>()
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => currentUser.Id));
            CreateMap<Article, ArticleDTO>()
                .ForMember(dest => dest.TagList, opt => opt.MapFrom(src => src.Tags.Select(t => t.Tag.Name)))
                .ForMember(dest => dest.Favorited, opt => opt.MapFrom(
                    src => currentUser != null ? src.FavoredUsers.Any(f => f.User.Id == currentUser.Id) : false)
                )
                .ForMember(dest => dest.Favorited, opt => opt.MapFrom<FavoriteResolver>())
                .ForMember(dest => dest.FavoritesCount, opt => opt.MapFrom(src => src.FavoredUsers.Count()));
            CreateMap<User, AuthorDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Name));

            CreateMap<CommentCreateDTO, Comment>()
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => currentUser.Id));
            CreateMap<Comment, CommentDTO>();
        }
    }
}
