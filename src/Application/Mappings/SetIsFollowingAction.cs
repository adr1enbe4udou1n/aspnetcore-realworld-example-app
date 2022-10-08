using Application.Features.Profiles.Queries;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class SetIsFollowingAction : IMappingAction<User, ProfileDTO>
{
    private readonly ICurrentUser _currentUser;

    public SetIsFollowingAction(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public void Process(User source, ProfileDTO destination, ResolutionContext context)
    {
        destination.Following = _currentUser.User != null ? _currentUser.User.IsFollowing(source) : false;
    }
}