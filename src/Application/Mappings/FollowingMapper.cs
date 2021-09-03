using System.Linq;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings
{
    public class FollowingMapper : IValueResolver<User, object, bool>
    {
        private readonly ICurrentUser _currentUser;

        public FollowingMapper(ICurrentUser currentUser = null)
        {
            _currentUser = currentUser;
        }

        bool IValueResolver<User, object, bool>.Resolve(User source, object destination, bool destMember, ResolutionContext context)
        {
            return _currentUser.IsAuthenticated && _currentUser.User.IsFollowing(source);
        }
    }
}