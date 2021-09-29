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

            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Token, opt => opt.MapFrom<JwtTokenResolver>());

            CreateMap<NewUserDTO, User>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username));

            CreateMap<UpdateUserDTO, User>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            ;

            CreateMap<User, ProfileDTO>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Following, opt => opt.MapFrom(src => currentUser != null ? src.IsFollowedBy(currentUser) : false))
                .ForMember(dest => dest.Following, opt => opt.MapFrom<FollowingResolver>());

            CreateMap<NewArticleDTO, Article>()
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => currentUser.Id));

            CreateMap<UpdateArticleDTO, Article>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Article, ArticleDTO>()
                .ForMember(dest => dest.TagList, opt => opt.MapFrom(src => src.Tags.Select(t => t.Tag.Name)))
                .ForMember(dest => dest.Favorited, opt => opt.MapFrom(
                    src => currentUser != null ? src.FavoredUsers.Any(f => f.User.Id == currentUser.Id) : false)
                )
                .ForMember(dest => dest.Favorited, opt => opt.MapFrom<FavoriteResolver>())
                .ForMember(dest => dest.FavoritesCount, opt => opt.MapFrom(src => src.FavoredUsers.Count));

            CreateMap<NewCommentDTO, Comment>()
                .ForMember(dest => dest.AuthorId, opt => opt.MapFrom(src => currentUser.Id));

            CreateMap<Comment, CommentDTO>();
        }
    }
}
