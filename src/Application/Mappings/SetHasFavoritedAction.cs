using Application.Features.Articles.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;


public class SetHasFavoritedAction : IMappingAction<Article, ArticleDTO>
{
    private readonly ICurrentUser _currentUser;

    public SetHasFavoritedAction(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public void Process(Article source, ArticleDTO destination, ResolutionContext context)
    {
        destination.Favorited = _currentUser.User != null && _currentUser.User.HasFavorite(source);
    }
}