using System.Linq;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class FavoriteResolver : IValueResolver<Article, object, bool>
{
    private readonly ICurrentUser _currentUser;

    public FavoriteResolver(ICurrentUser? currentUser = null)
    {
        _currentUser = currentUser;
    }

    bool IValueResolver<Article, object, bool>.Resolve(Article source, object destination, bool destMember, ResolutionContext context)
    {
        return _currentUser.IsAuthenticated && _currentUser.User.HasFavorite(source);
    }
}
