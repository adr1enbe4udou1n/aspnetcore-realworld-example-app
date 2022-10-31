using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using Application.Features.Auth.Commands;
using Application.Features.Auth.Queries;
using Application.Features.Comments.Commands;
using Application.Features.Comments.Queries;
using Application.Features.Profiles.Queries;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        User? currentUser = null;

        CreateMap<User, UserDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Name))
            .AfterMap<SetTokenAction>();

        CreateMap<NewUserDTO, User>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username))
            .AfterMap<NewUserAction>();

        CreateMap<UpdateUserDTO, User>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Username))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        ;

        CreateMap<User, ProfileDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Following, opt => opt.MapFrom(src => currentUser != null ? currentUser.IsFollowing(src) : false))
            .AfterMap<SetIsFollowingAction>();

        CreateMap<NewArticleDTO, Article>()
            .AfterMap<NewArticleAction>();

        CreateMap<UpdateArticleDTO, Article>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Article, ArticleDTO>()
            .ForMember(dest => dest.TagList, opt => opt.MapFrom(src => src.Tags
                .Select(t => t.Tag.Name)
                .OrderBy(t => t)
            ))
            .ForMember(dest => dest.Favorited, opt => opt.MapFrom(
                src => currentUser != null ? currentUser.HasFavorite(src) : false)
            )
            .ForMember(dest => dest.FavoritesCount, opt => opt.MapFrom(src => src.FavoredUsers.Count))
            .AfterMap<SetHasFavoritedAction>();

        CreateMap<NewCommentDTO, Comment>()
            .AfterMap<NewCommentAction>();

        CreateMap<Comment, CommentDTO>();
    }
}